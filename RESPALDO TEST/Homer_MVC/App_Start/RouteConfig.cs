using System.Web.Mvc;
using System.Web.Routing;

namespace Homer_MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "tareas Inspeccion",
                "tareasInspeccion",
                new { controller = "InspeccionVh", action = "TareasInspeccion" }
            );


            routes.MapRoute(
                "inicio tecnico",
                "inicioTecnico",
                new { controller = "Inicio", action = "inicioTecnico" }
            );

            routes.MapRoute(
              "Motivo Reprogramacion de Cita",
              "MtivoReprogramacionCita",
              new { controller = "agendaTallerServicio", action = "MtivoReprogramacionCita" }
          );

            routes.MapRoute(
              "Dashboard Analista Repuestos",
              "infotraslados",
              new { controller = "trasladoRepuestos", action = "infotraslados" }
          );

           routes.MapRoute(
              "ttarifastot",
              "ttarifastot",
              new { controller = "ordenTaller", action = "ttarifastot" }
           );

            routes.MapRoute(
                "items Inspeccion",
                "itemsInspeccion",
                new { controller = "itemInspeccion", action = "Create" }
            );

            routes.MapRoute(
              "insumos",
              "insumos",
              new { controller = "Insumos", action = "Index" }
          );
            routes.MapRoute(
         "retiros internos",
         "retirosinternos",
         new { controller = "ordenTaller", action = "retirosinternos" }
     );

            routes.MapRoute(
              "Administracion de Garantias",
              "AdmonGarantias",
              new { controller = "ordenTaller", action = "AdmonGarantias" }
          );
            routes.MapRoute(
                "categorias Inspeccion",
                "categoriasInspeccion",
                new { controller = "categoriaInspeccion", action = "Create" }
            );
            routes.MapRoute(
             "Estados Garantias",
             "EstadosGarantias",
             new { controller = "ordenTaller", action = "EstadosGarantias" }
         );
            routes.MapRoute(
         "Descuento Maximomo",
         "descuentomaximomo",
         new { controller = "DescuentoMax", action = "Index" }
     );
            routes.MapRoute(
                "conceptos Inspeccion",
                "conceptosInspeccion",
                new { controller = "conceptoInspeccion", action = "Create" }
            );

            routes.MapRoute(
                "motivospausaot",
                "motivospausaot",
                new { controller = "ordenTaller", action = "motivospausaot" }
            );
            routes.MapRoute(
             "tiempoConsultas",
             "tiempoConsultas",
             new { controller = "parametros_sistema", action = "TiempoConsultas" }
         );
            routes.MapRoute(
                "subfuentes",
                "subfuentes",
                new { controller = "subfuente", action = "Create" }
            );

            routes.MapRoute(
                "definicion Contable",
                "definicionContable",
                new { controller = "definicionContable", action = "Create" }
            );

            routes.MapRoute(
                "matrices Mantenimiento",
                "matricesMantenimiento",
                new { controller = "matricesMantenimiento", action = "Create" }
            );

            routes.MapRoute(
                name: "Medios de Pago",
                url: "bancos",
                defaults: new { controller = "bancos", action = "formasPago" }
             );

            routes.MapRoute(
                "itemInventario",
                "itemInventario",
                new { controller = "itemInventario", action = "Index" }
            );

            routes.MapRoute(
                "libros Contables",
                "librosContables",
                new { controller = "librosContables", action = "CuentaMayor" }
            );

            routes.MapRoute(
                "ordenTaller",
                "ordenTaller",
                new { controller = "ordenTaller", action = "Create" }
            );

            routes.MapRoute(
                "empresas",
                "empresas",
                new { controller = "empresa", action = "Create" }
            );

            routes.MapRoute(
                "departamentos Gerenciales",
                "departamentosGerenciales",
                new { controller = "departamentoGerencial", action = "Create" }
            );

            routes.MapRoute(
                "tipo Circular",
                "tipoCircular",
                new { controller = "tipoCircular", action = "Create" }
            );

            routes.MapRoute(
                "administracion Circulares",
                "administracionCirculares",
                new { controller = "administracionCirculares", action = "Index" }
            );
            routes.MapRoute(
            "Facturacion Garantias",
            "FacGarantias",
            new { controller = "FacGarantias", action = "Index" }
        );


            routes.MapRoute(
                "administracion Contrato",
                "administracionContrato",
                new { controller = "administracionContrato", action = "Create" }
            );

            routes.MapRoute(
                "tipo Contrato",
                "tipoContrato",
                new { controller = "tipoContrato", action = "Create" }
            );

            routes.MapRoute(
                "grupo Centro Costo",
                "grupoCentroCosto",
                new { controller = "grupoCentroCosto", action = "Create" }
            );

            routes.MapRoute(
                "combos",
                "tcombos",
                new { controller = "Tcombos", action = "Index" }
            );

            routes.MapRoute(
                "abrir Mes",
                "abrirMes",
                new { controller = "abrirMes", action = "Index" }
            );


            routes.MapRoute(
                "cierreMes",
                "cierreMes",
                new { controller = "cierreMes", action = "Index" }
            );


            routes.MapRoute(
                "consulta General Cuentas",
                "consultaGeneralCuentas",
                new { controller = "consultaGeneralCuentas", action = "Index" }
            );

            routes.MapRoute(
                "nota debito proveedores",
                "notaDebitoProveedor",
                new { controller = "notaDebitoProveedor", action = "Create" }
            );

            routes.MapRoute(
                "nota credito proveedores",
                "notaCreditoProveedor",
                new { controller = "notaCreditoProveedor", action = "Create" }
            );

            routes.MapRoute(
                "Notificacion Correo",
                "enviosmsnotf",
                new { controller = "SignosVitales", action = "notificacionOperacion" }
            );

            routes.MapRoute(
                "Notificacion Correo Taller",
                "enviosmsnotfope",
                new { controller = "OrdenTaller", action = "notificacionOperacion" }
            );

            //routes.MapRoute(
            //	name: "nota debito clientes",
            //	url: "notaDebito",
            //	defaults: new { controller = "notaDebito", action = "Create" }
            //  );


            //routes.MapRoute(
            // name: "nota credito clientes",
            // url: "notaCredito",
            // defaults: new { controller = "notaCredito", action = "Create" }
            //  );

            routes.MapRoute(
                "remision Clientes",
                "remisionClientes",
                new { controller = "remisionClientes", action = "Create" }
            );

            routes.MapRoute(
                "ubicacionesvehiculo",
                "ubivh",
                new { controller = "ubicacion_vh", action = "Create" }
            );

            routes.MapRoute(
                "listar asesor",
                "asesor",
                new { controller = "asesor", action = "Listar" }
            );

            routes.MapRoute(
                "conversion moneda",
                "moneda_conversion",
                new { controller = "moneda_conversion", action = "Create" }
            );

            routes.MapRoute(
                "recibo caja",
                "ReciboCaja",
                new { controller = "ReciboCaja", action = "Create" }
            );

            routes.MapRoute(
                "mov contable",
                "movcontables",
                new { controller = "movcontables", action = "Create" }
            );

            routes.MapRoute(
                "agenda Taller Servicio",
                "agendaTallerServicio",
                new { controller = "agendaTallerServicio", action = "Create" }
            );

            routes.MapRoute(
                "horario Taller",
                "horarioTaller",
                new { controller = "horarioTaller", action = "Create" }
            );

            routes.MapRoute(
                "separacion de mercancias",
                "ConfiguracionAnticipo",
                new { controller = "bodega_concesionario", action = "ConfiguracionAnticipo" }
            );

            routes.MapRoute(
               "Configuracion Anticipos",
               "rseparacionmercancias",
               new { controller = "rseparacionmercancias", action = "Create" }
           );
            routes.MapRoute(
                "razones de ingreso",
                "trazonesingreso",
                new { controller = "trazonesingreso", action = "Create" }
            );

            routes.MapRoute(
                "tipo Bahia",
                "tipoBahia",
                new { controller = "tipoBahia", action = "Create" }
            );

            routes.MapRoute(
                "motivo Entrada Cita",
                "motivoEntradaCita",
                new { controller = "motivoEntradaCita", action = "Create" }
            );

            routes.MapRoute(
                "tipo Cita Taller",
                "tipoCitaTaller",
                new { controller = "tipoCitaTaller", action = "Create" }
            );

            routes.MapRoute(
                "motivo Cita Cancelada",
                "motivoCitaCancelada",
                new { controller = "motivoCitaCancelada", action = "Create" }
            );

            routes.MapRoute(
                "estados Citas Taller",
                "estadosCitasTaller",
                new { controller = "estadosCitasTaller", action = "Create" }
            );

            //routes.MapRoute(
            // name: "devolucion",
            // url: "DevolucionFacturacionRepuestos",
            // defaults: new { controller = "DevolucionFacturacionRepuestos", action = "Index" }
            //  );

            routes.MapRoute(
                "cheque Posfechado",
                "chequePosfechado",
                new { controller = "chequePosfechado", action = "Create" }
            );

            routes.MapRoute(
                "bahia",
                "bahia",
                new { controller = "bahia", action = "Create" }
            );

            routes.MapRoute(
                "tecnicos",
                "tecnicos",
                new { controller = "tecnicos", action = "Create" }
            );

            routes.MapRoute(
                "tipo Tecnico",
                "tipoTecnico",
                new { controller = "tipoTecnico", action = "Create" }
            );

            routes.MapRoute(
                "Autorizaciones Repuestos",
                "BrowserAutorizacionesRepuestos",
                new { controller = "averias_users", action = "BrowserAutorizacionesRepuestos" }
            );

            //routes.MapRoute(
            // name: "facturacion repuestos",
            // url: "FacturacionRepuestos",
            // defaults: new { controller = "FacturacionRepuestos", action = "Facturar" }
            //  );

            routes.MapRoute(
                "tarifa Taller Cliente",
                "tarifaTallerCliente",
                new { controller = "tarifaTallerCliente", action = "Create" }
            );

            routes.MapRoute(
                "cotizacion Taller",
                "cotizacionTaller",
                new { controller = "cotizacionTaller", action = "Create" }
            );

            routes.MapRoute(
                "clasificacion abc",
                "rparametrizacionabcs",
                new { controller = "rparametrizacionabcs", action = "Create" }
            );

            routes.MapRoute(
                "aseguradora",
                "aseguradora",
                new { controller = "aseguradora", action = "Create" }
            );

            routes.MapRoute(
                "tipo Documento Aseguradora",
                "tipoDocumentoAseguradora",
                new { controller = "tipoDocumentoAseguradora", action = "Create" }
            );

            routes.MapRoute(
                "tarifas Taller",
                "tarifasTaller",
                new { controller = "tarifasTaller", action = "Create" }
            );

            routes.MapRoute(
                "Tipo Operacion Tempario",
                "tpOperacionTempario",
                new { controller = "tpOperacionTempario", action = "Create" }
            );

            routes.MapRoute(
                "tempario",
                "tempario",
                new { controller = "tempario", action = "Create" }
            );

            routes.MapRoute(
                "campanas Taller",
                "campanasTaller",
                new { controller = "campanasTaller", action = "Create" }
            );

            routes.MapRoute(
                "pedido Sugerido",
                "pedidoSugerido",
                new { controller = "pedidoSugerido", action = "Create" }
            );

            routes.MapRoute(
                "solicitud Repuestos",
                "solicitudRepuestos",
                new { controller = "solicitudRepuestos", action = "Create" }
            );

            //routes.MapRoute(
            //  name: "bajar Costo Referencia",
            //  url: "bajarCostoReferencia",
            //  defaults: new { controller = "bajarCostoReferencia", action = "Create" }
            //);

            //routes.MapRoute(
            //  name: "subir Costo Referencia",
            //  url: "subirCostoReferencia",
            //  defaults: new { controller = "subirCostoReferencia", action = "Create" }
            //);

            //routes.MapRoute(
            //  name: "salidas Repuestos",
            //  url: "salidasRepuestos",
            //  defaults: new { controller = "salidasRepuestos", action = "Create" }
            //);

            //routes.MapRoute(
            //  name: "entradas Repuestos",
            //  url: "entradasRepuestos",
            //  defaults: new { controller = "entradasRepuestos", action = "Create" }
            //);

            //routes.MapRoute(
            //  name: "traslado Repuestos",
            //  url: "trasladoRepuestos",
            //  defaults: new { controller = "trasladoRepuestos", action = "Create" }
            //);


            routes.MapRoute(
                "cargue referencias alternas",
                "rremplazos",
                new { controller = "rremplazos", action = "Index" }
            );

            routes.MapRoute(
                "Codigos iva",
                "codigo_iva",
                new { controller = "codigo_iva", action = "Create" }
            );

            routes.MapRoute(
                "Codigos Flota",
                "vcodflota",
                new { controller = "vcodflota", action = "Create" }
            );

            routes.MapRoute(
                "vehiculos por entregar",
                "entregaVehiculos",
                new { controller = "EventoVehiculo", action = "BrowserAnfitriona" }
            );

            routes.MapRoute(
                "documentos flota",
                "vdocumentosflota",
                new { controller = "vdocumentosflota", action = "Create" }
            );

            routes.MapRoute(
                "averias users",
                "averias_users",
                new { controller = "averias_users", action = "Create" }
            );

            routes.MapRoute(
                "averias autorizacion",
                "autorizacion_averias",
                new { controller = "averias_users", action = "BrowserAutorizaciones" }
            );

            routes.MapRoute(
                "orden Compra Proveedor",
                "ordenCompraProveedor",
                new { controller = "ordenCompraProveedor", action = "Create" }
            );

            routes.MapRoute(
                "Tipo Compra Repuesto",
                "TipoCompraRepuesto",
                new { controller = "TipoCompraRepuesto", action = "Create" }
            );

            routes.MapRoute(
                "check List Vehiculos",
                "checkListVehiculos",
                new { controller = "checkListVehiculos", action = "Create" }
            );

            routes.MapRoute(
                "check List Usados",
                "checkListUsados",
                new { controller = "checkListUsados", action = "Create" }
            );

            //routes.MapRoute(
            //  name: "traslados",
            //  url: "traslados",
            //  defaults: new { controller = "traslados", action = "Create" }
            //);

            routes.MapRoute(
                "tipos carroceria",
                "tipo_carroceria",
                new { controller = "tipo_carroceria", action = "Create" }
            );

            routes.MapRoute(
                "valores tramites",
                "valores_tramites",
                new { controller = "valores_tramites", action = "Create" }
            );

            routes.MapRoute(
                "calculo tramites",
                "calculoTramites",
                new { controller = "calculoTramites", action = "Index" }
            );

            routes.MapRoute(
                "encuesta llamada de restate",
                "encuestaLlamadaRescate",
                new { controller = "responderEncuestas", action = "LlamadaRescate" }
            );

            routes.MapRoute(
                "asignacion Plantas",
                "asignacionPlantas",
                new { controller = "asignacionPlantas", action = "Index" }
            );

            //routes.MapRoute(
            //  name: "devolucion Compras",
            //  url: "devolucionCompras",
            //  defaults: new { controller = "devolucionCompras", action = "Index" }
            //);

            routes.MapRoute(
                "venta Tipo Cliente",
                "vantaTipoCliente",
                new { controller = "ventaTipoCliente", action = "Index" }
            );

            routes.MapRoute(
                "check List Entregas",
                "checkListEntregas",
                new { controller = "checkListEntregas", action = "Index" }
            );

            routes.MapRoute(
                "crm Menu",
                "crmMenu",
                new { controller = "crmMenu", action = "Index" }
            );


            routes.MapRoute(
                "modelos vehiculo",
                "vmodelogs",
                new { controller = "vmodelogs", action = "Create" }
            );

            //routes.MapRoute(
            //  name: "devolucion Venta Automatica",
            //  url: "devolucionVentaAutomatica",
            //  defaults: new { controller = "devolucionVentaAutomatica", action = "Index" }
            //);

            routes.MapRoute(
                "carga CRM",
                "cargaCRM",
                new { controller = "cargaCRM", action = "Index" }
            );


            routes.MapRoute(
                "ubicacion repuesto bodega",
                "ubicacion_repuestobod",
                new { controller = "ubicacion_repuestobod", action = "Create" }
            );

            routes.MapRoute(
                "consultar documnetos por tercero",
                "ventaTipoCliente",
                new { controller = "ventaTipoCliente", action = "ConsultaPorTercero" }
            );

            routes.MapRoute(
                "responder encuestas",
                "responderEncuestas",
                new { controller = "responderEncuestas", action = "Index" }
            );

            routes.MapRoute(
                "indicadores asesor",
                "indicadoresAsesor",
                new { controller = "indicadoresAsesor", action = "Index" }
            );

            routes.MapRoute(
                "carga delima",
                "browserDelima",
                new { controller = "consultaEventos", action = "BrowserDelima" }
            );

            routes.MapRoute(
                "kit Accesorios",
                "kitsaccesorios",
                new { controller = "kitsaccesorios", action = "Create" }
            );

            routes.MapRoute(
                "consulta Eventos",
                "consultaEventos",
                new { controller = "consultaEventos", action = "Index" }
            );

            routes.MapRoute(
                "consulta Documentos",
                "consultaDocumentos",
                new { controller = "consultaDocumentos", action = "Index" }
            );

            routes.MapRoute(
                "encuestas modulo",
                "crm_encuesta_modulo",
                new { controller = "crm_encuesta_modulo", action = "Create" }
            );

            routes.MapRoute(
                "encuestas",
                "crm_encuestas",
                new { controller = "crm_encuestas", action = "Create" }
            );

            routes.MapRoute(
                "creacion encuestas",
                "creacionEncuentas",
                new { controller = "creacionEncuentas", action = "Index" }
            );

            routes.MapRoute(
                "pedido_tliberacion",
                "pedido_tliberacion",
                new { controller = "pedido_tliberacion", action = "Create" }
            );

            routes.MapRoute(
                "tp_vehiculo",
                "tp_vehiculo",
                new { controller = "tp_vehiculo", action = "Create" }
            );

            routes.MapRoute(
                "Clientes Activos",
                "clientesActivos",
                new { controller = "icb_terceros", action = "clientesActivos" }
            );

            routes.MapRoute(
                "Plan Financiero",
                "plan_financiero",
                new { controller = "plan_financiero", action = "Create" }
            );

            //routes.MapRoute(
            // name: "Facturacion Nuevos",
            // url: "facturacionNuevos",
            // defaults: new { controller = "facturacionNuevos", action = "Create" }
            //);

            routes.MapRoute(
            name: "facturacion comisiones",
            url: "FacturacionComisiones",
            defaults: new { controller = "FacturacionComisiones", action = "Index" }
              );

            routes.MapRoute(
                "indicadores creditos",
                "IndicadoresCreditos",
                new { controller = "IndicadoresCreditos", action = "Index" }
            );

            routes.MapRoute(
                "bonos de fabrica",
                "vdescuentoscondicionados",
                new { controller = "vdescuentoscondicionados", action = "Create" }
            );

            routes.MapRoute(
                "FacturaComision",
                "FacturaComision",
                new { controller = "v_creditos", action = "FacturaComision" }
            );

            routes.MapRoute(
                "Perfil Contable Documento",
                "PerfilContableDocumento",
                new { controller = "perfilContableDocumento", action = "Create" }
            );


            routes.MapRoute(
                "Perfil Contable Referencia",
                "PerfilContableReferencia",
                new { controller = "perfilContableReferencia", action = "Create" }
            );

            routes.MapRoute(
                "Documento Por Bodega",
                "DocumentoPorBodega",
                new { controller = "DocumentoPorBodega", action = "Create" }
            );

            routes.MapRoute(
                "cargo matricula",
                "vcargomatricula",
                new { controller = "vcargomatricula", action = "Create" }
            );

            routes.MapRoute(
                "Resolucion Facturas",
                "resolucionFacturas",
                new { controller = "resolucionFacturas", action = "Create" }
            );

            routes.MapRoute(
                "origenProspecto",
                "origenProspecto",
                new { controller = "origenProspecto", action = "Create" }
            );

            routes.MapRoute(
                "sitiosOrigen",
                "sitiosOrigen",
                new { controller = "sitiosOrigen", action = "Create" }
            );


            routes.MapRoute(
                "forma de pago pedidos",
                "condicionPagoPedidos",
                new { controller = "vformapagoes", action = "Create" }
            );

            routes.MapRoute(
                "parametrizacion horario",
                "parametrizacionHorario",
                new { controller = "parametrizacionHorario", action = "Create" }
            );

            routes.MapRoute(
                "cargue precios matriculas",
                "vpmatriculas",
                new { controller = "vpmatriculas", action = "Index" }
            );

            routes.MapRoute(
                "cargue precios polizas",
                "vppolizas",
                new { controller = "vppolizas", action = "Index" }
            );

            routes.MapRoute(
                "adecuaciones peritaje",
                "adecuacionesPeritaje",
                new { controller = "InspeccionPeritaje", action = "BrowserAdecuaciones" }
            );

            routes.MapRoute(
                "accesoriosModelo",
                "accesoriosModelo",
                new { controller = "accesoriosModelo", action = "Create" }
            );

            routes.MapRoute(
                "registrarEvento",
                "registrarEvento",
                new { controller = "EventoVehiculo", action = "Registrar" }
            );

            routes.MapRoute(
                "compraRetoma",
                "compraRetoma",
                new { controller = "peritaje", action = "CompraRetoma" }
            );


            routes.MapRoute(
                "peritajes",
                "peritajes",
                new { controller = "peritaje", action = "Peritajes" }
            );

            routes.MapRoute(
                "solicitudes",
                "solicitudes",
                new { controller = "peritaje", action = "Solicitud" }
            );

            routes.MapRoute(
                "agendaPerito",
                "agendaPerito",
                new { controller = "peritaje", action = "Agendar" }
            );


            routes.MapRoute(
                "aprobacion peritajes",
                "aprobacionPeritajes",
                new { controller = "InspeccionPeritaje", action = "Browser" }
            );

            routes.MapRoute(
                "vehiculos disponibles",
                "vehiculosDisponibles",
                new { controller = "Vehiculo", action = "BrowserAsesor" }
            );

            routes.MapRoute(
                "cargue lista precio nuevos",
                "vlisnuevos",
                new { controller = "vlisnuevos", action = "Index" }
            );

            routes.MapRoute(
                "browser asesor lista precios",
                "browserAsesor",
                new { controller = "vlisnuevos", action = "browserAsesorNuevos" }
            );

            routes.MapRoute(
                "pedidos vehiculos",
                "vpedidos",
                new { controller = "vpedidos", action = "Create" }
            );

            routes.MapRoute(
                "crear demos",
                "demos",
                new { controller = "demos", action = "Create" }
            );

            routes.MapRoute(
                "agenda demos",
                "agenda_demos",
                new { controller = "agenda_demos", action = "index" }
            );

            routes.MapRoute(
                "agenda asesor",
                "agendaAsesor",
                new { controller = "agenda_asesor", action = "index" }
            );

            routes.MapRoute(
                "creacion codigos flota",
                "vflotas",
                new { controller = "vflotas", action = "Create" }
            );

            routes.MapRoute(
                "Lista precios usados",
                "vlisretomas",
                new { controller = "vlisretomas", action = "Index" }
            );

            routes.MapRoute(
                "Solicitud credito",
                "Creditos",
                new { controller = "v_creditos", action = "Create" }
            );

            routes.MapRoute(
                "perfil tributario",
                "perfilTributario",
                new { controller = "perfilTributario", action = "Create" }
            );

            routes.MapRoute(
                "lista precios",
                "rprecios",
                new { controller = "rprecios", action = "Index" }
            );

            routes.MapRoute(
                "Monedas",
                "monedas",
                new { controller = "monedas", action = "Create" }
            );

            routes.MapRoute(
                "Motivo Compra",
                "motcompras",
                new { controller = "motcompras", action = "Create" }
            );

            routes.MapRoute(
                "Bancos",
                "bancos",
                new { controller = "bancos", action = "Create" }
            );

            routes.MapRoute(
                "Tipo Proveedor",
                "tipoproveedor",
                new { controller = "tipoproveedor", action = "Create" }
            );

            routes.MapRoute(
                "Tipo Clientes",
                "tipoclientes",
                new { controller = "tipoclientes", action = "Create" }
            );

            routes.MapRoute(
                "Unidad de Medida",
                "unidad_medida",
                new { controller = "unidad_medida", action = "Create" }
            );

            routes.MapRoute(
                "Subgrupo repuestos",
                "ref_subgrupo",
                new { controller = "ref_subgrupo", action = "Create" }
            );

            routes.MapRoute(
                "Linea repuestos",
                "ref_linea",
                new { controller = "ref_linea", action = "Create" }
            );

            routes.MapRoute(
                "Grupo repuestos",
                "ref_grupo",
                new { controller = "ref_grupo", action = "Create" }
            );

            routes.MapRoute(
                "Familia repuestos",
                "ref_familia",
                new { controller = "ref_familia", action = "Create" }
            );

            routes.MapRoute(
                "eventoVh",
                "eventoVh",
                new { controller = "EventoVehiculo", action = "Crear" }
            );

            routes.MapRoute(
                "creacion Vehiculo",
                "CrearVehiculocita",
                new { controller = "Vehiculo", action = "CrearVehiculocita" }
            );
            routes.MapRoute(
             "creacionVh",
             "creacionVh",
             new { controller = "Vehiculo", action = "Crear" }
         );

            routes.MapRoute(
                "Ubicación General",
                "ubicacion_bodega",
                new { controller = "ubicacion_bodega", action = "Create" }
            );

            routes.MapRoute(
                "Compra Repuestos",
                "compraRepuestos",
                new { controller = "compraRepuestos", action = "Index" }
            );


            routes.MapRoute(
                "prefijoDoc",
                "prefijoDoc",
                new { controller = "prefijoDocumento", action = "Crear" }
            );


            routes.MapRoute(
                "naturalezaDoc",
                "naturalezaDoc",
                new { controller = "naturalezaDocumento", action = "Crear" }
            );


            routes.MapRoute(
                "Financieras",
                "Financieras",
                new { controller = "Financieras", action = "Crear" }
            );


            routes.MapRoute(
                "grupoModelo",
                "grupoModelo",
                new { controller = "grupoModelo", action = "Crear" }
            );

            routes.MapRoute(
                "claseModelo",
                "claseModelo",
                new { controller = "claseModelo", action = "Crear" }
            );

            routes.MapRoute(
                "perfilModelo",
                "perfilModelo",
                new { controller = "perfilModelo", action = "Crear" }
            );

            routes.MapRoute(
                "tipoModelo",
                "tipoModelo",
                new { controller = "tipoModelo", action = "Crear" }
            );


            routes.MapRoute(
                "TipoTramite",
                "tpTramite",
                new { controller = "tipoTramite", action = "Crear" }
            );


            routes.MapRoute(
                "Referencias",
                "registroReferencias",
                new { controller = "registroReferencias", action = "Crear" }
            );

            routes.MapRoute(
                "Estado de Prospectos",
                "statusProspecto",
                new { controller = "statusProspecto", action = "Crear" }
            );

            routes.MapRoute(
                "Tipo Llamadas de Rescate",
                "tpLlamadaRescate",
                new { controller = "tpLlamadaRescate", action = "Crear" }
            );

            routes.MapRoute(
                "Llamada de Rescate",
                "llamadaRescate",
                new { controller = "llamadaRescate", action = "Index" }
            );

            routes.MapRoute(
                "paramSistema Peritaje",
                "paramSistema",
                new { controller = "parametros_sistema", action = "Crear" }
            );

            routes.MapRoute(
                "Convenciones Peritaje",
                "convenciones",
                new { controller = "convenciones", action = "Crear" }
            );

            routes.MapRoute(
                "Zonas Peritaje",
                "zonasPeritaje",
                new { controller = "zonasPeritaje", action = "Crear" }
            );

            routes.MapRoute(
                "Piezas Peritaje",
                "piezasPeritaje",
                new { controller = "piezasPeritaje", action = "Crear" }
            );

            routes.MapRoute(
                "Inspeccion Peritaje",
                "InspeccionPeritaje",
                new { controller = "InspeccionPeritaje", action = "InspeccionPeritaje" }
            );

            routes.MapRoute(
                "Recepcion Improntas",
                "recepcion_improntas",
                new { controller = "recepcion_improntas", action = "recepcion_improntas" }
            );

            routes.MapRoute(
                "Control Improntas",
                "control_improntas",
                new { controller = "control_improntas", action = "envio_improntas" }
            );

            routes.MapRoute(
                "toma_improntas",
                "toma_improntas",
                new { controller = "toma_improntas", action = "toma_improntas" }
            );

            routes.MapRoute(
                "gravedad_averias",
                "gravedad_averias",
                new { controller = "gravedad_averias", action = "Crear" }
            );


            routes.MapRoute(
                "averias",
                "averias",
                new { controller = "averias", action = "crear" }
            );

            routes.MapRoute(
                "areas",
                "areas",
                new { controller = "areas", action = "Crear" }
            );

            routes.MapRoute(
                "cargos",
                "cargos",
                new { controller = "cargos", action = "crear" }
            );

            //routes.MapRoute(
            //    name: "cliente",
            //    url: "cliente",
            //    defaults: new { controller = "tercero_cliente", action = "tercero_cliente" }
            //);

            routes.MapRoute(
                "tercero",
                "tercero",
                new { controller = "icb_terceros", action = "tercero" }
            );

            routes.MapRoute(
                "terceros",
                "terceros/{id}",
                new { controller = "icb_terceros", action = "tercero", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "clientesProspectos",
                "clientesProspectos",
                new { controller = "prospectos", action = "Create" }
            );

            routes.MapRoute(
                "act_eco",
                "act_eco",
                new { controller = "acteco_tercero", action = "Create" }
            );

            routes.MapRoute(
                "fpago",
                "fpago",
                new { controller = "fpago_tercero", action = "Create" }
            );

            routes.MapRoute(
                "cargueBaterias",
                "cargueBaterias",
                new { controller = "cargueBaterias", action = "Index" }
            );

            routes.MapRoute(
                "tp_imp",
                "tp_imp",
                new { controller = "tpimpu_tercero", action = "Create" }
            );

            routes.MapRoute(
                "tp_reg",
                "tp_reg",
                new { controller = "tpregimen_tercero", action = "Create" }
            );

            routes.MapRoute(
                "tp_sumin",
                "tp_sumin",
                new { controller = "tiposumi_tercero", action = "Create" }
            );

            routes.MapRoute(
                "genero",
                "genero",
                new { controller = "gen_tercero", action = "Create" }
            );

            routes.MapRoute(
                "tp_dpte",
                "tp_dpte",
                new { controller = "tp_Dpte", action = "Create" }
            );

            routes.MapRoute(
                "tp_hobby",
                "tp_hobby",
                new { controller = "tp_hobby", action = "Create" }
            );

            routes.MapRoute(
                "tp_ocup",
                "tp_ocup",
                new { controller = "tp_ocupacion", action = "Create" }
            );

            routes.MapRoute(
                "estado_civ",
                "estado_civ",
                new { controller = "estado_civil", action = "Create" }
            );

            routes.MapRoute(
                "clasf_terc",
                "clasf_terc",
                new { controller = "clasif_tercero", action = "Create" }
            );

            routes.MapRoute(
                "sector",
                "sector",
                new { controller = "nom_sector", action = "Create" }
            );

            routes.MapRoute(
                "rol",
                "rol",
                new { controller = "rols", action = "Create" }
            );

            routes.MapRoute(
                "ciudad",
                "ciudad",
                new { controller = "nom_ciudad", action = "Create" }
            );

            routes.MapRoute(
                "departamento",
                "dpto",
                new { controller = "nom_departamento", action = "Create" }
            );

            routes.MapRoute(
                "pais",
                "pais",
                new { controller = "nom_pais", action = "Create" }
            );

            routes.MapRoute(
                "tipodocumento",
                "tpdoc",
                new { controller = "tp_documento", action = "Create" }
            );


            routes.MapRoute(
                "colorvehiculo",
                "colvh",
                new { controller = "color_vh", action = "Create" }
            );

            routes.MapRoute(
                "marcavehiculo",
                "marvh",
                new { controller = "marca_vh", action = "Create" }
            );

            routes.MapRoute(
                "segmentovehiculo",
                "segvh",
                new { controller = "segmento_vh", action = "Create" }
            );

            routes.MapRoute(
                "tipomotorvehiculo",
                "tpmotvh",
                new { controller = "tipomotor_vh", action = "Create" }
            );

            routes.MapRoute(
                "tipocajavehiculo",
                "tpcajavh",
                new { controller = "tipocaja_vh", action = "Create" }
            );

            routes.MapRoute(
                "tiposerviciovehiculo",
                "tpservh",
                new { controller = "tiposervicio_vh", action = "Create" }
            );

            routes.MapRoute(
                "ubicacionvehiculo",
                "vformapagoes",
                new { controller = "ubicacion_vh", action = "Create" }
            );

            routes.MapRoute(
                "modelovehiculo",
                "modvh",
                new { controller = "modelo_vh", action = "Create" }
            );

            routes.MapRoute(
                "estilovehiculo",
                "estvh",
                new { controller = "estilo_vh", action = "Create" }
            );

            routes.MapRoute(
                "tipovehiculo",
                "tipovh",
                new { controller = "tipo_vh", action = "Create" }
            );

            routes.MapRoute(
                "clacompvehiculo",
                "clacompvh",
                new { controller = "clacomp_vh", action = "Create" }
            );

            routes.MapRoute(
                "clasificacionvehiculo",
                "clavh",
                new { controller = "clasificacion_vh", action = "Create" }
            );

            routes.MapRoute(
                "gruporepuesto",
                "grurpto",
                new { controller = "grupo_repuesto", action = "Create" }
            );

            routes.MapRoute(
                "ubicacionrepuesto",
                "ubirpto",
                new { controller = "ubicacion_repuesto", action = "Create" }
            );

            routes.MapRoute(
                "clasificacionrepuesto",
                "clarpto",
                new { controller = "clasificacion_repuesto", action = "Create" }
            );

            routes.MapRoute(
                "bodegarepuesto",
                "bodrpto",
                new { controller = "bodega_repuesto", action = "Create" }
            );

            routes.MapRoute(
                "inventariovehiculosnuevos",
                "invnv",
                new { controller = "inventario_vhNuevo", action = "Index" }
            );

            routes.MapRoute(
                "tipotramitadorvehiculo",
                "tptramvh",
                new { controller = "tptramitador_vh", action = "Create" }
            );

            routes.MapRoute(
                "tramitadorvehiculo",
                "tramvh",
                new { controller = "tramitador_vh", action = "Create" }
            );

            routes.MapRoute(
                "areabodega",
                "areabod",
                new { controller = "area_bodega", action = "Create" }
            );

            routes.MapRoute(
                "bodegaconcesionario",
                "bodccs",
                new { controller = "bodega_concesionario", action = "Create" }
            );

            routes.MapRoute(
                "inventariovehiculosusados",
                "invuv",
                new { controller = "inventario_vhUsado", action = "Index" }
            );

            routes.MapRoute(
                "inventario Repuestos y Accesorios",
                "icbrptos",
                new { controller = "inventario_repuesto", action = "Create" }
            );


            routes.MapRoute(
                "movimientofacturacion",
                "movfac",
                new { controller = "movimiento_facturacion", action = "Create" }
            );

            routes.MapRoute(
                "documentofacturacion",
                "docfac",
                new { controller = "documento_facturacion", action = "Create" }
            );

            routes.MapRoute(
                "centrocosto",
                "centcst",
                new { controller = "centro_costo", action = "Create" }
            );

            routes.MapRoute(
                "cuentaspuc",
                "cntpuc",
                new { controller = "cuentas_puc", action = "Create" }
            );

            //routes.MapRoute(
            //    name: "peritajes",
            //    url: "peritaje",
            //    defaults: new { controller = "peritaje", action = "Agendar" }
            //);

            routes.MapRoute(
                "gestionVhNuevo",
                "gestionVhNuevo",
                new { controller = "gestionVhNuevo", action = "PedidoEnFirme" }
            );

            routes.MapRoute(
                "gestionVhUsado",
                "gestionVhUsado",
                new { controller = "gestionVhUsado", action = "Index" }
            );

            routes.MapRoute(
                "backofficeTramites",
                "backofficeTramites",
                new { controller = "backofficeTramites", action = "Index" }
            );

            routes.MapRoute(
                "facturacion",
                "facturacion",
                new { controller = "facturacion", action = "Index" }
            );

            routes.MapRoute(
                "agendaAlistamiento",
                "agendaAlistamiento",
                new { controller = "agendaAlistamiento", action = "Index" }
            );

            routes.MapRoute(
                "inventarioVehiculos",
                "inventarioVehiculos",
                new { controller = "inventarioVehiculos", action = "Index" }
            );

            routes.MapRoute(
                "pedidos",
                "pedidos",
                new { controller = "pedidos", action = "Index" }
            );

            routes.MapRoute(
                "salidas",
                "salidas",
                new { controller = "salidas", action = "Index" }
            );

            routes.MapRoute(
                "manifiesto",
                "manifiesto",
                new { controller = "manifiesto", action = "Index" }
            );

            routes.MapRoute(
                "carteraCliente",
                "carteraCliente",
                new { controller = "carteraCliente", action = "Index" }
            );

            routes.MapRoute(
                "cotizacion",
                "cotizacion",
                new { controller = "cotizacion", action = "Create" }
            );

            routes.MapRoute(
                "solicitudCredito",
                "solicitudCredito",
                new { controller = "solicitudCredito", action = "Index" }
            );

            routes.MapRoute(
                "modulos",
                "modulos",
                new { controller = "modulos", action = "Index" }
            );

            routes.MapRoute(
                "moduloEnDesarrollo",
                "moduloEnDesarrollo",
                new { controller = "ModuloDesarrollo", action = "Index" }
            );


            routes.MapRoute(
                "inspeccionVehiculos",
                "inspeccionVehiculos",
                new { controller = "InspeccionVh", action = "Index" }
            );

            //routes.MapRoute(
            //    name: "serviciosvehiculo",
            //    url: "tpservh",
            //    defaults: new { controller = "tiposervicio_vh", action = "Create" }
            //);

            routes.MapRoute(
                "login",
                "login",
                new { controller = "Login", action = "login" }
            );

            routes.MapRoute(
                "Inicio",
                "inicio",
                new { controller = "Inicio", action = "inicio" }
            );

            routes.MapRoute(
                "usuario",
                "usuario",
                new { controller = "users", action = "usuario" }
            );

            routes.MapRoute(
                "usuario update",
                "usuario/update/{id}",
                new { controller = "user", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tipodocumento update",
                "tpdoc/update/{id}",
                new { controller = "tp_documento", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "pais update",
                "pais/update/{id}",
                new { controller = "nom_pais", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "departamento update",
                "dpto/update/{id}",
                new { controller = "nom_departamento", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "ciudad update",
                "ciudad/update/{id}",
                new { controller = "nom_ciudad", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "rol update",
                "rol/update/",
                new { controller = "rols", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "sector update",
                "sector/update/{id}",
                new { controller = "nom_sector", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "clasf_terc update",
                "clasf_terc/update/{id}",
                new { controller = "clasif_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "estado_civ update",
                "estado_civ/update/{id}",
                new { controller = "estado_civil", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_ocup update",
                "tp_ocup/update/{id}",
                new { controller = "tp_ocupacion", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_hobby update",
                "tp_hobby/update/{id}",
                new { controller = "tp_hobby", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_dpte update",
                "tp_dpte/update/{id}",
                new { controller = "tp_Dpte", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "genero update",
                "genero/update/{id}",
                new { controller = "gen_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_sumin update",
                "tp_sumin/update/{id}",
                new { controller = "tiposumi_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_reg update",
                "tp_reg/update/{id}",
                new { controller = "tpregimen_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tp_imp update",
                "tp_imp/update/{id}",
                new { controller = "tpimpu_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
         "CreacionTOT",
         "CreacionTOT",
         new { controller = "ordenTaller", action = "CreacionTOT" }
     );
            routes.MapRoute(
                "fpago update",
                "fpago/update/{id}",
                new { controller = "fpago_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "act_eco update",
                "act_eco/update/{id}",
                new { controller = "acteco_tercero", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "tercero update",
                "tercero/update/{id}",
                new { controller = "icb_terceros", action = "update", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                "cliente update",
                "cliente/update/{id}",
                new { controller = "tercero_cliente", action = "update", id = UrlParameter.Optional }
            );

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Inicio", action = "inicio", id = UrlParameter.Optional }
            );


        }
    }
}