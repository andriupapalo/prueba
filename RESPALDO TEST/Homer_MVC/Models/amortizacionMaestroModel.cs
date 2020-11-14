namespace Homer_MVC.Models
{
    public class amortizacionMaestroModel
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public string placa { get; set; }
        public int idcentro { get; set; }
        public int ubicacion { get; set; }
        public int idempresa { get; set; }
        public int idresponsable { get; set; }
        public int idproveedor { get; set; }
        public string fecha_compra { get; set; }
        public bool mantenimiento { get; set; }
        public string numeroserial { get; set; }
        public string fecha_vencepoliza { get; set; }
        public string numeropoliza { get; set; }
        public int? idaseguradora { get; set; }
        public bool pignorado { get; set; }
        public string fecha_vencegarantia { get; set; }
        public string detalle_garantia { get; set; }
        public string orden_compra { get; set; }
        public bool activo { get; set; }
        public int? metododepreciacion { get; set; }
        public string valoractivo { get; set; }
        public string fecha_activacion { get; set; }
        public int? meses_depreciacion { get; set; }
        public string fecha_findepreciacion { get; set; }
        public string valorresidual { get; set; }
        public string fecha_baja { get; set; }
        public int? capacidadlocal { get; set; }
        public bool depreciable { get; set; }
        public int? metododeprecniff { get; set; }
        public string valoractivoniff { get; set; }
        public string fecha_activacionniif { get; set; }
        public int? meses_depreniff { get; set; }
        public string fecha_findepreniff { get; set; }
        public string valorresidualniif { get; set; }
        public string fecha_bajaniif { get; set; }
        public int? clasificacion { get; set; }
        public int? capacidadniif { get; set; }
        public bool depreciableniif { get; set; }
        public int? clasificacionniff { get; set; }
        public int? id_licencia { get; set; }
        public string fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public string fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }



        public string constantedepre { get; set; }
        public string constantedepreniif { get; set; }
        public string fechaactualizacion { get; set; }
        public string valordepre { get; set; }
        public string valordepreniif { get; set; }
        public bool vendido { get; set; }
        public int? motivo { get; set; }
        public bool finalizado { get; set; }
        public int? mesesfaltantes { get; set; }
        public int? mesesfaltantesniif { get; set; }

        public bool estado { get; set; }
        public string razon_inactivo { get; set; }

        public bool sipo { get; set; }
    }
}