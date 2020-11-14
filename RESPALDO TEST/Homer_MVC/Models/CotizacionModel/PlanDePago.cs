using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models.CotizacionModel
{
    public class PlanDePago
    {
        public int id { get; set; }
        public int? asesor_id { get; set; }
        public int? licencia_id { get; set; }
        public int? cotizacion_id { get; set; }
        public string tasa_interes { get; set; }
        public decimal? valor_total { get; set; }
        public decimal? credito { get; set; }
        public decimal? cuota_inicial { get; set; }
        public string cuotas { get; set; }
        public int? plan_id { get; set; }
        public bool plan_elegido { get; set; }
        public string modelo { get; set; }
        public decimal valor_residual { get; set; }
        public decimal otros_valores { get; set; }

        public virtual int plan_pago_id { get; set; }
        public virtual string plazo { get; set; }
        public virtual decimal valor { get; set; }
        public virtual Decimal tasa { get; set; }       

    }
}