namespace Homer_MVC.Models
{
    public class DetalleRepuestoOrdenModel
    {

        public string CodigoReferencia { get; set; }
        public string NombreReferencia { get; set; }
        public decimal PrecioReferencia { get; set; }
        public decimal DescuentoReferencia { get; set; }
        public decimal IvaReferencia { get; set; }
        public decimal CantidadReferencia { get; set; }
        public decimal PrecioTotal { get; set; }

    }
}