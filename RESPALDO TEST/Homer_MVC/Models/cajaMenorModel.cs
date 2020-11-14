using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class cajaMenorModel
    {
        public int cjm_id { get; set; }
        [Display(Name = "Caja: ")]
        [Required]
        public string cjm_desc { get; set; }
        [Display(Name = "Reponsable:")]
        [Required]
        public int id_responsable { get; set; }

        [Display(Name = "Bodega:")]
        public int id_bodega { get; set; }

        [Display(Name = "Monto Caja:")]
        [Required]
        public decimal cjm_valor { get; set; }
        [Display(Name = "Estado:")]
        [Required]
        public bool cjm_estado { get; set; }
        [Display(Name = "Razon Inactividad:")]
        public string cjm_razoninactivo { get; set; }
    }

    public class bodegaCajaMenor
    {
        public int id { get; set; }
        public string bodccs_nombre { get; set; }
        public bool check { get; set; }
    }

    public class usuarioList
    {
        public int user_id { get; set; }
        public string user_nombre { get; set; }
    }

    public class listaGeneral
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public string codigo { get; set; }
    }


}