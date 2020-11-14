using System;
using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class ModeloPlanFinanciero
    {
        public int plan_id { get; set; }
        public string plan_fecela { get; set; }
        public string plan_usuela { get; set; }
        public string plan_descripcion { get; set; }
        public bool plan_estado { get; set; }
        public string plan_nombre { get; set; }
        public string plan_imagen { get; set; }
        public bool plan_comision { get; set; }
        public double plan_porcentaje_comision { get; set; }
        public int plan_usuario_actualizacion { get; set; }
        public DateTime plan_fecha_actualizacion { get; set; }
        public string plan_razon_inactivo { get; set; }
        public int idfinanciera { get; set; }
        public double? tasa_interes { get; set; }
        public List<Listaplanfinancierobodega> Listaplanfinancierobodega { get; set; }
    }

    public class Listaplanfinancierobodega
    {
        public int id { get; set; }
        public int idbodega { get; set; }
        public string nomBodega { get; set; }
        public int idplanfinanciero { get; set; }
        public string nomFinaciera { get; set; }
        public double porcentaje { get; set; }
        public string fechadesde { get; set; }
        public string fechahasta { get; set; }
        public bool estado { get; set; }
        public string Desestado { get; set; }
        public string razoninactividad { get; set; }


    }
}