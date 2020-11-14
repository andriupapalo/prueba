using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class ModeloPdfPedido
    {



        public string Ptitulo { get; set; }
        //------------------------------ Empresa ------------------------------ 
        public int PidEmpresa { get; set; }
        public string PdescripcionEmpresa { get; set; }
        public string PdireccionEmpresa { get; set; }
        public string PrifEmpresa { get; set; }
        //------------------------------ Cotizacion ------------------------------ 

        public int Pref_mov { get; set; }
        public int PidPedido { get; set; }
        public long PnumeroPedido { get; set; }
        public string PfechaPedido { get; set; }
        public string PfechaVencimiento { get; set; }
        public string Pnota { get; set; }
        public string Pvalorbruto { get; set; }
        public string Pvalordescuento { get; set; }
        public string Pvalorfletes { get; set; }
        public string Pbaseiva { get; set; }
        public string Pvaloriva { get; set; }
        public string Pvalorneto { get; set; }
        //------------------------------ Cliente ------------------------------ 
        public int PidCliente { get; set; }
        public string PnitCliente { get; set; }
        public string PdocCliente { get; set; }
        public string PnomCliente { get; set; }
        public string PdirCliente { get; set; }
        public string PtelCliente { get; set; }

        //------------------------------ Vehiculo ------------------------------ 
        public int PidVehiculo { get; set; }
        public int PdesVehiculo { get; set; }
        public string PmodeloVehiculo { get; set; }
        public string PplacaVehiculo { get; set; }
        public string PaseguradoVehiculo { get; set; }

        public string vigenciatext { get; set; }
        public int vigencianum { get; set; }
        public int Pidusuario { get; set; }
        public List<detallePDF_Pedido> detallePDF_Pedido { get; set; }
    }
    public class detallePDF_Pedido
    {
        //------------------------------ detalle Comprobante ------------------------------ 
        public int Pcontador { get; set; }
        public int Pref_mov { get; set; }
        public int Pid_Item { get; set; }
        public string PcodItem { get; set; }
        public int Pnumitem { get; set; }
        public string PdescripcionItem { get; set; }
        public string PcanItem { get; set; }
        public string PdctItem { get; set; }
        public string Pdctval { get; set; }
        public string Pvaliva { get; set; }
        public string PivaItem { get; set; }
        public string Ppre_unit_Item { get; set; }
        public string PvalorItem { get; set; }
        public decimal PvalorItem2 { get; set; }

        public string Ppre_unit_dcto_Item { get; set; }
        public string Pvalor_dcto_Item { get; set; }
    }
}