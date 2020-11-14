namespace Homer_MVC.Models
{
    public class ElementosFacturacion
    {
        public string tipo { get; set; }
        public string codigo { get; set; }
        public decimal cantidad { get; set; }
        public decimal valor_unitario { get; set; }
        public int tipo_tarifa { get; set; }
        public decimal tiempo { get; set; }
        public decimal porcentaje_descuento { get; set; }
        public decimal valor_descuento { get; set; }
        public decimal porcentaje_iva { get; set; }
        public decimal valor_iva { get; set; }

        public int centro_costo { get; set; }

    }
}