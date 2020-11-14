namespace Homer_MVC.Models
{
    public class Modelo_adecuaciones
    {
        public int id_adecuacion { get; set; }

        //Repuestos
        public int peritaje_id { get; set; }
        public string cod_referencia { get; set; }
        public string des_referencia { get; set; }
        public string fecha { get; set; }
        public string valor_unitario_repuesto { get; set; }
        public string cantidad { get; set; }
        public string descuento { get; set; }
        public string valor_descuento { get; set; }
        public string valor_sin_Iva { get; set; }
        public string Iva { get; set; }
        public string valor_Iva { get; set; }
        public string Total_Item { get; set; }

        //Mano de Obra
        public int cod_mano_obra { get; set; }
        public string fecha_mo { get; set; }
        public string valor_unitario_mo { get; set; }


    }
}