namespace Homer_MVC.IcebergModel
{
    public class DetalleCotizacionModel
    {
        public decimal num_cotizacion { get; set; }
        public int id_detalle { get; set; }
        public string descripcion { get; set; }
        public decimal valor { get; set; }
        public decimal? poliza { get; set; }
        public decimal? matricula { get; set; }
        public decimal? soat { get; set; }
        public decimal? vrretoma { get; set; }
        public string modeloretoma { get; set; }
        public string placaretoma { get; set; }
        public int kilometrajeretoma { get; set; }
        public int anioVhretoma { get; set; }
        public int cantidad { get; set; }
        public string codigo_referencia { get; set; }
        public string codigo_modelovh { get; set; }
        public decimal valor_total { get; set; }
    }
}