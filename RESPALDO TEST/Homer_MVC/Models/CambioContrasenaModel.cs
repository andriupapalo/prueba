using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class CambioContrasenaModel
    {
        public int? id_usuario { get; set; }

        [Display(Name = "Contraseña actual")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string ContrasenaActual { get; set; }

        [Display(Name = "Contraseña nueva")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string ContrasenaNueva { get; set; }

        [Display(Name = "Confirmar contraseña")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string ConfirmarContrasena { get; set; }
    }
}