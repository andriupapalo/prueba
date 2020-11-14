using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class ModeloPdfCotizacion
    {



        public string titulo { get; set; }
        //------------------------------ Empresa ------------------------------ 
        public int idEmpresa { get; set; }
        public string descripcionEmpresa { get; set; }
        public string direccionEmpresa { get; set; }
        public string rifEmpresa { get; set; }
        //------------------------------ Cotizacion ------------------------------ 

        public int ref_mov { get; set; }
        public int idCotizacion { get; set; }
        public long numeroCotizacion { get; set; }
        public string fechaCotizacion { get; set; }
        public string fechaVencimiento { get; set; }
        public string nota { get; set; }
        public string valorbruto { get; set; }
        public string valordescuento { get; set; }
        public string valorfletes { get; set; }
        public string baseiva { get; set; }
        public string valoriva { get; set; }
        public string valorneto { get; set; }
        //------------------------------ Cliente ------------------------------ 
        public int idCliente { get; set; }
        public string nitCliente { get; set; }
        public string docCliente { get; set; }
        public string nomCliente { get; set; }
        public string dirCliente { get; set; }
        public string telCliente { get; set; }

        //------------------------------ Vehiculo ------------------------------ 
        public int idVehiculo { get; set; }
        public int desVehiculo { get; set; }
        public string modeloVehiculo { get; set; }
        public string placaVehiculo { get; set; }
        public string aseguradoVehiculo { get; set; }

        public int idusuario { get; set; }
        public List<detalleCotizacion> detalleCotizacion { get; set; }
        public string vigenciatext { get; set; }
        public int vigencianum { get; set; }
        public string nombreusuario { get; set; }
    }

    public class detalleCotizacion
    {
        //------------------------------ detalle Comprobante ------------------------------ 
        public int contador { get; set; }
        public int ref_mov { get; set; }
        public int id_Item { get; set; }
        public string codItem { get; set; }
        public int numitem { get; set; }
        public string descripcionItem { get; set; }
        public string canItem { get; set; }
        public string dctItem { get; set; }
        public string dctval { get; set; }
        public string valiva { get; set; }
        public string ivaItem { get; set; }
        public string pre_unit_Item { get; set; }
        public string valorItem { get; set; }
        public decimal valorItem2 { get; set; }

        public string pre_unit_dcto_Item { get; set; }
        public string valor_dcto_Item { get; set; }
    }

}