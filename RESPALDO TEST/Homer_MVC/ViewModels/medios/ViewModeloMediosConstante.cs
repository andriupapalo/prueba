using Homer_MVC.IcebergModel;
using System.Collections.Generic;

namespace Homer_MVC.ViewModels.medios
{
    public class ViewModeloMediosConstante
    {
        public medios_constantes Viewmediosconstantes { get; set; }
        public medios_gen Viewmedios_gen { get; set; }
        public medios_movtos Viewmedios_movtos { get; set; }
        public cuenta_puc Viewcuenta_puc { get; set; }
        public tp_doc_registros Viewtp_doc_registros { get; set; }
        public mediostipovalor Viewmediostipovalor { get; set; }
        public icb_terceros Viewterceros { get; set; }
        public List<icb_terceros> ViewListterceros { get; set; }


        public bool sitope { get; set; }


    }
}