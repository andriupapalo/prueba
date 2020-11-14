using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class RemisionClienteModel
    {

        public int? encabezado_id { get; set; }
        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int TipoDocumento { get; set; }
        [Display(Name = "bodega")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int bodega { get; set; }
        [Display(Name = "asesor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int asesor { get; set; }
        [Display(Name = "cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int cliente { get; set; }
        public int estado { get; set; }
        public string Referencia { get; set; }
        public decimal? cantidad { get; set; }
        public string Notas { get; set; }

    }
}