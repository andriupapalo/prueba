using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PdfFacturaOT1
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
        public string codigoentrada { get; set; }
        public string prefijo { get; set; }
        public long concecutivo { get; set; }
        public string referencia { get; set; }
        public string fechai { get; set; }
        public string facti { get; set; }
        public string factf { get; set; }
        public string asesor { get; set; }
        public string tecnico { get; set; }
        public int id_Vendedor { get; set; }
        public string nomVendedor { get; set; }


        // Vehiculo
        public string placa { get; set; }
        public string vehiculo { get; set; }
        public string modelo { get; set; }
        public string serie { get; set; }
        public long kilometraje { get; set; }
        public string Inspeccion { get; set; }
        public int id_Vehiculo { get; set; }
        public string desVehiculo { get; set; }
        public string placaVehiculo { get; set; }
        public string modeloVehiculo { get; set; }


        // Cliente
        public string txtDocumentoCliente { get; set; }
        public string nombrecliente { get; set; }
        public string telefonocliente { get; set; }
        public string celularcliente { get; set; }
        public string correocliente { get; set; }
        public string ciudadcliente { get; set; }
        public string aseguradora { get; set; }
        public string poliza { get; set; }


        // TOT
        public List<TOT> TOT { get; set; }

        public List<operaciones> operaciones_interno { get; set; }
        public List<operaciones> operaciones_garantia { get; set; }

        public List<operaciones> operacionesOT { get; set; }
        public List<repuestos> repuestos { get; set; }
        public List<detalleFacturacion> detalleFacturacionOT { get; set; }
        public List<operacionesPlan> operaciones_plan { get; set; }
        public string totaltiempooperacionesplan { get; set; }
        public string totalvaloroperacionesplan { get; set; }
        public List<suministrosPlan> repuestos_plan { get; set; }
        public string totaltcantidadrepuestosplan { get; set; }
        public string totalvalorrepuestosplan { get; set; }
        public string totaltcantidadrepuestos { get; set; }
        public string totalvalorrepuestos { get; set; }
        public string valorbruto { get; set; }
        public string valordescuento { get; set; }
        public string valorfletes { get; set; }
        public string baseiva { get; set; }
        public string subtotal { get; set; }
        public string valoriva { get; set; }
        public string ic_bolsa { get; set; }
        public string totalFactura { get; set; }
        public string totaltiempooperaciones { get; set; }
        public string totalvaloroperaciones { get; set; }
        public string totaliva { get; set; }
        public string totaldescuento { get; set; }
    }

    public class detalleFacturacionOT
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

    public class TOT {

        public string tot { get; set; }
        public string fecha { get; set; }
        public string proveedor { get; set; }
        public List<operacionestot> operacionesTOT { get; set; }
        public List<repuestostot> RepuestosTOT { get; set; }
        public string valor_Total { get; set; }

    }

    public class operacionestot {

        public string operacion { get; set; }

    }
    public class repuestostot {

        public string repuesto { get; set; }

    }

}