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
    
    public partial class segmentacion_historico_clientes_crm
    {
        public long id_segmentacion_historico_cliente { get; set; }
        public int id_segmentacion_cliente { get; set; }
        public Nullable<int> id_tercero { get; set; }
        public System.DateTime fecha_actualizacion { get; set; }
        public int resultado_cantidad_vehiculo { get; set; }
        public string resultado_revisiones { get; set; }
        public string resultado_soat { get; set; }
        public string resultado_polizas { get; set; }
        public Nullable<int> id_prospecto { get; set; }
    
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual prospectos prospectos { get; set; }
        public virtual segmentacion segmentacion { get; set; }
    }
}
