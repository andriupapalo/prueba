using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Homer_MVC.ViewModels
{
    public class ViewTipo : tipo_vehiculo
    {
        public virtual ViewVehiculo ViewVehiculo { get; set; }
    }
}