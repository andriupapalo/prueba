using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class SolicitudPeritajeModel
    {
        [Display(Name = "tipo peritaje")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tpper_id { get; set; }

        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tpdoc_id { get; set; }

        [Display(Name = "marca")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int marcvh_id { get; set; }

        [Display(Name = "modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string modvh_codigo { get; set; }

        [Display(Name = "color")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string colvh_id { get; set; }

        [Display(Name = "tipo vehiculo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tpvh_id { get; set; }

        public int? tpserv_id { get; set; }
        public int? cilindraje { get; set; }
        public int? tpcaj_id { get; set; }

        [Display(Name = "serie")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string serie { get; set; }

        [Display(Name = "numero motor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string num_motor { get; set; }

        public int? tpmot_id { get; set; }

        [Display(Name = "ciudad")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int ciu_id { get; set; }

        //[Display(Name = "genero")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public int? gentercero_id { get; set; }

        [Display(Name = "fecha")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime fecha_peritaje { get; set; }

        [Display(Name = "hora inicio")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string horaInicio { get; set; }

        [Display(Name = "hora fin")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string horaFin { get; set; }

        public string doc_tercero { get; set; }

        //[Display(Name = "primer nombre")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public string prinom_tercero { get; set; }

        //[Display(Name = "razon social")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public string razon_social { get; set; }

        public string segnom_tercero { get; set; }

        //[Display(Name = "primer apellido")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }
        public string telf_tercero { get; set; }

        [Display(Name = "celular")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string celular_tercero { get; set; }

        public string email_tercero { get; set; }

        [Display(Name = "placa")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string plac_vh { get; set; }

        public int? ciudad_placa { get; set; }

        [Display(Name = "perito")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int perito_id { get; set; }

        public int anio { get; set; }

        [Display(Name = "año")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int anioTxt { get; set; }


        [Display(Name = "Kilometraje registrado")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public long kilometraje { get; set; }

        [Display(Name = "Kilometraje actual")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public long kilometrajeActual { get; set; }
    }
}