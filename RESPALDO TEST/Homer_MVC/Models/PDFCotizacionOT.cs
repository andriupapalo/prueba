using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Homer_MVC.Models
{
    public class PDFCotizacionOT
    {
        public int? id { get; set; }
        [Display(Name = "Tipo Documento ")]
        [Required]
        public string idtipodoc { get; set; }
        [Required]
        public string codigoentrada { get; set; }
        [Required]
        public string bodega { get; set; }
        [Required]
        public string numero { get; set; }
        [Required]
        public string tercero { get; set; }
        [Required]
        public string placa { get; set; }
        public string modelo { get; set; }
        public int? anio_modelo { get; set; }
        public string serie { get; set; }
        public string numero_motor { get; set; }
        public string color { get; set; }
        public string fecha_venta { get; set; }
        public string fecha_fin_garantia { get; set; }

        public string kilometraje_actual { get; set; }

        [Required]
        public string asesor { get; set; }
        [Required]
        public string tecnico { get; set; }
        [Required]
        public string entrega { get; set; }
        public string aseguradora { get; set; }
        public string poliza { get; set; }
        public string siniestro { get; set; }
        public string deducible { get; set; }
        public string minimo { get; set; }
        public string notas { get; set; }
        public string kilometraje { get; set; }
        public string razoningreso { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int? tipooperacion { get; set; }
        public bool? domicilo { get; set; }
        public int? idcita { get; set; }

        [Required]
        public string txtDocumentoCliente { get; set; }
        public string nombrecliente { get; set; }
        public string telefonocliente { get; set; }
        public string celularcliente { get; set; }
        public string correocliente { get; set; }
        public string ciudadcliente { get; set; }

        public int? estadoorden { get; set; }
        public string centrocosto { get; set; }
        public int? razon_secundaria { get; set; }
        public string garantia_falla { get; set; }
        public string garantia_causa { get; set; }
        public string garantia_solucion { get; set; }
        public string fecha_soat { get; set; }
        public string fecha_generacion { get; set; }
        public string fecha_prometida { get; set; }
        public string numero_soat { get; set; }
        public int? recibidode { get; set; }
        public string id_plan_mantenimiento { get; set; }
        public List<operacionesPlan> operaciones_plan { get; set; }
        public string totaltiempooperacionesplan { get; set; }
        public string totalvaloroperacionesplan { get; set; }
        public List<suministrosPlan> repuestos_plan { get; set; }
        public string totaltcantidadrepuestosplan { get; set; }
        public string totalvalorrepuestosplan { get; set; }
        public List<operaciones> operaciones { get; set; }
        public string totaltiempooperaciones { get; set; }
        public string totalvaloroperaciones { get; set; }
        public List<repuestos> repuestos { get; set; }
        public string totaltcantidadrepuestos { get; set; }
        public string totalvalorrepuestos { get; set; }
        public string valorDescuento { get; set; }
        public string valorIva { get; set; }
        public string valorTotal { get; set; }
        public List<solicitudes> solicitudes { get; set; }
    }

    public class operacionesPlan
    {
        public string codigo { get; set; }
        public string operacion { get; set; }
        public string tecnico { get; set; }
        public string tiempo { get; set; }
        public string valor_total { get; set; }
        public decimal? valor_total2 { get; set; }
    }

    public class suministrosPlan
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string precio_unitario { get; set; }
        public string iva { get; set; }
        public string precio_total { get; set; }
        public decimal? precio_total2 { get; set; }
        public int suma { get; set; }
        public int bruto1 { get; set; }
        public int bruto2 { get; set; }

    }

    public class operaciones
    {
        public string codigo { get; set; }
        public string operacion { get; set; }
        public string tiempo { get; set; }
        public string tecnico { get; set; }
        public string valorUnitario { get; set; }
        public string valorBruto { get; set; }
        public string valorBaseiva { get; set; }
        public string porcentaje_iva { get; set; }
        public string porcentaje_descuento { get; set; }
        public string totaliva { get; set; }
        public string totaldescuento { get; set; }
        public string valor_total { get; set; }
        public decimal? valor_total2 { get; set; }
    }

    public class repuestos
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
        public decimal descuento2 { get; set; }
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

    public class solicitudes
    {
        public string descripcion_solicitud { get; set; }
        public string respuesta_taller { get; set; }
    }
}