using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class EntradaSalidaModel
    {

        public int? encabezado_id { get; set; }
        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int TipoDocumento { get; set; }
        [Display(Name = "origen")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int BodegaOrigen { get; set; }
        [Display(Name = "perfil contable")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int PerfilContable { get; set; }
        public string Referencia { get; set; }
        public decimal? cantidad { get; set; }
        public string Notas { get; set; }

    }
}