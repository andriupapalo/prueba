using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class depreciacionActivosModelo
    {
        public int? encabezado_id { get; set; }
        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int TipoDocumento { get; set; }
        [Display(Name = "ubicacion")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int Ubicacion { get; set; }
        [Display(Name = "perfil contable")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int PerfilContable { get; set; }
        public int BodegaOrigen { get; set; }
        public int Centros { get; set; }
        public string Referencia { get; set; }
        public decimal? cantidad { get; set; }
        public string Notas { get; set; }


        //[Display(Name = "origen")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //public int BodegaOrigen { get; set; }
        //[Display(Name = "perfil contable")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //public int PerfilContable { get; set; }
        //public string Referencia { get; set; }
        //public decimal? cantidad { get; set; }
        //public string Notas { get; set; }
    }
}