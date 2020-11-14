using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class consecutivoOrdenTrabajoModel
    {
        public int otcon_id { get; set; }
        [Display(Name = "Prefijo:")]
        public string otcon_prefijo { get; set; }
        [Display(Name = "Bodega:")]
        public int otcon_bodega { get; set; }
        [Display(Name = "Consecutivo:")]
        public int otcon_consecutivo { get; set; }
        [Display(Name = "Estado:")]
        public bool otcon_estado { get; set; }
        [Display(Name = "Razon Inactivación:")]
        public string otcon_razoninactivo { get; set; }
    }
}