using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models.SeguimientoModel
{
    public class SeguimientosModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int? Codigo { get; set; }

        [Required]
        public bool EsManual { get; set; }

        [Required]
        public bool EsEstandar { get; set; }

        [Required]
        public string Evento { get; set; }

        [Required]
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        [Required]
        public int? Modulo { get; set; }

    }

    public class ModuloSeguimientosModel
    {

        public int Id { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int? Codigo { get; set; }


        [Required]
        public bool Estado { get; set; }


        [Required]
        public string Modulo { get; set; }

    }

    public class SeguimientoAnulacion
    {
        public int Id { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int? CodigoSeguimiento { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int? IdMotivoAnulacion { get; set; }

        public bool EstadoAnulacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

    }
}