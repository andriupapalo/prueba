using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class SeguimientoCarteraTerceroModel
    {
        public int id { get; set; }
        public int tercero_id { get; set; }
        public int tipo_id { get; set; }
        [Display(Name = "Nota")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string nota { get; set; }
        public System.DateTime fecha { get; set; }
        public int user_creacion_id { get; set; }
        public int encabezado_id { get; set; }
    }
}