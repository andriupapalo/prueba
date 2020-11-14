namespace Homer_MVC.IcebergModel
{
    public class InpeccionPeritajeModel
    {
        public int veh_id { get; set; }
        public string primerNombre { get; set; }
        public string segundoNombre { get; set; }
        public string primerApellido { get; set; }
        public string segundoApellido { get; set; }
        public string telefono { get; set; }
        public string celular { get; set; }
        public string email { get; set; }
        public string placa { get; set; }
        public string modelo { get; set; }
        public int? tpVehiculo { get; set; }
        public string color { get; set; }
        public string serie { get; set; }
        public int? cilindraje { get; set; }
        public int? tpMotor { get; set; }
        public string numMotor { get; set; }
        public int zona_peritaje { get; set; }
        public int pieza_peritaje { get; set; }
        public int conve_peritaje { get; set; }
    }
}