using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class InspeccionVhModel
    {
        public int icbvh_id { get; set; }
        public string vin { get; set; }
        public string modvh_descripcion { get; set; }
        public int insp_bodega { get; set; }
        public string bodega_nombre { get; set; }
        public string colvh_nombre { get; set; }
        public string evento_nombre { get; set; }

        [Display(Name = "voltaje")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int voltaje { get; set; }

        public DateTime? insp_fechains { get; set; }
        public int? insp_usuela { get; set; }
        public string insp_descripcion { get; set; }
        public int? ave_id { get; set; }
        public int? grave_id { get; set; }
        public string insp_recibidode { get; set; }
        public string insp_recibidoen { get; set; }
        public string insp_imagen { get; set; }
        public bool insp_estado { get; set; }
        public string insp_observacion { get; set; }
        public string insp_firma { get; set; }
        public string insp_dispositivo { get; set; }
        public int? insp_ubicacion { get; set; }
        public bool insp_sinmanual { get; set; }
        public bool insp_sinllaves { get; set; }
        public bool insp_sincontrol { get; set; }
        public bool insp_sucio { get; set; }
        public bool insp_sincintas { get; set; }
        public int medida_id { get; set; }
        public int comb_id { get; set; }
        public int zona_id { get; set; }
        public int impacto_id { get; set; }
        public int? insp_km { get; set; }
        public bool insp_sinmanualgarantia { get; set; }
        public int? insp_nrofotos { get; set; }
        public int? insp_kms { get; set; }
        public int? insp_idvin { get; set; }
        public int? insp_iddispositivo { get; set; }
        public string insp_prealerta { get; set; }
        public DateTime? insp_fechaprealerta { get; set; }
        public int? insp_idserial { get; set; }
        public int? insp_usuprealerta { get; set; }
        public DateTime? insp_fechaaddprealerta { get; set; }
        public string insp_versioninspeccion { get; set; }
        public int? taller_averia_id { get; set; }
        public int? estado_averia_id { get; set; }
    }
}