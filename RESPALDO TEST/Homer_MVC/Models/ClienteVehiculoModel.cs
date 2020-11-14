namespace Homer_MVC.Models
{
    public class ClienteVehiculoModel
    {
        // En caso de usarse para una cita
        public int? CitaId { get; set; }

        public string PlanMayor { get; set; }
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Color { get; set; }
        public string Serie { get; set; }
        public string Motor { get; set; }
        public string TipoVehiculo { get; set; }
        public long? KilometrajeAnterior { get; set; }

        public long? Kilometraje { get; set; }
        public int? CombustibleId { get; set; }
        public int? BahiaId { get; set; }


        public int? TerceroId { get; set; }
        public string Documento { get; set; }
        public string Nombres { get; set; }
        public string DigitoVerificacion { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }

    }
}