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
    
    public partial class detallePedido_GM
    {
        public int detallePedido_id { get; set; }
        public string detallePedido_kmat_zvsk { get; set; }
        public string detallePedido_descripcion { get; set; }
        public int detallePedido_anioModelo { get; set; }
        public string detallePedido_modeloCodigo { get; set; }
        public string detallePedido_colorCodigo { get; set; }
        public int detallePedido_cantidad { get; set; }
        public long detallePedido_pedidoCodigo { get; set; }
    
        public virtual color_vehiculo color_vehiculo { get; set; }
        public virtual modelo_vehiculo modelo_vehiculo { get; set; }
        public virtual pedido_GM pedido_GM { get; set; }
    }
}
