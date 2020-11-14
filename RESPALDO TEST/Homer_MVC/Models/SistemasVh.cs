using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Homer_MVC.Models
{
    [Bind(Exclude = "ttemmpario")]
    public class SistemasVh
    {
        public int id { get; set; }
        [Display(Name = "Sistema Vehiculo ")]
        [Required]
        public string tsis_sistema { get; set; }
        [Display(Name = "Razon Inactivación")]
        public string tsis_razoninactivo { get; set; }
        [Display(Name = "Estado")]
        public bool tsis_estado { get; set; }
        public int tsis_usuela { get; set; }
        public DateTime tsis_fecela { get; set; }
        public int tsis_usumod { get; set; }
        public DateTime tsis_fecmod { get; set; }
        [Display(Name = "Operaciones")]
        [Required]
        public List<listaGeneral> ttemmpario_list { get; set; }
        public List<string> ttempario_codigo { get; set; }
    }
}