namespace Homer_MVC.Models
{
    public class ListaPlanPagoModel
    {
        public int idCotizacion { get; set; }
        public string tasaInteres { get; set; }
        public decimal? valorTotal { get; set; }
        public decimal? cuotaInicial { get; set; }
        public string cuotas { get; set; }
        public decimal? credito { get; set; }
        public string cuota12 { get; set; }
        public string cuota24 { get; set; }
        public string cuota36 { get; set; }
        public string cuota48 { get; set; }
        public string cuota60 { get; set; }
        public string cuota72 { get; set; }
        public string cuota84 { get; set; }
        public decimal? valor_cuota12 { get; set; }
        public decimal? valor_cuota24 { get; set; }
        public decimal? valor_cuota36 { get; set; }
        public decimal? valor_cuota48 { get; set; }
        public decimal? valor_cuota60 { get; set; }
        public decimal? valor_cuota72 { get; set; }
        public decimal? valor_cuota84 { get; set; }
    }
}