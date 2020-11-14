using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class AgendaMensajeria
    {
        public int id { get; set; }
        [Required]
        public DateTime desde { get; set; }
        [Required]
        public DateTime hasta { get; set; }
        [Required]
        public int mensajero { get; set; }
        [Required]
        public int userid_creacion { get; set; }
        [Required]
        public int user_idactualizacion { get; set; }
        [Required]
        public DateTime fec_creacion { get; set; }
        [Required]
        public DateTime fec_actualizacion { get; set; }
        [Required]
        public string descripcion { get; set; }


    }
}