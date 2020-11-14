using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PdfRemision
    {

        //idRemision, numero de remision, fecha, referencias, cantidades, documento cliente, asesor Seccion de totales: Subtotal, iva, descuento, valor total.

        public int? idRemision { get; set; }
        public string prefijo { get; set; }
        public long consecutivo { get; set; }
        public string numRemision { get; set; }
        public string fecha { get; set; }
        public List<repuestos_remision> repuestos { get; set; }
        public string documento { get; set; }
        public string cliente { get; set; }
        public string asesor { get; set; }
        public string subtotal { get; set; }
        public string valoriva { get; set; }
        public string totaldescuento { get; set; }
        public string totalFactura { get; set; }

    }

    public class repuestos_remision
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string codigo { get; set; }
        public bool recibidorep { get; set; }
        public string cantidad { get; set; }
        public int centro_costo { get; set; }
        public int tarifatipo { get; set; }
        public string precio_unitario { get; set; }
        public string valorBruto { get; set; }
        public string valorBaseiva { get; set; }
        public string porcentaje_iva { get; set; }
        public string porcentaje_descuento { get; set; }
        public string descuento { get; set; }
        public string totaldescuento { get; set; }
        public string iva { get; set; }
        public decimal iva2 { get; set; }
        public string totaliva { get; set; }
        public string totalsiniva { get; set; }
        public int suma { get; set; }
        public string precio_total { get; set; }
        public decimal? precio_total2 { get; set; }
        public decimal? sum { get; set; }
        public decimal? bruto1 { get; set; }
        public decimal? bruto2 { get; set; }

    }
}