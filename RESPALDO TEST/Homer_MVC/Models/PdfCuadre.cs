using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PdfCuadre
    {
        public List<mediosPago> efectivo { get; set; }
        public List<mediosPago> tarjetas { get; set; }
        public List<mediosPago> cheque { get; set; }
        public List<mediosPago> cupo { get; set; }

        public string totalefectivo { get; set; }
        public string totaltarjeta { get; set; }
        public string totalCheque { get; set; }
        public string totalcupo { get; set; }
        public string totalGeneral { get; set; }

    }

    public class mediosPago
    {
        public int id_Medio { get; set; }
        public string nomMedio { get; set; }
        public int id_Factura { get; set; }
        public long numFactura { get; set; }
        public decimal ValorFactura { get; set; }
        public decimal ValorMedio { get; set; }
    }
}