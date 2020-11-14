using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class flotasModel
    {
        public int? idflota { get; set; }
        [Required]
        public int flota { get; set; }
        [Required]
        public string numero { get; set; }
        [Required]
        public int nit_flota { get; set; }
        [Required]
        public string fec_solicitud { get; set; }
        public string detalle { get; set; }
    }
}