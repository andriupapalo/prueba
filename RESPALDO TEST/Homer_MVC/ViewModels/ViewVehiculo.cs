using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Homer_MVC.IcebergModel;

namespace Homer_MVC.ViewModels
{

    public class ViewVehiculo : icb_vehiculo
    {
        public virtual ICollection<ViewModelo> viewmodelos { get; set; }
        public virtual ICollection<ViewMarca> viewmarcas { get; set; }
        public virtual ICollection<ViewTipo> viewtipos { get; set; }

    }
}