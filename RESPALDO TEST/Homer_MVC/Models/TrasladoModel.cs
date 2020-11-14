using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class TrasladoModel
    {
        public int? encabezado_id { get; set; }
        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int TipoDocumento { get; set; }
        [Display(Name = "origen")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int BodegaOrigen { get; set; }
        [Display(Name = "destino")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int BodegaDestino { get; set; }
        public string Referencia { get; set; }
        public decimal? Costo { get; set; }
        [Display(Name = "usuario")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int UsuarioRecepcion { get; set; }
        [Display(Name = "perfil contable")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int PerfilContable { get; set; }
        public string modeloVehiculo { get; set; }
        public string colorVehiculo { get; set; }
        public decimal? cantidad { get; set; }
        public string Notas { get; set; }
        public int mot_traslado { get; set; }
        public bool mensajeria_atendido { get; set; }
        public bool requiere_mensajeria { get; set; }
        public int? solicitudtranslado { get; set; }



    }
}