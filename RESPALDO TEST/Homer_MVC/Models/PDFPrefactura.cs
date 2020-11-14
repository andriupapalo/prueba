using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PDFPrefactura
    {
        public string codigoentrada { get; set; }
        public string placa { get; set; }
        public string vehiculo { get; set; }
        public string modelo { get; set; }
        public string serie { get; set; }
        public long kilometraje { get; set; }
        public string Inspeccion { get; set; }
        public string txtDocumentoCliente { get; set; }
        public string nombrecliente { get; set; }
        public string telefonocliente { get; set; }
        public string celularcliente { get; set; }
        public string correocliente { get; set; }
        public string ciudadcliente { get; set; }
        public string asesor { get; set; }
        public string tecnico { get; set; }
        public string prefijo { get; set; }
        public long concecutivo { get; set; }
        public string bodega { get; set; }
        public string aseguradora { get; set; }
        public string poliza { get; set; }
        public List<operaciones> operacionesGarantia { get; set; }
        public string totaltiempooperaciones { get; set; }
        public string totalvaloroperaciones { get; set; }
        public decimal? totaliva { get; set; }
        public decimal? totaldescuento { get; set; }
    }

}