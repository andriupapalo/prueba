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
    
    public partial class bitacoraExcepcionFactura
    {
        public int id { get; set; }
        public Nullable<int> id_excepcion { get; set; }
        public Nullable<int> id_Autorizacion { get; set; }
        public Nullable<int> id_Bodega { get; set; }
        public Nullable<int> id_Pedido { get; set; }
        public Nullable<int> id_usuario_solicitante { get; set; }
        public Nullable<int> id_usuario_apobador { get; set; }
        public Nullable<System.DateTime> fecha_aprobacion { get; set; }
    
        public virtual autorizaciones autorizaciones { get; set; }
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual ftipo_excepciones ftipo_excepciones { get; set; }
        public virtual vpedido vpedido { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}