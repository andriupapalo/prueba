using System;

namespace Homer_MVC.IcebergModel
{
    public class TerceroClienteModel
    {
        public int cltercero_id { get; set; }
        public int? tercliid_licencia { get; set; }
        public DateTime? terclifec_creacion { get; set; }
        public int? tercliuserid_creacion { get; set; }
        public DateTime? terclifec_actualizacion { get; set; }
        public int? tercliuserid_actualizacion { get; set; }
        public DateTime fec_nacimiento { get; set; }
        public int? genero_tercero { get; set; }
        public int? numhijos_tercero { get; set; }
        public string contac_tercero { get; set; }
        public int? tlfcontac_tercero { get; set; }
        public bool habdtautor_correo { get; set; }
        public bool habdtautor_celular { get; set; }
        public bool habdtautor_msm { get; set; }
        public bool tercli_estado { get; set; }
        public int tercero_id { get; set; }
        public int tpocupacion_id { get; set; }
        public int tphobby_id { get; set; }
        public int tpdpte_id { get; set; }
        public int edocivil_id { get; set; }
        public int? razoninactivo_id { get; set; }
        public bool empleado { get; set; }
        public bool proveedor { get; set; }
    }
}