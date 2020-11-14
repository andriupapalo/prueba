using Homer_MVC.IcebergModel;
using System.Collections.Generic;

namespace Homer_MVC.ViewModels.contabpdf
{
    public class ViewModelPDFcontable
    {
        public cuenta_puc vm_cuenta_pub { get; set; }
        public centro_costo vm_centro_costo { get; set; }
        public icb_terceros vm_icb_terceros { get; set; }
        public cuentas_valores vm_cuentas_valores { get; set; }
        public mov_contable vm_mov_contable { get; set; }
        public List<icb_terceros> vm_list_icb_terceros { get; set; }
        public List<centro_costo> vm_list_centro_costo { get; set; }
        public List<cuenta_puc> vm_list_cuenta_pub { get; set; }
        public List<cuentas_valores> vm_list_cuentas_valores { get; set; }
        public List<mov_contable> vm_list_mov_contable { get; set; }
    }
}