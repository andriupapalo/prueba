using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class OrdenRepuestoModel
    {


        public int NumeroOrden { get; set; }
        public string Bodega { get; set; }
        public string TipoCompra { get; set; }
        public string FormaPago { get; set; }
        public int DiasValidez { get; set; }
        public string Proveedor { get; set; }
        public string DocumentoProveedor { get; set; }
        public string DireccionProveedor { get; set; }
        public string TelefonoProveedor { get; set; }
        public string CiudadProveedor { get; set; }
        public string PaisProveedor { get; set; }
        public string NombreDestinatario { get; set; }
        //public string CedulaDestinatario { get; set; }
        //public string PrimerNombreDestinatario { get; set; }
        //public string SegundoNombreDestinatario { get; set; }
        //public string PrimerApellidoDestinatario { get; set; }
        //public string SegundoApellidoDestinatario { get; set; }
        //public string CelularDestinatario { get; set; }
        //public string CorreoDestinatario { get; set; }
        //public string DireccionDestinatario { get; set; }
        public string CondicionPago { get; set; }
        public string Notas { get; set; }
        public string Fecha { get; set; }
        public decimal Descuento { get; set; }
        public decimal Iva { get; set; }
        public decimal Fletes { get; set; }
        public decimal IvaFletes { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Asesor { get; set; }
        public string usuario_login { get; set; }
        public List<DetalleRepuestoOrdenModel> ListaDetalles { get; set; }


        public OrdenRepuestoModel()
        {
            ListaDetalles = new List<DetalleRepuestoOrdenModel>();
        }



    }
}