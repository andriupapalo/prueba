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
    
    public partial class vw_kardex_movimientos
    {
        public Nullable<int> sw { get; set; }
        public int id_encabezado { get; set; }
        public int idencabezado { get; set; }
        public string prefijo { get; set; }
        public string tpdoc_nombre { get; set; }
        public Nullable<bool> entrada_salida { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public long numero { get; set; }
        public Nullable<int> id_movimiento_interno { get; set; }
        public long numeroext { get; set; }
        public string prefijoext { get; set; }
        public string nombredocex { get; set; }
        public Nullable<decimal> cantentrada { get; set; }
        public Nullable<decimal> cantsalida { get; set; }
        public Nullable<decimal> costentrada { get; set; }
        public Nullable<decimal> costsalida { get; set; }
        public Nullable<decimal> CostoTotal { get; set; }
        public string codigo { get; set; }
        public int bodega { get; set; }
    }
}
