using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class DespachoOrden
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
        public int idorden { get; set; }
        public int idpedido { get; set; }
        public string Notas { get; set; }
    }
}