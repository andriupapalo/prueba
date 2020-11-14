using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class ModeloComprobante
    {
        public string titulo { get; set; }
        //------------------------------ Empresa ------------------------------ 
        public int idEmpresa { get; set; }
        public string descripcionEmpresa { get; set; }
        public string direccionEmpresa { get; set; }
        public string rifEmpresa { get; set; }
        //------------------------------ Comprobante ------------------------------ 

        public int idComprobante { get; set; }
        public long numeroComprobante { get; set; }
        public string fechaComprobante { get; set; }
        public string totalComprobante { get; set; }
        public string nota { get; set; }
        //------------------------------ Proveedor ------------------------------ 
        public int idProveedor { get; set; }
        public int nitProveedor { get; set; }
        public string docProveedor { get; set; }
        public string nomProveedor { get; set; }

        public string totalDebitoComprobante { get; set; }
        public string totalCreditoComprobante { get; set; }
        public int idusuario { get; set; }
        public List<detalleComprobante> detalleComprobante { get; set; }


    }

    public class detalleComprobante
    {
        //------------------------------ detalle Comprobante ------------------------------ 
        public int iddetalle { get; set; }
        public string cuenta { get; set; }
        public string detalle { get; set; }
        public string docProveedor { get; set; }
        public string nombreProveedor { get; set; }
        public string debito { get; set; }
        public string credito { get; set; }

        public string tdeb { get; set; }
        public string tcre { get; set; }

        public string cheque { get; set; }
        //------------------------------ otros detalle Comprobante ------------------------------ 
        public string tipo { get; set; }
        public string factura { get; set; }
        public decimal costo { get; set; }
        public decimal total { get; set; }
    }


}