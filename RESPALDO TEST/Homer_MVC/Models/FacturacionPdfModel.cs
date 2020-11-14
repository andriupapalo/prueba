using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class FacturacionPDFModel
    {
        // Empresa
        public int id_Empresa { get; set; }
        public string nomEmpresa { get; set; }
        public string dirEmpresa { get; set; }
        public string nitEmpresa { get; set; }
        public string bodega { get; set; }
        // Factura

        public int id_Factura { get; set; }
        public string numFactura { get; set; }
        public string fechaFactura { get; set; }
        public string formapagoFactura { get; set; }
        public string fechavenceFactura { get; set; }
        public string tipo_factura { get; set; }

        // cliente
        public int id_Cliente { get; set; }
        public string docCliente { get; set; }
        public string nomCliente { get; set; }
        public string dirCliente { get; set; }
        public string telCliente { get; set; }
        public string ciuCliente { get; set; }

        // Vehiculo

        public int id_Vehiculo { get; set; }
        public string desVehiculo { get; set; }
        public string placaVehiculo { get; set; }
        public string modeloVehiculo { get; set; }

        // Vendedor
        public int id_Vendedor { get; set; }
        public string nomVendedor { get; set; }
        // Detalle Facturacion
        public List<detalleFacturacion> detalleFacturacion { get; set; }

        // TOTALES
        public string valorbruto { get; set; }
        public string valordescuento { get; set; }
        public string valorfletes { get; set; }
        public string baseiva { get; set; }
        public string subtotal { get; set; }
        public string valoriva { get; set; }
        public string ic_bolsa { get; set; }
        public string totalFactura { get; set; }

        //Resolucion
        public string prefi { get; set; }
        public string referencia { get; set; }
        public string fechai { get; set; }
        public string facti { get; set; }
        public string factf { get; set; }

    }

    public class detalleFacturacion
    {
        public int id_Factura { get; set; }
        public int id_detalleFactura { get; set; }
        public string referenciaFactura { get; set; }
        public string descripcionFactura { get; set; }
        public string cantFactura { get; set; }
        public string pordescuentoFactura { get; set; }
        public string porivaFactura { get; set; }
        public string montoivaFactura { get; set; }
        public string preciounitarioFactura { get; set; }
        public string valorFactura { get; set; }
        public string tipo_tarifa { get; set; }


    }

}