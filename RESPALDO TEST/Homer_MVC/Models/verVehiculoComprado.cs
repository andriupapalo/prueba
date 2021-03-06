﻿using System;

namespace Homer_MVC.Models
{
    public class verVehiculoComprado
    {
        public string plan_mayor { get; set; }
        public int icbvh_id { get; set; }
        public Nullable<int> icbvhid_licencia { get; set; }
        public System.DateTime icbvhfec_creacion { get; set; }
        public int icbvhuserid_creacion { get; set; }
        public Nullable<System.DateTime> icbvhfec_actualizacion { get; set; }
        public Nullable<int> icbvhuserid_actualizacion { get; set; }
        public bool icbvh_estado { get; set; }
        public string icbvhrazoninactivo { get; set; }
        public Nullable<int> tpvh_id { get; set; }
        public int marcvh_id { get; set; }
        public Nullable<int> clavh_id { get; set; }
        public string kmat_zvsk { get; set; }
        public string modvh_id { get; set; }
        public Nullable<int> estvh_id { get; set; }
        public string colvh_id { get; set; }
        public string vin { get; set; }
        public string nummot_vh { get; set; }
        public string plac_vh { get; set; }
        public int anio_vh { get; set; }
        public int id_bod { get; set; }
        public Nullable<int> icbvh_bodpro { get; set; }
        public Nullable<System.DateTime> fecfact_fabrica { get; set; }
        public Nullable<int> numinv_vh { get; set; }
        public Nullable<long> numpedido_vh { get; set; }
        public decimal costototal_vh { get; set; }
        public decimal costosiniva_vh { get; set; }
        public float? iva_vh { get; set; }
        public Nullable<System.DateTime> fecfact_ccs { get; set; }
        public Nullable<System.DateTime> fecentman_vh { get; set; }
        public string nummanf_vh { get; set; }
        public Nullable<int> ciumanf_vh { get; set; }
        public Nullable<int> diaslibres_vh { get; set; }
        public Nullable<System.DateTime> fecman_vh { get; set; }
        public Nullable<System.DateTime> fecllegadaccs_vh { get; set; }
        public Nullable<System.DateTime> fecentregatransp_vh { get; set; }
        public Nullable<System.DateTime> fecharunt_vh { get; set; }
        public Nullable<int> tramitador_id { get; set; }
        public Nullable<int> proveedor_id { get; set; }
        public Nullable<int> arch_fact_id { get; set; }
        public string icbvh_estatus { get; set; }
        public Nullable<System.DateTime> icbvh_fecinsp_ingreso { get; set; }
        public Nullable<System.DateTime> icbvh_fec_impronta { get; set; }
        public Nullable<System.DateTime> icbvh_fecing_bateria { get; set; }
        public Nullable<System.DateTime> icbvh_fec_envioimpronta { get; set; }
        public Nullable<System.DateTime> icbvh_fec_recepcionimpronta { get; set; }
        public Nullable<int> ciudadplaca { get; set; }
        public Nullable<int> clanegocio_id { get; set; }
        public Nullable<int> propietario { get; set; }
        public string bodegaGM { get; set; }
        public string codigo_pago { get; set; }
        public string Notas { get; set; }
        public bool flota { get; set; }
        public Nullable<decimal> impconsumo { get; set; }
        public Nullable<bool> nuevo { get; set; }
        public Nullable<bool> usado { get; set; }
        public Nullable<long> asignado { get; set; }
        public Nullable<int> id_evento { get; set; }
        public Nullable<int> clasificacion_id { get; set; }
        public string Numerogarantia { get; set; }
        public Nullable<System.DateTime> fecha_garantia { get; set; }
        public Nullable<int> tiempogarantia { get; set; }
        public long kmgarantia { get; set; }
        public long kilometraje { get; set; }
        public string numerosoat { get; set; }
        public Nullable<int> nitaseguradora { get; set; }
        public Nullable<System.DateTime> fecha_venta { get; set; }
        public Nullable<System.DateTime> fecha_fin_garantia { get; set; }
        public Nullable<System.DateTime> fecha_tecnomecanica { get; set; }
        public Nullable<int> tiposervicio { get; set; }
        public decimal iva_vh_venta { get; set; }
        public Nullable<System.DateTime> fecmatricula { get; set; }
        public Nullable<int> ubicacionactual { get; set; }
        public Nullable<int> idasesor { get; set; }
        public Nullable<int> nitprenda { get; set; }
        public Nullable<System.DateTime> fecha_entrega { get; set; }
        public Nullable<System.DateTime> fecha_soat { get; set; }
        public string equipamento { get; set; }


        /**************lineas****************/
        public int id { get; set; }
        public int id_encabezado { get; set; }
        public string codigo { get; set; }
        public int seq { get; set; }
        public System.DateTime fec { get; set; }
        public int nit { get; set; }
        public decimal cantidad { get; set; }
        public Nullable<float> porcentaje_iva { get; set; }
        public decimal valor_unitario { get; set; }
        public Nullable<float> porcentaje_descuento { get; set; }
        public decimal costo_unitario { get; set; }
        public string adicional { get; set; }
        public Nullable<int> vendedor { get; set; }
        public int bodega { get; set; }
        public string und { get; set; }
        public decimal cantidad_und { get; set; }
        public decimal cantidad_pedida { get; set; }
        public decimal costo_unitario_sin { get; set; }
        public Nullable<int> pedido { get; set; }
        public float cantidad_otra_und { get; set; }
        public Nullable<int> telefono { get; set; }
        public string tipo_op { get; set; }
        public Nullable<int> numero_op { get; set; }
        public decimal cantidad_devuelta { get; set; }
        public string tipo_link { get; set; }
        public Nullable<int> numero_link { get; set; }
        public Nullable<int> seq_link { get; set; }
        public Nullable<short> cantidad_dos { get; set; }
        public string serial { get; set; }
        public Nullable<float> porc_dcto_2 { get; set; }
        public Nullable<float> porc_dcto_3 { get; set; }
        public Nullable<int> Id_Documentos_lin_ped { get; set; }
        public string desc_adicional { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<float> impoconsumo { get; set; }
        public decimal costo_niff { get; set; }
        public Nullable<int> moneda { get; set; }
        public Nullable<decimal> tasa { get; set; }
    }
}