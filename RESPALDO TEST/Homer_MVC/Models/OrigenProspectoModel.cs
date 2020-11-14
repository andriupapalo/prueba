using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class OrigenProspectoModel
    {
        [Display(Name = "nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string descripcion { get; set; }

        [Display(Name = "fecha Inicial")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechaini { get; set; }

        [Display(Name = "fecha Final")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public DateTime fechafin { get; set; }

        public bool evento { get; set; }
    }
}