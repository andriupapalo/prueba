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
    
    public partial class icb_vehiculo_eventos
    {
        public int evento_id { get; set; }
        public string planmayor { get; set; }
        public Nullable<int> eventoid_licencia { get; set; }
        public System.DateTime eventofec_creacion { get; set; }
        public int eventouserid_creacion { get; set; }
        public Nullable<System.DateTime> eventofec_actualizacion { get; set; }
        public Nullable<int> eventouserid_actualizacion { get; set; }
        public string evento_nombre { get; set; }
        public bool evento_estado { get; set; }
        public string eventorazoninactivo { get; set; }
        public int bodega_id { get; set; }
        public int id_tpevento { get; set; }
        public string evento_observacion { get; set; }
        public System.DateTime fechaevento { get; set; }
        public Nullable<int> ubicacion { get; set; }
        public Nullable<int> terceroid { get; set; }
        public string placa { get; set; }
        public string vin { get; set; }
        public Nullable<int> idtramitador { get; set; }
        public Nullable<int> idtecnico { get; set; }
        public string poliza { get; set; }
        public Nullable<int> idencabezado { get; set; }
        public Nullable<int> cartera_id { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual encab_documento encab_documento { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual icb_tpeventos icb_tpeventos { get; set; }
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual Tipos_Cartera Tipos_Cartera { get; set; }
        public virtual tramitador_vh tramitador_vh { get; set; }
        public virtual ttecnicos ttecnicos { get; set; }
        public virtual ubicacion_bodega ubicacion_bodega { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
