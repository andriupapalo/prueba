using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class ModeloTempario
    {
        public string codigo { get; set; }
        [Display(Name = "Operacion")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string operacion { get; set; }
        [Display(Name = "Tipo Operacion")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? tipooperacion { get; set; }
        [Display(Name = "Tiempo")]
        public float? tiempo { get; set; }
        public int? iva { get; set; }
        public decimal? costo { get; set; }
        public decimal? precio { get; set; }
        public int? id_licencia { get; set; }
        public DateTime fec_creacion { get; set; }
        public int? userid_creacion { get; set; }
        public DateTime? fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        [Display(Name = "Es Matriz")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool esmatriz { get; set; }
        [Display(Name = "Es Matriz")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool aplica_costo { get; set; }
        public decimal? preciomatriz { get; set; }
        public decimal? precioflotamatriz { get; set; }
        public float? tiempomatriz { get; set; }
        [Display(Name = "Horas Operario")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string HoraOperario { get; set; }
        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string categoria { get; set; }
        [Display(Name = "Plan de Mantenimiento")]
        [ValidarPlan]
        public int? idplanm { get; set; }
        [Display(Name = "Horas Cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string HoraCliente { get; set; }
        public string operacionhr { get; set; }


        public class ValidarPlan : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ModeloTempario razon = (ModeloTempario)validationContext.ObjectInstance;
                if (razon.esmatriz)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido.");
                    }
                }
                return ValidationResult.Success;
            }
        }
    }
}