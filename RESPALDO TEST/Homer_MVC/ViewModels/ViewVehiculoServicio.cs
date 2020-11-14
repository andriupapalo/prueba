using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.ViewModels
{
//    private Iceberg_Context db = new Iceberg_Context();
    public class ViewVehiculoServicio
    {
        public ViewVehiculo vvicb_vehiculo { get; set; }
        public List<ViewVehiculo> vvlistaicb_vehiculo { get; set; }
        public ViewModelo vvmodelo_vehiculo { get; set; }
        public List<ViewModelo> vvlistamodelo_vehiculo { get; set; }
        public ViewMarca vvmarca_vehiculo { get; set; }
        public List<ViewMarca> vvlistamarca_vehiculo { get; set; }
        public ViewTipo vvtipo_vehiculo { get; set; }
        public List<ViewTipo> vvlistatipo_vehiculo { get; set; }

       

     


    }
}