using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class SegmentacionClienteForm
    {
        public int? id { get; set; }
        [Display(Name = "nombre Segmento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string descripcion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        [Display(Name = "Concesionario")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? concesionario { get; set; }
        [Display(Name = "Evalua Cantidad")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool evalua_cantidad { get; set; }
        [Display(Name = "Cantidad Vehículos")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int cantidad_vehiculos { get; set; }
        [Display(Name = "Revisiones")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool revisiones_dia { get; set; }
        [Display(Name = "Evalua Revisiones")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool evalua_revisiones { get; set; }
        [Display(Name = "Soat")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool soat_dia { get; set; }
        [Display(Name = "Evalua Soat")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool evalua_soat { get; set; }
        [Display(Name = "Evalua pólizas")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool evalua_polizas { get; set; }
        [Display(Name = "Póliza")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool poliza_dia { get; set; }
    }
}