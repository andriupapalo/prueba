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
    
    public partial class vw_browser_pedidos_firme
    {
        public string ref_codigo { get; set; }
        public string ref_descripcion { get; set; }
        public int id_archivo_compra { get; set; }
        public int tipo_compra { get; set; }
        public string descripcion { get; set; }
        public int bodega { get; set; }
        public string sede_nombre { get; set; }
        public System.DateTime fecha { get; set; }
        public string fecha2 { get; set; }
        public string archivo { get; set; }
        public int numero_compra { get; set; }
        public string numero_compra2 { get; set; }
        public string numero_gm { get; set; }
        public Nullable<int> cantidad_pedida { get; set; }
        public Nullable<int> cantidad_recibida { get; set; }
        public Nullable<int> cantidad_pedido { get; set; }
        public Nullable<int> pedidas { get; set; }
        public Nullable<int> recibiendo { get; set; }
        public Nullable<int> recibidos { get; set; }
        public Nullable<int> cancelados { get; set; }
    }
}