using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class VehiculoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        CultureInfo miCulturaTime = new CultureInfo("en-US");

        //Creo un arbol de expresión de datos
        /// <summary>
        ///     Los árboles de expresión son expresiones organizadas en una estructura de datos en forma de árbol. Cada nodo en el
        ///     árbol es una representación de una expresión, una expresión que es código. Una representación en memoria de una
        ///     expresión Lambda sería un árbol de expresión, que contiene los elementos reales (es decir, el código) de la
        ///     consulta, pero no su resultado. Los árboles de expresión hacen que la estructura de una expresión lambda sea
        ///     transparente y explícita.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static Expression<Func<vw_historial_vehiculos_nuevos, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_historial_vehiculos_nuevos), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_historial_vehiculos_nuevos, string>> lambda = Expression.Lambda<Func<vw_historial_vehiculos_nuevos, string>>(menuProperty, menu);

            return lambda;
        }

        public void ListasDesplegables(CrearVehiculoModel modelo)
        {
            //leo prueba de agregar asd

            int bod = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo.Where(x => x.mar_vh_id == modelo.marcvh_id),
                "modvh_codigo", "modvh_nombre", modelo.modvh_id);
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre", modelo.colvh_id);
            ViewBag.marcvh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre", modelo.marcvh_id);
            ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre", modelo.tpvh_id);
            ViewBag.ciumanf_vh = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", modelo.ciumanf_vh);
            ViewBag.tipo_servicio = new SelectList(context.tpservicio_vehiculo.OrderBy(x => x.tpserv_nombre),
                "tpserv_id", "tpserv_nombre", modelo.tipo_servicio);
            IQueryable<codigo_iva> fff = context.codigo_iva.Where(x => x.Descripcion == "COMPRA");
            IQueryable<codigo_iva> ggg = context.codigo_iva.Where(x => x.Descripcion == "VENTA");
            decimal cover = Convert.ToDecimal(modelo.iva_compra_vh,miCultura);
            decimal cover2 = Convert.ToDecimal(modelo.iva_vh,miCultura);
            ViewBag.iva_compra_vh = new SelectList(fff, "id", "Porcentaje",
                fff.Where(x => x.porcentaje == cover).Select(x => x.id).FirstOrDefault());
            ViewBag.iva_vh = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id", "Porcentaje",
                ggg.Where(x => x.porcentaje == cover).Select(x => x.id).FirstOrDefault());
            ViewBag.concepto = new SelectList(context.vconceptocompra, "codigo", "descripcion", modelo.codigo_pago);
            ViewBag.ubicacion =
                new SelectList(
                    context.ubicacion_bodega.Where(x => x.estado && x.idbodega == bod && x.tipo == 2)
                        .OrderBy(x => x.descripcion), "id", "descripcion", modelo.ubicacion);
            ViewBag.ciudadplaca = new SelectList(context.nom_ciudad.Where(x => x.ciu_estado).OrderBy(x => x.ciu_nombre),
                "ciu_id", "ciu_nombre", modelo.ciudadplaca);
            ViewBag.nitprenda =
                new SelectList(
                    context.icb_unidad_financiera.Where(x => x.financiera_estado).OrderBy(x => x.financiera_nombre),
                    "financiera_id", "financiera_nombre", modelo.nitprenda);
            ViewBag.impconsumo = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "IMPOCONSUMO"), "id",
                "Porcentaje", modelo.impconsumo);

            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombre = "(" + ter.doc_tercero + ") - " + ter.prinom_tercero + " " + ter.apellido_tercero + " " +
                                           ter.razon_social
                              }).ToList();
            ViewBag.proveedor_id = new SelectList(provedores, "idTercero", "nombre", modelo.proveedor_id);

            var propietario = from pro in context.tercero_cliente
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombre = ter.razon_social != null
                                      ? "(" + ter.doc_tercero + ") - " + ter.razon_social
                                      : "(" + ter.doc_tercero + ") - " + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                        ter.apellido_tercero + " " + ter.segapellido_tercero
                              };
            ViewBag.propietario = new SelectList(propietario, "idTercero", "nombre", modelo.propietario);

            var aseguradora = from aseg in context.icb_aseguradoras
                              join ter in context.icb_terceros
                                  on aseg.idtercero equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombre = "(" + ter.doc_tercero + ") - " + ter.razon_social + " " + ter.prinom_tercero + " " +
                                           ter.segnom_tercero + " " + ter.apellido_tercero + " " + ter.segapellido_tercero
                              };
            ViewBag.aseguradora_id = new SelectList(aseguradora, "idTercero", "nombre", modelo.aseguradora_id);

            var asesores = from x in context.users
                           where x.user_estado && (x.rol_id == 4 || x.rol_id == 2030)
                           select new
                           {
                               x.user_id,
                               nombre = x.user_nombre + " " + x.user_apellido
                           };
            ViewBag.idasesor = new SelectList(asesores.OrderBy(x => x.nombre), "user_id", "nombre", modelo.idasesor);
        }

        // GET: CreacionVh
        public ActionResult Crear(string planMayor, string placa, int? menu)
        {
            int bod = Convert.ToInt32(Session["user_bodega"]);
            BuscarFavoritos(menu);
            ListasDesplegables(new CrearVehiculoModel());
            ViewBag.iva_vh =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id", "Porcentaje");
            ViewBag.iva_compra_vh = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "COMPRA"), "id",
                "Porcentaje");
            ViewBag.impconsumo = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "IMPOCONSUMO"), "id",
                "Porcentaje");
            ViewBag.concepto = new SelectList(context.vconceptocompra, "codigo", "descripcion");
            ViewBag.ubicacion =
                new SelectList(
                    context.ubicacion_bodega.Where(x => x.estado && x.idbodega == bod && x.tipo == 2)
                        .OrderBy(x => x.descripcion), "id", "descripcion");
            ViewBag.ciudadplaca = new SelectList(context.nom_ciudad.Where(x => x.ciu_estado).OrderBy(x => x.ciu_nombre),
                "ciu_id", "ciu_nombre");
            ViewBag.nitprenda =
                new SelectList(
                    context.icb_unidad_financiera.Where(x => x.financiera_estado).OrderBy(x => x.financiera_nombre),
                    "financiera_id", "financiera_nombre");
            if (placa != null || placa != "")
            {
                ViewBag.placaParametro = placa;
            }
            else
            {
                ViewBag.placaParametro = "";
            }

            var asesores = from x in context.users
                           where x.user_estado && (x.rol_id == 4 || x.rol_id == 2030)
                           select new
                           {
                               x.user_id,
                               nombre = x.user_nombre + " " + x.user_apellido
                           };

            ViewBag.idasesor = new SelectList(asesores.OrderBy(x => x.nombre), "user_id", "nombre");

            var propietario = from pro in context.tercero_cliente
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombre = "(" + ter.doc_tercero + ") - " + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                           ter.apellido_tercero + " " + ter.segapellido_tercero
                              };

            ViewBag.propietario = new SelectList(propietario, "idTercero", "nombre");


            return View(new CrearVehiculoModel { nuevo = true, plan_mayor = planMayor });
        }

        [HttpPost]
        public ActionResult Crear(CrearVehiculoModel modelo, int? menu)
        {
            string ubicacion = Request["ubicacionVal"];
            if (ModelState.IsValid)
            {
                icb_vehiculo buscaPlanMayor = context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == modelo.plan_mayor);
                if (buscaPlanMayor != null)

                {
                    TempData["mensaje_error"] = "El plan mayor ya se encuentra registrado, por favor verifique...";
                }
                else
                {
                    icb_vehiculo crearVehiculo = new icb_vehiculo
                    {
                        plan_mayor = modelo.plan_mayor,
                        vin = modelo.vin,
                        plac_vh = modelo.plac_vh,
                        nummot_vh = modelo.nummot_vh,
                        marcvh_id = modelo.marcvh_id,
                        modvh_id = modelo.modvh_id,
                        anio_vh = modelo.anio_vh,
                        costosiniva_vh = Convert.ToDecimal(modelo.costosiniva_vh,miCultura),

                        tiposervicio = modelo.tipo_servicio,
                        iva_vh = Convert.ToDecimal(Request["textIVACompra"], miCultura),
                        impconsumo = Convert.ToDecimal(Request["text_impoconsumo"], miCultura),
                        iva_vh_venta = Convert.ToDecimal(Request["textIVAVenta"], miCultura),
                        codigo_pago = Request["valConcepto"],

                        costototal_vh = Convert.ToDecimal(modelo.costototal_vh, miCultura),
                        colvh_id = modelo.colvh_id,
                        tpvh_id = modelo.tpvh_id,
                        Notas = modelo.Notas,
                        flota = modelo.flota,
                        proveedor_id = modelo.proveedor_id,
                        ciumanf_vh = modelo.ciumanf_vh,
                        nummanf_vh = modelo.nummanf_vh,
                        nuevo = modelo.nuevo,
                        usado = modelo.usado,
                        nitaseguradora = modelo.aseguradora_id,
                        propietario = Convert.ToInt32(modelo.propietario),
                        kilometraje = (long)Convert.ToDecimal(modelo.kilometraje, miCultura),
                        Numerogarantia = modelo.Numerogarantia,
                        tiempogarantia = modelo.tiempogarantia,
                        kmgarantia = (long)Convert.ToDecimal(modelo.kmgarantia, miCultura),
                        numerosoat = modelo.numerosoat,
                        ciudadplaca = modelo.ciudadplaca,
                        nitprenda = modelo.nitprenda,
                        idasesor = modelo.idasesor,
                        icbvh_estado = modelo.icbvh_estado,
                        icbvhrazoninactivo = modelo.icbvhrazoninactivo
                    };
                    if (ubicacion == "")
                    {
                        crearVehiculo.ubicacionactual = null;
                    }
                    else
                    {
                        crearVehiculo.ubicacionactual = Convert.ToInt32(ubicacion);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecfact_fabrica))
                    {
                        crearVehiculo.fecfact_fabrica = DateTime.Parse(modelo.fecfact_fabrica, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecentman_vh))
                    {
                        crearVehiculo.fecentman_vh = DateTime.Parse(modelo.fecentman_vh, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_garantia))
                    {
                        crearVehiculo.fecha_garantia = DateTime.Parse(modelo.fecha_garantia, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_venta))
                    {
                        crearVehiculo.fecha_venta = DateTime.Parse(modelo.fecha_venta, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_tecnomecanica))
                    {
                        crearVehiculo.fecha_tecnomecanica = DateTime.Parse(modelo.fecha_tecnomecanica, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_matricula))
                    {
                        crearVehiculo.fecmatricula = DateTime.Parse(modelo.fecha_matricula, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_soat))
                    {
                        crearVehiculo.fecha_soat = DateTime.Parse(modelo.fecha_soat, miCulturaTime);
                    }
                    icb_terceros buscarNitPropietario =
                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.propietario);
                    if (buscarNitPropietario != null)
                    {
                        crearVehiculo.propietario = buscarNitPropietario.tercero_id;
                    }

                    icb_terceros buscarNitAseguradora =
                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.nitaseguradora);
                    if (buscarNitAseguradora != null)
                    {
                        crearVehiculo.nitaseguradora = buscarNitAseguradora.tercero_id;
                    }

                    crearVehiculo.icbvhfec_creacion = DateTime.Now;
                    crearVehiculo.icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    crearVehiculo.id_bod = Convert.ToInt32(Session["user_bodega"]);
                    context.icb_vehiculo.Add(crearVehiculo);

                    // Agrega los mismos campos en la tabla icb_referencia
                    modelo_vehiculo buscarModelo = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == modelo.modvh_id);

                    icb_referencia refer = new icb_referencia
                    {
                        ref_codigo = modelo.plan_mayor,
                        ref_descripcion = buscarModelo.modvh_nombre != null ? buscarModelo.modvh_nombre : "",
                        ref_fecha_creacion = DateTime.Now,
                        ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        modulo = "V",
                        ref_valor_unitario = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                        ref_valor_total = Convert.ToDecimal(modelo.costototal_vh, miCultura),
                        manejo_inv = true,
                        por_iva_compra = float.Parse(Request["textIVACompra"]),
                        idporivacompra = Convert.ToInt32(Request["valIVACompra"]),
                        impconsumo = float.Parse(Request["text_impoconsumo"]),
                        por_iva = float.Parse(Request["textIVAVenta"]),
                        idporivaventa = Convert.ToInt32(Request["valIVAVenta"])
                    };
                    context.icb_referencia.Add(refer);

                    cambioubicacionveh cambio = new cambioubicacionveh
                    {
                        idvehiculo = modelo.plan_mayor,
                        idubicanterior = null
                    };
                    if (ubicacion == "")
                    {
                        cambio.idubicactual = null;
                    }
                    else
                    {
                        cambio.idubicactual = Convert.ToInt32(ubicacion);
                    }

                    cambio.fechacambio = DateTime.Now;
                    cambio.iduser = Convert.ToInt32(Session["user_usuarioid"]);
                    context.cambioubicacionveh.Add(cambio);

                    bool guardarVehiculo = context.SaveChanges() > 0;
                    if (guardarVehiculo)
                    {
                        TempData["mensaje"] = "El registro del nuevo vehiculo fue exitoso";
                        return RedirectToAction("Crear");
                    }
                }
            }

            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public JsonResult BuscarTerceroPorNit(string nit)
        {
            icb_terceros buscarNit = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == nit);
            if (buscarNit != null)
            {
                return Json(new
                {
                    encontrado = true,
                    buscarNit.razon_social,
                    nombreCompleto = buscarNit.prinom_tercero + " " + buscarNit.segnom_tercero + " " +
                                     buscarNit.apellido_tercero
                                     + " " + buscarNit.segapellido_tercero
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserAsesor(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserAsesorUsados(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserAdministracionVehiculosNuevos(int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado)
                    .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                int rol = Convert.ToInt32(Session["user_rolid"]);
                int user = Convert.ToInt32(Session["user_usuarioid"]);
                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;
                if (rol != admin)
                {
                    //bodega 
                    List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == user).Select(d => d.id_bodega)
                        .ToList();
                    bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                        .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                }

                ViewBag.bodega = new MultiSelectList(bodegas, "id", "nombre");

                var modelo = context.icb_vehiculo.Where(d => d.nuevo == true)
                    .Select(d => new { id = d.icbvh_id, nombre = d.modvh_id }).ToList();
                int rol1 = Convert.ToInt32(Session["user_rolid"]);
                int user1 = Convert.ToInt32(Session["user_usuarioid"]);
                icb_sysparameter admin2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin3 = admin2 != null ? Convert.ToInt32(admin2.syspar_value) : 1;

                if (rol1 != admin3)
                {
                    List<int> model1 = context.icb_vehiculo.Where(d => d.icbvhuserid_creacion == user1)
                        .Select(d => d.icbvh_id).ToList();
                    modelo = context.icb_vehiculo.Where(d => d.nuevo == true && model1.Contains(d.icbvh_id))
                        .Select(d => new { id = d.icbvh_id, nombre = d.modvh_id }).ToList();
                }

                ViewBag.modelo = new MultiSelectList(modelo, "id", "nombre");

                var entregado = context.icb_vehiculo_eventos.Where(d => d.evento_estado)
                    .Select(d => new { id = d.evento_id }).ToList();
                if (rol != admin)
                {
                    List<int> entreg = context.icb_vehiculo_eventos.Where(d => d.eventouserid_creacion == user)
                        .Select(d => d.evento_id).ToList();
                    entregado = context.icb_vehiculo_eventos.Where(d => d.evento_estado && entreg.Contains(d.evento_id))
                        .Select(d => new { id = d.evento_id }).ToList();
                }

                ViewBag.entregado = new MultiSelectList(entregado, "id");

                DateTime fecha = DateTime.Now;
                DateTime fechainicio = fecha.AddDays(-30);
                ViewBag.fechadesde = fechainicio.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                ViewBag.fechahasta = fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"));

                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        public JsonResult buscarPaginacion(string filtroGeneral, int[] bodega)
        {
            if (Session["user_usuarioid"] != null)
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
                //aquí defino las variables que voy a tener en cvuetna para mi consulta en las columnas  de menor a mayor y que no se vuelva a re calcular todo
                // Es importante que la columna en el data table tenga el nombre de la tabla o la vista a consultar, porque es la que se va a utilizar para ordenar
                string sortColumn = Request.Form
                    .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                    .FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                search = search.Replace(" ", " ");
                int pagina = Convert.ToInt32(start);
                int pageSize = Convert.ToInt32(length);

                int skip = 0;
                if (pagina == 0)
                {
                    skip = 0;
                }
                else
                {
                    skip = pagina;
                }

                CultureInfo elGT = CultureInfo.CreateSpecificCulture("el-GR");

                int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
                //buscar el usuario
                users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
                int rol = Convert.ToInt32(Session["user_rolid"]);

                // por si es necesario el rol del usuario 

                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_historial_vehiculos_nuevos, bool>> predicate = PredicateBuilder.True<vw_historial_vehiculos_nuevos>();
                Expression<Func<vw_historial_vehiculos_nuevos, bool>> predicate2 = PredicateBuilder.False<vw_historial_vehiculos_nuevos>();
                Expression<Func<vw_historial_vehiculos_nuevos, bool>> predicate3 = PredicateBuilder.False<vw_historial_vehiculos_nuevos>();
                //busco los permisos del rol
                opcion_acceso opcion_acceso = context.opcion_acceso.Where(d => d.codigo == "OA67").FirstOrDefault();
                int rolpermiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.id_opcion_acceso == opcion_acceso.id_opcion_acceso).Count();
                if (bodega.Count() > 0 && bodega[0] != 0)
                {
                    foreach (int item in bodega)
                    {
                        predicate2 = predicate2.Or(d => d.idBodega == item.ToString());
                    }

                    predicate = predicate.And(predicate2);
                }
                else
                {
                    if (rol != admin)
                    {
                        //Bodegas y usuarios
                        List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == idusuario).Select(d => d.id_bodega)
                            .ToList();
                        var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                            .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                        foreach (var item in bodegas)
                        {
                            predicate2 = predicate2.Or(d => d.idBodega == item.id.ToString());
                        }

                        predicate = predicate.And(predicate2);
                    }
                }

                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    predicate3 = predicate3.Or(d => 1 == 1 && d.plan_mayor.ToString().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.vin.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.FechaCompra.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.FechaEntrega.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.FechaSoat.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.fechaPedido.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.fechaCotizacion.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.nombreCliente.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.nombreProspecto.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.bodccs_nombre.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.fechaActualizacion.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.ubicacion.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.color.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.numerocotizacion.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.anio.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.numerosoat.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.numeropedido.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.modelovehiculo.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.placavehiculo.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.flota2.ToString().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.modvh_nombre.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.segvh_nombre.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.poliza.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.estado_ve.Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.FechaRecepcion.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.ideventoBodega.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.diasInventario2.ToString().Contains(filtroGeneral));
                    predicate3 = predicate3.Or(d => 1 == 1 && d.idBodega.ToString().Contains(filtroGeneral));

                    predicate = predicate.And(predicate3);
                }

                int totalRegistros = context.vw_historial_vehiculos_nuevos.Where(predicate).Count();
                //el ordenamiento será ascendente o descendente
                if (pageSize == -1)
                {
                    pageSize = totalRegistros;
                }

                if (sortColumnDir == "asc")
                {
                    List<vw_historial_vehiculos_nuevos> querySec = context.vw_historial_vehiculos_nuevos.Where(predicate)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = querySec.Select(d => new
                    {
                        d.vin,
                        cantidadAverias = context.icb_inspeccionvehiculos.Where(c => c.planmayor == d.plan_mayor).Count(),
                        d.FechaEntrega,
                        d.ubicacion,
                        d.color,
                        d.anio,
                        cliente = d.numeropedido != null ? d.nombreCliente : d.numerocotizacion != null ? d.nombreProspecto : "",
                        d.fechaPedido,
                        d.fechaCotizacion,
                        d.FechaSoat,
                        d.numerosoat,
                        d.plan_mayor,
                        d.FechaCompra,
                        d.fechaActualizacion,
                        d.bodccs_nombre,
                        d.numerocotizacion,
                        numeropedido = !string.IsNullOrWhiteSpace(d.numeropedido) ? d.numeropedido : "0",
                        d.modelovehiculo,
                        d.placavehiculo,
                        d.flota2,
                        d.modvh_nombre,
                        d.segvh_nombre,
                        d.poliza,
                        d.estado_ve,
                        d.FechaRecepcion,
                        d.ideventoBodega,
                        d.diasInventario2,
                        d.idBodega,
                        asesor = d.user_nombre + " " + d.user_apellido,
                        permiso = rolpermiso > 0 ? true : false,
                    }).ToList();
                    int count = query.Count();
                    return Json(
                        new { draw, recordsFiltered = totalRegistros, recordsTotal = totalRegistros, data = query },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<vw_historial_vehiculos_nuevos> querySec = context.vw_historial_vehiculos_nuevos.Where(predicate)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = querySec.Select(d => new
                    {
                        d.vin,
                        cantidadAverias = context.icb_inspeccionvehiculos.Where(c => c.planmayor == d.plan_mayor).Count(),
                        d.FechaEntrega,
                        d.ubicacion,
                        d.color,
                        d.anio,
                        cliente = d.numeropedido != null ? d.nombreCliente : d.numerocotizacion != null ? d.nombreProspecto : "",
                        d.fechaPedido,
                        d.fechaCotizacion,
                        d.FechaSoat,
                        d.numerosoat,
                        d.plan_mayor,
                        d.FechaCompra,
                        d.fechaActualizacion,
                        d.bodccs_nombre,
                        d.numerocotizacion,
                        numeropedido = !string.IsNullOrWhiteSpace(d.numeropedido) ? d.numeropedido : "0",
                        d.modelovehiculo,
                        d.placavehiculo,
                        d.flota2,
                        d.modvh_nombre,
                        d.segvh_nombre,
                        d.poliza,
                        d.estado_ve,
                        d.FechaRecepcion,
                        d.ideventoBodega,
                        d.diasInventario2,
                        d.idBodega,
                        permiso = rolpermiso > 0 ? true : false,

                    }).ToList();
                    int count = query.Count();
                    return Json(
                        new { draw, recordsFiltered = totalRegistros, recordsTotal = totalRegistros, data = query },
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0);
        }


        //public ActionResult agregarNotas(icb_inspeccionvehiculos model) {

        //    int flag;
        //    string nota = model.nota;
        //    var planmayor = model.planmayor;
        //    try
        //    {


        //        context.Entry(nota).State = EntityState.Modified;
        //       context.SaveChanges();

        //        flag = 1;
        //        return Json(flag, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //        flag = 0;
        //        return Json(flag, JsonRequestBehavior.AllowGet);
        //    }

        //}


        public ActionResult agregarNotas(string planmayor, string notaSeguimiento)
        {
            string mensaje = "";
            int flag;

            icb_inspeccionvehiculos ve = new icb_inspeccionvehiculos
            {
                nota = notaSeguimiento
            };

            try
            {
                context.Entry(ve).State = EntityState.Modified;
                context.SaveChanges();
                flag = 1;
            }
            catch (DbEntityValidationException e)
            {
                flag = 0;
                mensaje = e.Message;
            }

            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        public JsonResult hojavidaVH(string planmayor)
        {
            var datos = (from vh in context.icb_vehiculo
                         join ped in context.vpedido
                         on vh.plan_mayor equals ped.planmayor into pedido
                         from ped in pedido.DefaultIfEmpty()
                         join cl in context.icb_terceros
                         on ped.nit equals cl.tercero_id into cliente
                         from cl in cliente.DefaultIfEmpty()
                         join dir in context.terceros_direcciones
                         on cl.tercero_id equals dir.idtercero into direccion
                         from dir in direccion.DefaultIfEmpty()
                         where vh.plan_mayor == planmayor
                         select new
                         {
                             documento = cl.doc_tercero,
                             nombre = cl.prinom_tercero + " " + cl.segnom_tercero + " " + cl.apellido_tercero + " " + cl.segapellido_tercero,
                             dir.direccion,
                             telefono = cl.telf_tercero,
                             celular = cl.celular_tercero,
                             vh.plan_mayor,
                             vh.vin,
                             placa = vh.plac_vh,
                             modelo = vh.modelo_vehiculo.modvh_nombre,
                             color = vh.colvh_id != null ? vh.color_vehiculo.colvh_nombre : "",
                             km = vh.kilometraje,
                             anio = vh.anio_vh,
                             tipo = vh.tpvh_id != null ? vh.tipo_vehiculo.tpvh_nombre : "",
                             tiposervicio = vh.tiposervicio != null ? vh.tpservicio_vehiculo.tpserv_nombre : "",
                             tipocompra = vh.codigo_pago != null && vh.codigo_pago != "" ? context.vconceptocompra.Where(x => x.codigo == vh.codigo_pago).FirstOrDefault().descripcion : "",
                             nitprenda = vh.nitprenda != null ? vh.icb_unidad_financiera.financiera_nombre : "",
                             flota = vh.flota ? "Si" : "No",
                             codflota = ped.codigoflota,
                             nuevo = vh.nuevo == true ? "Nuevo" : "Usado"
                         }).FirstOrDefault();

            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eventsAdministracion(string planmayor)
        {
            if (planmayor != null || planmayor != "")
            {
                var eventos = (from ve in context.icb_vehiculo_eventos
                               join te in context.icb_tpeventos
                                   on ve.id_tpevento equals te.tpevento_id
                               join b in context.bodega_concesionario
                                   on ve.bodega_id equals b.id
                               join u in context.users
                                   on ve.eventouserid_creacion equals u.user_id
                               join df in context.documento_facturacion
                                   on te.iddocasociado equals df.docfac_id into temp
                               from df in temp.DefaultIfEmpty()
                               where ve.planmayor == planmayor
                               select new
                               {
                                   te.tpevento_id,
                                   te.codigoevento,
                                   te.tpevento_nombre,
                                   b.bodccs_nombre,
                                   ve.fechaevento,
                                   u.user_nombre,
                                   u.user_apellido,
                                   te.iddocasociado,
                                   cargado =
                                       context.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                           x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado) != null
                                           ? context.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                               x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado).id
                                           : 0
                               }).ToList();

                List<int> listaEventos = new List<int>();
                foreach (var item in eventos)
                {
                    listaEventos.Add(item.tpevento_id);
                }

                var buscarFaltantes = (from a in context.icb_tpeventos
                                       where !listaEventos.Contains(a.tpevento_id) && a.tpevento_estado
                                       select new
                                       {
                                           a.tpevento_id,
                                           a.tpevento_nombre
                                       }).ToList();


                var data2 = eventos.Select(x => new
                {
                    codigo = x.codigoevento,
                    nombre = x.tpevento_nombre,
                    bodega = x.bodccs_nombre,
                    fecha = x.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("is-IS")),
                    usuario = x.user_nombre + ' ' + x.user_apellido,
                    documento = x.iddocasociado != null ? x.iddocasociado : 0,
                    x.cargado
                }).ToList();

                return Json(new { info = true, data2, buscarFaltantes }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarModelosPorMarca(int idMarca)
        {
            var buscarModelos = context.modelo_vehiculo.Where(x => x.mar_vh_id == idMarca).Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            }).ToList();
            ViewBag.buscarModelo = new SelectList(buscarModelos, "modvh_codigo", "modvh_nombre");
            return Json(buscarModelos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAniosModeloPorId(string modeloCodigo)
        {
            var buscarAnios = context.anio_modelo.Where(x => x.codigo_modelo == modeloCodigo).Select(x => new
            {
                x.anio
            }).ToList();
            return Json(buscarAnios, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsonBrowserAsesor(string idModelo)
        {
            int roluser = Convert.ToInt32(Session["user_rolid"]);
            //var rolnombre = context.rols.Where(x => x.rol_id == roluser);
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            string sDate = DateTime.Now.ToString();
            DateTime datevalue = Convert.ToDateTime(sDate);
            int mn = datevalue.Month;
            int yy = datevalue.Year;
            //se crea un distinc para actualizar los vehiculos que no se encuentran disponibles. 23/09/2019 Leo OK
            var modelos = context.vpedido.Where(d => d.planmayor != null && d.planmayor != string.Empty && d.anulado==false).ToList();
            List<string> datdistinctct = modelos.GroupBy(d => d.planmayor)
                .Where(p => p.Key != null && p.Key != string.Empty).Select(d => d.Key).ToList();
            List<vw_referencias_total> data2 = context.vw_referencias_total.Where(v =>
                v.modvh_id == idModelo && v.ano == yy && v.mes == mn /*&& v.bodega == bodegaActual*/ && v.nuevo == true &&
                v.stock > 0 && !datdistinctct.Contains(v.codigo)).OrderByDescending(d=>d.ano).ThenByDescending(d=>d.mes).ToList();
            var data9 = data2.GroupBy(v => v.codigo).Select(v => new
            {
                planmayor = v.Key,
                serie = v.Select(x => x.vin).FirstOrDefault(),
                descripcion = v.Select(x => x.modvh_nombre).FirstOrDefault(),
                anomodelo = v.Select(x => x.anio_vh).FirstOrDefault(),
                color = v.Select(x => x.colvh_nombre).FirstOrDefault(),
                fec_compra = v.Select(x => x.fecfact_fabrica.ToString()).FirstOrDefault(),
                fecfact_fabrica= v.Select(x => x.fecfact_fabrica).FirstOrDefault(),
                asignado= v.Select(x => x.asignado).FirstOrDefault(),
                bodccs_nombre= v.Select(x => x.bodccs_nombre).FirstOrDefault(),
                Estado = v.Select(x => x.Estado).FirstOrDefault(),
                ubicacion = v.Select(x => x.descripcion).FirstOrDefault(),
                fechaevento=v.Select(x => x.fechaevento).FirstOrDefault(),
                vin= v.Select(y => y.vin).FirstOrDefault(),
                stock= v.Select(y => y.stock).FirstOrDefault(),
                bodega= v.Select(y => y.bodega).FirstOrDefault(),
            }).ToList();
               //var data2 = context.vw_referenciasVehiTotal2.Where(v => v.modvh_nombre == idModelo && v.ano == yy && v.mes == mn && v.bodega == bodegaActual && v.nuevo == true && v.stock > 0 /* && v.evento_id == */ ).ToList();
               var data = data9.Select(v => new
            {
                v.planmayor,
                 v.serie,
                v.descripcion,
                v.anomodelo,
                v.color,
                v.fec_compra,
                dias_inventario =
                    (DateTime.Now - (v.fecfact_fabrica != null ? v.fecfact_fabrica.Value : DateTime.Now)).TotalDays
                    .ToString("N0", new CultureInfo("is-IS")),
                asignado = v.asignado != null ? v.asignado.Value.ToString() : "",
                v.stock,
                bodega = !string.IsNullOrWhiteSpace(v.bodccs_nombre) ? v.bodccs_nombre: "",
                Estado=v.Estado,
                ubicacion = !string.IsNullOrWhiteSpace(v.descripcion) ? v.descripcion : "",
                fechaEvento = v.fechaevento != null
                    ? v.fechaevento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "Proceso de llegada a bodega",
                cantidadAverias = context.icb_inspeccionvehiculos
                    .Where(x => x.icb_vehiculo.vin == v.vin).Count(),
                idBodega = v.bodega
            }).ToList();

            var data3 = (from data4 in data
                         join ev in context.icb_vehiculo_eventos
                             on data4.planmayor equals ev.planmayor
                         where ev.evento_estado && ev.evento_nombre == "Envio a Exibicion"
                         select new
                         {
                             ev.evento_id,
                             ev.planmayor,
                             ev.evento_nombre,
                             ev.evento_estado
                         }).ToList();
            var eventos = (from data8 in data3
                           group data8 by data8.planmayor).ToList();
            return Json(new { data, eventos, roluser }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAverias(string plan_mayor)
        {
            var buscar = context.icb_vehiculo_eventos.Where(x => x.planmayor == plan_mayor && x.id_tpevento == 15)
                .Select(x => new
                {
                    x.planmayor,
                    x.fechaevento,
                    x.evento_observacion
                }).OrderBy(x => x.fechaevento).ToList();

            var data = buscar.Select(x => new
            {
                x.planmayor,
                fecha = x.fechaevento.ToString("yyyy/MM/dd HH:mm"),
                observacion = x.evento_observacion
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAveriasInsp2(string planmayor)
        {
            var buscar = context.icb_inspeccionvehiculos.Where(x => x.planmayor == planmayor)
                .Select(x => new
                {
                    x.insp_id,
                    x.planmayor,
                    fechaevento = x.insp_fechains,
                    evento_observacion = x.insp_observacion,
                    estadoA = x.estado_averia_id != null ? x.estadoAverias.nombre : "",
                    taller = x.taller_averia_id != null ? x.tallerAveria.nombre : "",
                    solicitar = x.insp_solicitar
                }).OrderBy(x => x.fechaevento).ToList();

            var data = buscar.Select(x => new
            {
                x.insp_id,
                x.planmayor,
                fecha = x.fechaevento.Value.ToString("yyyy/MM/dd HH:mm"),
                observacion = x.evento_observacion,
                x.estadoA,
                x.taller,
                x.solicitar
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //---------
        public JsonResult buscarAveriasInsp(string planmayor,string insp_vin)
        {
            if(!string.IsNullOrWhiteSpace(planmayor)|| !string.IsNullOrWhiteSpace(insp_vin))
            {
                var predicado = PredicateBuilder.True<icb_inspeccionvehiculos>();
                if (!string.IsNullOrWhiteSpace(planmayor))
                {
                    predicado = predicado.And(d => d.planmayor == planmayor);
                }
                if (!string.IsNullOrWhiteSpace(insp_vin))
                {
                    predicado = predicado.And(d => d.insp_vin == insp_vin);
                }

                var buscar = context.icb_inspeccionvehiculos.Where(predicado)
                    .Select(x => new
                    {
                        x.insp_id,
                        x.planmayor,
                        fechaevento = x.insp_fechains,
                        evento_observacion = x.insp_observacion,
                        estadoA = x.estado_averia_id != null ? x.estadoAverias.nombre : "",
                        taller = x.taller_averia_id != null ? x.tallerAveria.nombre : "",
                        solicitar = x.insp_solicitar
                    }).OrderBy(x => x.fechaevento).ToList();

                var data = buscar.Select(x => new
                {
                    x.insp_id,
                    x.planmayor,
                    fecha = x.fechaevento.Value.ToString("yyyy/MM/dd HH:mm"),
                    observacion = x.evento_observacion,
                    x.estadoA,
                    x.taller,
                    x.solicitar
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);

            }

        }


        public JsonResult buscarAveriasBackOffice(string planmayor)
        {
            long estadoG = context.estadoAverias.Where(x => x.nombre.Contains("solucion")).FirstOrDefault().id;

            //Buscar Sin Gestionar.
            var buscarSG = context.icb_inspeccionvehiculos.Where(x => x.planmayor == planmayor && x.estado_averia_id != estadoG)
               .Select(x => new
               {
                   x.insp_id,
                   x.planmayor,
                   fechaevento = x.insp_fechains,
                   evento_observacion = x.insp_observacion,
                   estadoA = x.estado_averia_id != null ? x.estadoAverias.nombre : "",
                   taller = x.taller_averia_id != null ? x.tallerAveria.nombre : "",
                   solicitar = x.insp_solicitar
               }).OrderBy(x => x.fechaevento).ToList();

            var dataSG = buscarSG.Select(x => new
            {
                x.insp_id,
                x.planmayor,
                fecha = x.fechaevento.Value.ToString("yyyy/MM/dd HH:mm"),
                observacion = x.evento_observacion,
                x.estadoA,
                x.taller,
                x.solicitar
            });

            //BuscarGestionadas.
            var buscarG = context.icb_inspeccionvehiculos.Where(x => x.planmayor == planmayor && x.estado_averia_id == estadoG)
               .Select(x => new
               {
                   x.insp_id,
                   x.planmayor,
                   fechaevento = x.insp_fechains,
                   evento_observacion = x.insp_observacion,
                   estadoA = x.estado_averia_id != null ? x.estadoAverias.nombre : "",
                   taller = x.taller_averia_id != null ? x.tallerAveria.nombre : "",
                   solicitar = x.insp_solicitar,
                   modelo = x.planmayor != null ? x.icb_vehiculo.modvh_id != null ? x.icb_vehiculo.modelo_vehiculo.modvh_nombre : "" : "",
                   vin = x.insp_vin,
                   placa = x.planmayor != null ? x.icb_vehiculo.plac_vh != null ? x.icb_vehiculo.plac_vh : "" : "",
                   color = x.planmayor != null ? x.icb_vehiculo.colvh_id != null ? x.icb_vehiculo.color_vehiculo.colvh_nombre : "" : "",
                   pedido = x.planmayor != null ? context.vpedido.Where(f => f.planmayor == x.planmayor).FirstOrDefault() != null ? context.vpedido.Where(f => f.planmayor == x.planmayor).FirstOrDefault().numero.ToString() : "" : "",
                   encargado = x.encargado != null ? x.users.user_nombre + " " + x.users.user_apellido : ""
               }).OrderBy(x => x.fechaevento).ToList();

            var dataG = buscarG.Select(x => new
            {
                x.insp_id,
                x.planmayor,
                fecha = x.fechaevento.Value.ToString("yyyy/MM/dd HH:mm"),
                observacion = x.evento_observacion,
                x.estadoA,
                x.taller,
                x.solicitar,
                x.modelo,
                x.vin,
                x.placa,
                x.color,
                x.pedido,
                x.encargado
            });

            return Json(new { singestionar = dataSG, gestionados = dataG }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Actualizar(string id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vehiculo = (from a in context.icb_vehiculo
                            join b in context.icb_referencia
                                on a.plan_mayor equals b.ref_codigo into aa
                            from b in aa.DefaultIfEmpty()
                            join c in context.cambioubicacionveh
                                on a.plan_mayor equals c.idvehiculo into xx
                            from c in xx.DefaultIfEmpty()
                            join d in context.ubicacion_vehiculo
                                on c.idubicactual equals d.ubivh_id into zz
                            from d in zz.DefaultIfEmpty()
                            join e in context.modelo_vehiculo
                            on a.modvh_id equals e.modvh_codigo into ee
                            from e in ee.DefaultIfEmpty()
                            join f in context.concesionario
                            on e.concesionarioid equals f.id into ff
                            from f in ff.DefaultIfEmpty()

                            where a.plan_mayor == id
                            select new
                                {
                                impconsumo = a.impconsumo != null ? a.impconsumo : 0,
                                a.costosiniva_vh,
                                a.iva_vh,
                                a.costototal_vh,
                                a.icbvh_id,
                                a.plan_mayor,
                                a.vin,
                                a.plac_vh,
                                a.nummot_vh,
                                a.marcvh_id,
                                a.modvh_id,
                                a.anio_vh,
                                a.colvh_id,
                                a.tpvh_id,
                                a.Notas,
                                a.flota,
                                a.nuevo,
                                a.usado,
                                a.proveedor_id,
                                a.fecfact_fabrica,
                                a.fecentman_vh,
                                a.ciumanf_vh,
                                a.nummanf_vh,
                                a.diaslibres_vh,
                                a.icbvhuserid_creacion,
                                a.icbvhid_licencia,
                                a.icbvhuserid_actualizacion,
                                a.icbvhfec_actualizacion,
                                a.icbvhfec_creacion,
                                a.kilometraje,
                                a.Numerogarantia,
                                a.fecha_garantia,
                                a.tiempogarantia,
                                a.kmgarantia,
                                a.numerosoat,
                                a.fecha_soat,
                                a.fecha_venta,
                                a.fecha_tecnomecanica,
                                a.propietario,
                                a.nitaseguradora,
                                a.codigo_pago,
                                a.fecmatricula,
                                a.ubicacionactual,
                                b.idporivacompra,
                                b.idporivaventa,
                                c.idubicactual,
                                c.idubicanterior,
                                d.ubivh_nombre,
                                a.ciudadplaca,
                                a.nitprenda,
                                a.idasesor,
                                impo = b.impconsumo,
                                nomconcesio = f.nombre
                                }).FirstOrDefault();

            //var ubicaciones = (from a in context.cambioubicacionveh
            //                   join b in context.ubicacion_vehiculo
            //                   on a.idubicactual equals b.ubivh_id                               
            //                   where a.idvehiculo == id
            //                   select new {
            //                       b.ubivh_nombre
            //                   }).FirstOrDefault();

            cambioubicacionveh ubicaciones = context.cambioubicacionveh.Where(a => a.idvehiculo == id).FirstOrDefault();
            var ultimaUbicacion = (from a in context.cambioubicacionveh
                                   join b in context.ubicacion_bodega
                                       on a.idubicanterior equals b.id into zz
                                   from b in zz.DefaultIfEmpty()
                                   select new
                                   {
                                       a.id,
                                       a.idubicactual,
                                       a.idubicanterior,
                                       b.descripcion
                                   }).FirstOrDefault();
            if (vehiculo == null)
            {
                return HttpNotFound();
            }

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            CrearVehiculoModel modelo = new CrearVehiculoModel();
            string montoiva = "";
            decimal montoimp = 0;
            if (vehiculo.impconsumo == null)
            {
                anio_modelo buscarCostos = context.anio_modelo.FirstOrDefault(x =>
                    x.codigo_modelo == vehiculo.modvh_id && x.anio == vehiculo.anio_vh);
                if (buscarCostos != null)
                {
                    modelo.costosiniva_vh = vehiculo.costosiniva_vh.ToString("N0", new CultureInfo("is-IS"));
                    montoiva = (buscarCostos.valor * (buscarCostos.porcentaje_iva / 100)).ToString("N0",
                        new CultureInfo("is-IS"));
                    modelo.costototal_vh =
                        (vehiculo.costosiniva_vh + vehiculo.iva_vh * vehiculo.costosiniva_vh / 100 +
                         vehiculo.costosiniva_vh * (vehiculo.impconsumo / 100)).Value
                        .ToString("N0", new CultureInfo("is-IS"));
                    montoimp = buscarCostos.valor * (buscarCostos.impuesto_consumo / 100);


                    //modelo.impconsumo = buscarCostos.idporcentajeimpoconsumo.ToString("N0", new CultureInfo("is-IS"));
                    modelo.impconsumo = "0";
                    modelo.iva_vh = buscarCostos.idporcentajeiva.ToString("N0", new CultureInfo("is-IS"));
                    modelo.iva_compra_vh = buscarCostos.idporcentajecompra.ToString("N0", new CultureInfo("is-IS"));
                }
            }
            else
            {
                modelo.costosiniva_vh = vehiculo.costosiniva_vh.ToString("N0", new CultureInfo("is-IS"));
                montoiva = (vehiculo.costosiniva_vh * (vehiculo.iva_vh / 100)).ToString("N0", new CultureInfo("is-IS"));
                modelo.costototal_vh = vehiculo.costototal_vh.ToString("N0", new CultureInfo("is-IS"));
                montoimp = Convert.ToDecimal((vehiculo.costosiniva_vh * (vehiculo.impconsumo / 100)), miCultura);

                modelo.iva_vh = vehiculo.iva_vh.ToString("N0", new CultureInfo("is-IS"));
                modelo.impconsumo = Convert.ToString(vehiculo.impconsumo);
                //modelo.iva_compra_vh = vehiculo.idporcentajecompra.ToString("N0", new CultureInfo("is-IS"));
            }

            modelo.codigo_pago = vehiculo.codigo_pago;
            modelo.icbvh_id = vehiculo.icbvh_id;
            modelo.plan_mayor = vehiculo.plan_mayor;
            modelo.vin = vehiculo.vin;
            modelo.plac_vh = vehiculo.plac_vh;
            modelo.nummot_vh = vehiculo.nummot_vh;
            modelo.marcvh_id = vehiculo.marcvh_id;
            modelo.modvh_id = vehiculo.modvh_id;
            modelo.anio_vh = vehiculo.anio_vh;
            modelo.colvh_id = vehiculo.colvh_id;
            modelo.tpvh_id = vehiculo.tpvh_id;
            modelo.Notas = vehiculo.Notas;
            modelo.flota = vehiculo.flota;
            modelo.nuevo = vehiculo.nuevo ?? false;
            modelo.usado = vehiculo.usado ?? false;
            modelo.proveedor_id = vehiculo.proveedor_id;
            modelo.fecfact_fabrica = vehiculo.fecfact_fabrica!=null?vehiculo.fecfact_fabrica.Value.ToString("yyyy/MM/dd",miCulturaTime):"";
            modelo.fecentman_vh = vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.ToString("yyyy/MM/dd", miCulturaTime):"";
            modelo.ciumanf_vh = vehiculo.ciumanf_vh;
            modelo.nummanf_vh = vehiculo.nummanf_vh;
            modelo.diaslibres_vh = vehiculo.diaslibres_vh;
            modelo.icbvhuserid_creacion = vehiculo.icbvhuserid_creacion;
            modelo.icbvhid_licencia = vehiculo.icbvhid_licencia;
            modelo.icbvhuserid_actualizacion = vehiculo.icbvhuserid_actualizacion;
            modelo.icbvhfec_actualizacion = vehiculo.icbvhfec_actualizacion;
            modelo.icbvhfec_creacion = vehiculo.icbvhfec_creacion;
            modelo.kilometraje = vehiculo.kilometraje.ToString("0,0", elGR);
            modelo.Numerogarantia = vehiculo.Numerogarantia;
            modelo.fecha_garantia = vehiculo.fecha_garantia != null ? vehiculo.fecha_garantia.Value.ToString("yyyy/MM/dd", miCulturaTime) : "";
            modelo.tiempogarantia = vehiculo.tiempogarantia;
            modelo.kmgarantia = vehiculo.kmgarantia.ToString("0,0", elGR);
            modelo.numerosoat = vehiculo.numerosoat;
            modelo.fecha_soat = vehiculo.fecha_soat != null ? vehiculo.fecha_soat.Value.ToString("yyyy/MM/dd", miCulturaTime) : "";
           
            modelo.fecha_tecnomecanica = vehiculo.fecha_tecnomecanica != null ? vehiculo.fecha_tecnomecanica.Value.ToString("yyyy/MM/dd", miCulturaTime) : "";
            modelo.nombreUbicacion = vehiculo.ubivh_nombre;
            modelo.ubicacion = vehiculo.ubicacionactual;
            modelo.fecha_matricula = vehiculo.fecmatricula != null ? vehiculo.fecmatricula.Value.ToString("yyyy/MM/dd", miCulturaTime) : "";
            modelo.ciudadplaca = vehiculo.ciudadplaca;
            modelo.idasesor = vehiculo.idasesor;
            modelo.nitprenda = vehiculo.nitprenda;
            modelo.propietario = Convert.ToString(vehiculo.propietario);

            if (vehiculo.plan_mayor!=null)
                {
                vpedido pedido = context.vpedido.Where(x => x.planmayor == vehiculo.plan_mayor).FirstOrDefault();


                if (pedido!=null)
                    {
                    if (pedido.pazysalvo == true && pedido.fecpazysalvo != null)
                        {
                        modelo.fecha_venta = vehiculo.fecha_venta != null ? vehiculo.fecha_venta.Value.ToString("yyyy/MM/dd", miCulturaTime) : "";
                        modelo.nombreconcesionario = vehiculo.nomconcesio != null ? vehiculo.nomconcesio : "";
                        }
                    }
           

                }

            icb_terceros buscarNitPropietario = context.icb_terceros.FirstOrDefault(x => x.tercero_id == vehiculo.propietario);
            if (buscarNitPropietario != null)
            {
                modelo.propietario = Convert.ToString(vehiculo.propietario);
            }

            icb_terceros buscarNitAseguradora =
                context.icb_terceros.FirstOrDefault(x => x.tercero_id == vehiculo.nitaseguradora);
            if (buscarNitAseguradora != null)
            {
                modelo.nitaseguradora = buscarNitAseguradora.doc_tercero;
            }

            BuscarDatosCreacion(modelo);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 202);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            int bod = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.nombreEnlaces = enlaces;
            ViewBag.anioSeleccionado = vehiculo.anio_vh;
            ViewBag.proveedorElegido = vehiculo.proveedor_id;
            ViewBag.fechaManifiesto =
                vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.ToShortDateString() : "";
            ViewBag.fechaCompra = vehiculo.fecfact_fabrica != null
                ? vehiculo.fecfact_fabrica.Value.ToShortDateString()
                : "";
            ViewBag.concepto = new SelectList(context.vconceptocompra, "codigo", "descripcion");
            ViewBag.nombreUbicacion =
                new SelectList(
                    context.ubicacion_bodega.Where(x => x.estado && x.idbodega == bod && x.tipo == 2)
                        .OrderBy(x => x.descripcion), "id", "descripcion", modelo.ubicacion);
            string UltimaUbicacion = (from a in context.cambioubicacionveh
                                      join b in context.ubicacion_bodega
                                          on a.idubicanterior equals b.id
                                      where a.idvehiculo == modelo.plan_mayor
                                      select b.descripcion).FirstOrDefault();
            modelo.nombreUltimaUbicacion = UltimaUbicacion != null ? UltimaUbicacion : "No ha cambiado la ubicación";
            ViewBag.montoiva = montoiva;
            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Actualizar(CrearVehiculoModel modelo, int? menu)
        {
            string idUbiacion = Request["ubicacionVal"];
            //if (ModelState.IsValid)
            //{
            if (modelo.plan_mayor == modelo.vin)
            {
                TempData["mensaje_error"] = "El numero de serie no puede ser igual al plan mayor";
                int bode = Convert.ToInt32(Session["user_bodega"]);
                ViewBag.anioSeleccionado = modelo.anio_vh;
                ListasDesplegables(modelo);
                ViewBag.proveedorElegido = modelo.proveedor_id;
                ViewBag.iva_vh = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id",
                    "Porcentaje");
                ViewBag.iva_compra_vh = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "COMPRA"), "id",
                    "Porcentaje");
                ViewBag.impconsumo = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "IMPOCONSUMO"), "id",
                    "Porcentaje");
                ViewBag.concepto = new SelectList(context.vconceptocompra, "codigo", "descripcion");
                ViewBag.nombreUbicacion =
                    new SelectList(
                        context.ubicacion_bodega.Where(x => x.estado && x.idbodega == bode && x.tipo == 2)
                            .OrderBy(x => x.descripcion), "id", "descripcion", modelo.ubicacion);
                ViewBag.ciudadplaca =
                    new SelectList(context.nom_ciudad.Where(x => x.ciu_estado).OrderBy(x => x.ciu_nombre), "ciu_id",
                        "ciu_nombre", modelo.ciudadplaca);
                ViewBag.nitprenda =
                    new SelectList(
                        context.icb_unidad_financiera.Where(x => x.financiera_estado).OrderBy(x => x.financiera_nombre),
                        "financiera_id", "financiera_nombre", modelo.nitprenda);
                var asesores = from x in context.users
                               where x.user_estado && (x.rol_id == 4 || x.rol_id == 2030)
                               select new
                               {
                                   x.user_id,
                                   nombre = x.user_nombre + " " + x.user_apellido
                               };

                ViewBag.idasesor =
                    new SelectList(asesores.OrderBy(x => x.nombre), "user_id", "nombre", modelo.idasesor);
                BuscarDatosCreacion(modelo);
                BuscarFavoritos(menu);
                return View(modelo);
            }

            {
                int buscarPorPlanMayor = (from a in context.icb_vehiculo
                                          where a.plan_mayor == modelo.plan_mayor && a.icbvh_id == modelo.icbvh_id
                                          select a.plan_mayor).Count();

                if (buscarPorPlanMayor > 0)
                {
                    int? ubicacion;
                    if (modelo.nombreUbicacion == "" || modelo.nombreUbicacion == null)
                    {
                        ubicacion = null;
                    }
                    else
                    {
                        ubicacion = Convert.ToInt32(modelo.nombreUbicacion);
                    }

                    icb_vehiculo crearVehiculo = new icb_vehiculo
                    {
                        codigo_pago = Request["valConcepto"],
                        icbvh_id = modelo.icbvh_id,
                        id_bod = Convert.ToInt32(Session["user_bodega"]),
                        plan_mayor = modelo.plan_mayor,
                        vin = modelo.vin,
                        plac_vh = modelo.plac_vh,
                        nummot_vh = modelo.nummot_vh,
                        marcvh_id = modelo.marcvh_id,
                        modvh_id = modelo.modvh_id,
                        anio_vh = modelo.anio_vh,
                        costosiniva_vh = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                        iva_vh = Convert.ToDecimal(modelo.iva_vh, miCultura),
                        impconsumo = Convert.ToDecimal(modelo.impconsumo, miCultura),
                        costototal_vh = Convert.ToDecimal(modelo.costototal_vh, miCultura),
                        colvh_id = modelo.colvh_id,
                        tpvh_id = modelo.tpvh_id,
                        Notas = modelo.Notas,
                        flota = modelo.flota,
                        proveedor_id = modelo.proveedor_id,
                        ciumanf_vh = modelo.ciumanf_vh,
                        nummanf_vh = modelo.nummanf_vh,
                        nuevo = modelo.nuevo,
                        usado = modelo.usado,
                        kilometraje = (long)Convert.ToDecimal(modelo.kilometraje, miCultura),
                        Numerogarantia = modelo.Numerogarantia,
                        tiempogarantia = modelo.tiempogarantia,
                        kmgarantia = (long)Convert.ToDecimal(modelo.kmgarantia, miCultura),
                        numerosoat = modelo.numerosoat,
                        ubicacionactual = ubicacion,
                        icbvhuserid_creacion = modelo.icbvhuserid_creacion,
                        icbvhid_licencia = modelo.icbvhid_licencia,
                        icbvhfec_creacion = modelo.icbvhfec_creacion,
                        ciudadplaca = modelo.ciudadplaca,
                        nitprenda = modelo.nitprenda,
                        idasesor = modelo.idasesor,
                    };
                    if (!string.IsNullOrWhiteSpace(modelo.fecfact_fabrica))
                    {
                        crearVehiculo.fecfact_fabrica = DateTime.Parse(modelo.fecfact_fabrica, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecentman_vh))
                    {
                        crearVehiculo.fecentman_vh = DateTime.Parse(modelo.fecentman_vh, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_garantia))
                    {
                        crearVehiculo.fecha_garantia = DateTime.Parse(modelo.fecha_garantia, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_venta))
                    {
                        crearVehiculo.fecha_venta = DateTime.Parse(modelo.fecha_venta, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_tecnomecanica))
                    {
                        crearVehiculo.fecha_tecnomecanica = DateTime.Parse(modelo.fecha_tecnomecanica, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_matricula))
                    {
                        crearVehiculo.fecmatricula = DateTime.Parse(modelo.fecha_matricula, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(modelo.fecha_soat))
                    {
                        crearVehiculo.fecha_soat = DateTime.Parse(modelo.fecha_soat, miCulturaTime);
                    }
                    int propietarioVH = Convert.ToInt32(Request["propietario"]);

                    icb_terceros buscarNitPropietario = context.icb_terceros.FirstOrDefault(x => x.tercero_id == propietarioVH);
                    if (buscarNitPropietario != null)
                    {
                        crearVehiculo.propietario = buscarNitPropietario.tercero_id;
                    }

                    icb_terceros buscarNitAseguradora =
                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.nitaseguradora);
                    if (buscarNitAseguradora != null)
                    {
                        crearVehiculo.nitaseguradora = buscarNitAseguradora.tercero_id;
                    }

                    crearVehiculo.icbvhfec_actualizacion = DateTime.Now;
                    crearVehiculo.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(crearVehiculo).State = EntityState.Modified;
                    context.SaveChanges();


                    icb_referencia refer = context.icb_referencia.Where(x => x.ref_codigo == modelo.plan_mayor).FirstOrDefault();

                    refer.ref_codigo = modelo.plan_mayor;
                    refer.ref_fecha_actualizacion = DateTime.Now;
                    refer.ref_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    refer.ref_valor_unitario = Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                    refer.ref_valor_total = Convert.ToDecimal(modelo.costototal_vh, miCultura);
                    refer.manejo_inv = true;
                    refer.por_iva_compra = Request["textIVACompra"] != null ? float.Parse(Request["textIVACompra"]) : 0;
                    refer.idporivacompra =
                        Request["valIVACompra"] != null ? Convert.ToInt32(Request["valIVACompra"]) : 1;
                    refer.impconsumo = Request["text_impoconsumo"] != null
                        ? float.Parse(Request["text_impoconsumo"])
                        : 0;
                    refer.por_iva = Request["textIVAVenta"] != null ? float.Parse(Request["textIVAVenta"]) : 0;
                    refer.idporivaventa = Request["valIVAVenta"] != null ? Convert.ToInt32(Request["valIVAVenta"]) : 4;
                    context.Entry(refer).State = EntityState.Modified;
                    context.SaveChanges();
                    if (modelo.nombreUbicacion != null)
                    {
                        cambioubicacionveh cambio = context.cambioubicacionveh.Where(x => x.idvehiculo == modelo.plan_mayor)
                            .FirstOrDefault();
                        if (cambio == null)
                        {
                            cambioubicacionveh cam = new cambioubicacionveh
                            {
                                idvehiculo = modelo.plan_mayor,
                                idubicactual = Convert.ToInt32(modelo.nombreUbicacion),
                                fechacambio = DateTime.Now,
                                iduser = Convert.ToInt32(Session["user_usuarioid"])
                            };
                            context.cambioubicacionveh.Add(cam);
                        }
                        else
                        {
                            cambio.idvehiculo = modelo.plan_mayor;
                            cambio.idubicanterior = cambio.idubicactual;
                            cambio.idubicactual = Convert.ToInt32(modelo.nombreUbicacion);
                            cambio.fechacambio = DateTime.Now;
                            cambio.iduser = Convert.ToInt32(Session["user_usuarioid"]);
                            context.Entry(cambio).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                    }

                    TempData["mensaje"] = "La actualización del vehiculo fue exitoso!";
                }
                else
                {
                    int nom2 = (from a in context.icb_vehiculo
                                where a.plan_mayor == modelo.plan_mayor
                                select a.plan_mayor).Count();
                    if (nom2 == 0)
                    {
                        icb_vehiculo crearVehiculo = new icb_vehiculo
                            {
                            codigo_pago = Request["valConcepto"],
                            icbvh_id = modelo.icbvh_id,
                            plan_mayor = modelo.plan_mayor,
                            vin = modelo.vin,
                            plac_vh = modelo.plac_vh,
                            nummot_vh = modelo.nummot_vh,
                            marcvh_id = modelo.marcvh_id,
                            modvh_id = modelo.modvh_id,
                            anio_vh = modelo.anio_vh,
                            costosiniva_vh = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                            iva_vh = Convert.ToDecimal(modelo.iva_vh, miCultura),
                            impconsumo = Convert.ToDecimal(modelo.impconsumo, miCultura),
                            costototal_vh = Convert.ToDecimal(modelo.costototal_vh, miCultura),
                            colvh_id = modelo.colvh_id,
                            tpvh_id = modelo.tpvh_id,
                            Notas = modelo.Notas,
                            flota = modelo.flota,
                            proveedor_id = modelo.proveedor_id,
                            ciumanf_vh = modelo.ciumanf_vh,
                            nummanf_vh = modelo.nummanf_vh,
                            diaslibres_vh = modelo.diaslibres_vh,
                            icbvhuserid_creacion = modelo.icbvhuserid_creacion,
                            icbvhid_licencia = modelo.icbvhid_licencia,
                            icbvhuserid_actualizacion = modelo.icbvhuserid_actualizacion,
                            icbvhfec_actualizacion = modelo.icbvhfec_actualizacion,
                            icbvhfec_creacion = modelo.icbvhfec_creacion,
                            ubicacionactual = Convert.ToInt32(modelo.nombreUbicacion),
                        
                        };
                        if (!string.IsNullOrWhiteSpace(modelo.fecfact_fabrica))
                        {
                            crearVehiculo.fecfact_fabrica = DateTime.Parse(modelo.fecfact_fabrica, miCulturaTime);
                        }
                        if (!string.IsNullOrWhiteSpace(modelo.fecentman_vh))
                        {
                            crearVehiculo.fecentman_vh = DateTime.Parse(modelo.fecentman_vh, miCulturaTime);
                        }
                        if (!string.IsNullOrWhiteSpace(modelo.fecha_matricula))
                        {
                            crearVehiculo.fecmatricula = DateTime.Parse(modelo.fecha_matricula, miCulturaTime);
                        }
                        if (!string.IsNullOrWhiteSpace(modelo.fechaentrega))
                            {
                            crearVehiculo.fecha_entrega = DateTime.Parse(modelo.fechaentrega, miCulturaTime);
                            }


                        crearVehiculo.ubicacionactual = modelo.ubicacion;
                        crearVehiculo.icbvhfec_actualizacion = DateTime.Now;
                        crearVehiculo.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        crearVehiculo.ciudadplaca = modelo.ciudadplaca;
                        crearVehiculo.nitprenda = modelo.nitprenda;
                        crearVehiculo.idasesor = modelo.idasesor;
                        crearVehiculo.concesionariovh = modelo.nombreconcesionario;

                        context.Entry(crearVehiculo).State = EntityState.Modified;

                        context.SaveChanges();
                        icb_referencia refer = context.icb_referencia.Where(x => x.ref_codigo == modelo.plan_mayor)
                            .FirstOrDefault();

                        refer.ref_codigo = modelo.plan_mayor;
                        refer.ref_fecha_actualizacion = DateTime.Now;
                        refer.ref_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        refer.ref_valor_unitario = Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                        refer.ref_valor_total = Convert.ToDecimal(modelo.costototal_vh, miCultura);
                        refer.manejo_inv = true;
                        refer.por_iva_compra = float.Parse(Request["textIVACompra"]);
                        refer.idporivacompra = Convert.ToInt32(Request["valIVACompra"]);
                        refer.impconsumo = float.Parse(Request["text_impoconsumo"]);
                        refer.por_iva = float.Parse(Request["textIVAVenta"]);
                        refer.idporivaventa = Convert.ToInt32(Request["valIVAVenta"]);
                        context.Entry(refer).State = EntityState.Modified;
                        context.SaveChanges();

                        cambioubicacionveh cambio = context.cambioubicacionveh.Where(x => x.idvehiculo == modelo.plan_mayor)
                            .FirstOrDefault();
                        cambio.idvehiculo = modelo.plan_mayor;
                        cambio.idubicanterior = cambio.idubicactual;
                        cambio.idubicactual = Convert.ToInt32(modelo.nombreUbicacion);
                        cambio.fechacambio = DateTime.Now;
                        cambio.iduser = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(cambio).State = EntityState.Modified;
                        context.SaveChanges();
                        TempData["mensaje"] = "La actualización del vehiculo fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                }
            }
            //}
            int bod = Convert.ToInt32(Session["user_bodega"]);

            ViewBag.anioSeleccionado = modelo.anio_vh;
            ViewBag.proveedorElegido = modelo.proveedor_id;
            ViewBag.nombreUbicacion =
                new SelectList(
                    context.ubicacion_bodega.Where(x => x.estado && x.idbodega == bod && x.tipo == 2)
                        .OrderBy(x => x.descripcion), "id", "descripcion", modelo.ubicacion);
            string UltimaUbicacion = (from a in context.cambioubicacionveh
                                      join b in context.ubicacion_bodega
                                          on a.idubicanterior equals b.id
                                      where a.idvehiculo == modelo.plan_mayor
                                      select b.descripcion).FirstOrDefault();
            modelo.nombreUltimaUbicacion = UltimaUbicacion != null ? UltimaUbicacion : "No ha cambiado la ubicación";

            BuscarDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ListasDesplegables(modelo);
            return View(modelo);
        }

        public JsonResult ValidarCostosVehiculo(string codigoModelo, string anio, bool actualizar, string placa)
        {
            if (anio != "")
            {
                int anio2 = Convert.ToInt32(anio);
                if (actualizar)
                {
                    var buscarCostos = (from a in context.anio_modelo
                                        join b in context.icb_vehiculo
                                            on a.codigo_modelo equals b.modvh_id
                                        where codigoModelo == b.modvh_id && anio2 == b.anio_vh && placa == b.plan_mayor
                                        select new
                                        {
                                            valor = b.costosiniva_vh,
                                            a.porcentaje_iva,
                                            a.impuesto_consumo,
                                            a.idporcentajecompra,
                                            a.idporcentajeiva,
                                            a.idporcentajeimpoconsumo
                                        }).FirstOrDefault();

                    if (buscarCostos != null)
                    {
                        var calcularCostos = new
                        {
                            valor = buscarCostos.valor.ToString("N0", new CultureInfo("is-IS")),
                            montoiva = (buscarCostos.valor * (buscarCostos.porcentaje_iva / 100)).ToString("N0",
                                new CultureInfo("is-IS")),
                            montoimconsumo =
                                (buscarCostos.valor * (buscarCostos.impuesto_consumo / 100)).ToString("N0",
                                    new CultureInfo("is-IS")),
                            costoTotal =
                                (buscarCostos.valor + buscarCostos.porcentaje_iva * buscarCostos.valor / 100).ToString(
                                    "N0",
                                    new CultureInfo(
                                        "is-IS")), //  costoTotal = (buscarCostos.valor + (buscarCostos.porcentaje_iva * buscarCostos.valor / 100)).ToString().Replace(".", ","),
                            porcentaje = buscarCostos.idporcentajecompra.ToString("N0", new CultureInfo("is-IS")),
                            impconsumo = buscarCostos.idporcentajeimpoconsumo.ToString("N0", new CultureInfo("is-IS")),
                            por_iva_venta = buscarCostos.idporcentajeiva.ToString("N0", new CultureInfo("is-IS"))
                        };

                        var equipamientoVH = (from m in context.modelo_vehiculo
                                              join e in context.vequipamiento
                                                  on m.idequipamiento equals e.id
                                              where m.modvh_codigo == codigoModelo
                                              select new
                                              {
                                                  m.idequipamiento,
                                                  e.codigo,
                                                  e.Descripcion
                                              }).FirstOrDefault();

                        return Json(new { calcularCostos, equipamientoVH }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        anio_modelo buscarCostos2 =
                            context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == codigoModelo && x.anio == anio2);
                        var calcularCostos = new
                        {
                            valor = buscarCostos2.valor.ToString("N0", new CultureInfo("is-IS")),
                            montoiva = (buscarCostos2.valor * (buscarCostos2.porcentaje_iva / 100)).ToString("N0",
                                new CultureInfo("is-IS")),
                            montoimconsumo =
                                (buscarCostos2.valor * (buscarCostos2.impuesto_consumo / 100)).ToString("N0",
                                    new CultureInfo("is-IS")),
                            costoTotal =
                                (buscarCostos2.valor + buscarCostos2.porcentaje_iva * buscarCostos2.valor / 100)
                                .ToString("N0",
                                    new CultureInfo(
                                        "is-IS")), //  costoTotal = (buscarCostos.valor + (buscarCostos.porcentaje_iva * buscarCostos.valor / 100)).ToString().Replace(".", ","),
                            porcentaje = buscarCostos2.idporcentajecompra.ToString("N0", new CultureInfo("is-IS")),
                            impconsumo = buscarCostos2.idporcentajeimpoconsumo.ToString("N0", new CultureInfo("is-IS")),
                            por_iva_venta = buscarCostos2.idporcentajeiva.ToString("N0", new CultureInfo("is-IS"))
                        };

                        var equipamientoVH = (from m in context.modelo_vehiculo
                                              join e in context.vequipamiento
                                                  on m.idequipamiento equals e.id
                                              where m.modvh_codigo == codigoModelo
                                              select new
                                              {
                                                  m.idequipamiento,
                                                  e.codigo,
                                                  e.Descripcion
                                              }).FirstOrDefault();

                        return Json(new { calcularCostos, equipamientoVH }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (string.IsNullOrWhiteSpace(codigoModelo) || /*anio == null*/string.IsNullOrWhiteSpace(anio))
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                }

                {
                    anio_modelo buscarCostos =
                        context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == codigoModelo && x.anio == anio2);
                    var calcularCostos = new
                    {
                        valor = buscarCostos.valor.ToString("N0", new CultureInfo("is-IS")),
                        montoiva = (buscarCostos.valor * (buscarCostos.porcentaje_iva / 100)).ToString("N0",
                            new CultureInfo("is-IS")),
                        montoimconsumo =
                            (buscarCostos.valor * (buscarCostos.impuesto_consumo / 100)).ToString("N0",
                                new CultureInfo("is-IS")),
                        costoTotal =
                            (buscarCostos.valor + buscarCostos.porcentaje_iva * buscarCostos.valor / 100).ToString("N0",
                                new CultureInfo(
                                    "is-IS")), //  costoTotal = (buscarCostos.valor + (buscarCostos.porcentaje_iva * buscarCostos.valor / 100)).ToString().Replace(".", ","),
                        porcentaje = buscarCostos.idporcentajecompra.ToString("N0", new CultureInfo("is-IS")),
                        impconsumo = buscarCostos.idporcentajeimpoconsumo.ToString("N0", new CultureInfo("is-IS")),
                        por_iva_venta = buscarCostos.idporcentajeiva.ToString("N0", new CultureInfo("is-IS"))
                    };

                    var equipamientoVH = (from m in context.modelo_vehiculo
                                          join e in context.vequipamiento
                                              on m.idequipamiento equals e.id
                                          where m.modvh_codigo == codigoModelo
                                          select new
                                          {
                                              m.idequipamiento,
                                              e.codigo,
                                              e.Descripcion
                                          }).FirstOrDefault();

                    return Json(new { calcularCostos, equipamientoVH }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public void BuscarDatosCreacion(CrearVehiculoModel vehiculo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(vehiculo.icbvhuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(vehiculo.icbvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult GetVehiculosJson()
        {
            var Buscarvehiculos = (from vehiculo in context.icb_vehiculo
                                   join modelov in context.modelo_vehiculo
                                       on vehiculo.modvh_id equals modelov.modvh_codigo
                                   select new
                                   {
                                       vehiculo.plan_mayor,
                                       vehiculo.vin,
                                       modelov.modvh_codigo,
                                       modelov.modvh_nombre,
                                       vehiculo.anio_vh,
                                       vehiculo.icbvhfec_creacion
                                   }).ToList();

            var vehiculos = Buscarvehiculos.Select(x => new
            {
                x.plan_mayor,
                x.vin,
                x.modvh_nombre,
                x.modvh_codigo,
                x.anio_vh,
                icbvhfec_creacion = x.icbvhfec_creacion.ToString("yyyy/MM/dd")
            }).ToList();
            return Json(vehiculos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarSeguimientoVH(string planmayor)
        {
            List<seguimiento_admin_vh> buscarSeg = context.seguimiento_admin_vh.Where(x => x.planmayor == planmayor).ToList();
            var data = buscarSeg.Select(x => new
            {
                x.id,
                nombre = x.userid_creacion != null ? x.users.user_nombre + " " + x.users.user_apellido : "",
                x.observacion,
                fecha = x.fec_creacion != null ? x.fec_creacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.planmayor
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarSeguimientoVH(string planmayor, string nota)
        {
            if (planmayor != null && nota != null)
            {
                seguimiento_admin_vh seguimiento = new seguimiento_admin_vh
                {
                    planmayor = planmayor,
                    observacion = nota,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                };
                context.seguimiento_admin_vh.Add(seguimiento);
                context.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        //funcion para buscar si plan mayor existe
        public JsonResult buscarplanmayor(string plan)
        {
            //esto es si plan es nulo o espacios en blanco
            if (string.IsNullOrWhiteSpace(plan))
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

            //Cuento cuantos existen con este plan mayor
            int existe = context.icb_vehiculo.Where(x => x.plan_mayor == plan).Count();
            if (existe > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarseriecrea(string serie)
        {
            //esto es si plan es nulo o espacios en blanco
            if (string.IsNullOrWhiteSpace(serie))
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

            //Cuento cuantos existen con este plan mayor
            int existe = context.icb_vehiculo.Where(x => x.vin == serie).Count();
            if (existe > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarmotorcrea(string motor)
        {
            //esto es si plan es nulo o espacios en blanco
            if (string.IsNullOrWhiteSpace(motor))
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

            //Cuento cuantos existen con este plan mayor
            int existe = context.icb_vehiculo.Where(x => x.nummot_vh == motor).Count();
            if (existe > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarplacacrea(string placa)
        {
            //esto es si plan es nulo o espacios en blanco
            if (string.IsNullOrWhiteSpace(placa))
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

            //Cuento cuantos existen con este plan mayor
            int existe = context.icb_vehiculo.Where(x => x.plac_vh == placa).Count();
            if (existe > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult buscarPedidos()
        {
            return View();
        }

        public JsonResult buscarPedido()
        {
            var buscar = (from a in context.vw_browserBackOffice
                          where a.planmayor == null
                          select new
                          {
                              a.numero,
                              a.fecha,
                              a.anio_vh,
                              a.modelo,
                              a.colvh_nombre,
                              a.bodccs_nombre,
                              a.ubivh_nombre,
                              a.asesor,
                              a.planmayor,
                              a.vin,
                              a.doc_tercero,
                              a.prinom_tercero,
                              a.segnom_tercero,
                              a.apellido_tercero,
                              a.segapellido_tercero,
                              a.razon_social,
                              a.vrtotal,
                              a.valor,
                              a.proceso
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.numero,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                anio = x.anio_vh != null ? x.anio_vh : 0,
                x.modelo,
                color = x.colvh_nombre != null ? x.colvh_nombre : "",
                x.bodccs_nombre,
                ubicacion = x.ubivh_nombre != null ? x.ubivh_nombre : "",
                x.asesor,
                planmayor = x.planmayor != null ? x.planmayor : "",
                vin = x.vin != null ? x.vin : "",
                cliente = x.razon_social != null
                    ? x.doc_tercero + "-" + x.razon_social
                    : x.doc_tercero + "-" + x.prinom_tercero + "-" + x.segnom_tercero + "-" + x.apellido_tercero + "-" +
                      x.segapellido_tercero,
                x.vrtotal,
                x.valor,
                x.proceso
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult buscarSinFactura()
        {
            return View();
        }

        public JsonResult buscarSinFacturaJson()
        {
            var data = from p in context.vw_browserBackOffice
                       where p.facturado == false && p.planmayor != null
                       select new
                       {
                           p.id,
                           p.numero,
                           p.bodega,
                           p.bodccs_nombre,
                           p.proceso,
                           p.modelo,
                           p.vrtotal,
                           placa = p.plac_vh != null ? p.plac_vh : "",
                           color = p.colvh_nombre != null ? p.colvh_nombre : "",
                           vin = p.vin != null ? p.vin : "",
                           ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                           anio = p.anio_vh != null ? p.anio_vh : null,
                           fechaMatricula = p.fecmatricula != null ? p.fecmatricula.Value.ToString() : "",
                           valor = p.valor != null ? p.valor.ToString() : "0",
                           p.asesor,
                           cliente = p.razon_social != null
                               ? p.doc_tercero + "-" + p.razon_social
                               : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero +
                                 " " + p.segapellido_tercero,
                           fecha = p.fecha.ToString(),
                           fechaA = p.fecha_asignacion_planmayor.ToString(),
                           saldo = p.saldo != null ? p.saldo.ToString() : " ",
                           planmayor = p.planmayor.ToString(),
                           fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString(),
                           p.autorizado,
                           p.autorizados
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult buscarSinMatricula()
        {
            return View();
        }

        public JsonResult buscarSinMatriculaJson()
        {
            var data = from p in context.vw_browserBackOffice
                       where p.facturado && p.fecmatricula == null && p.planmayor != null
                       select new
                       {
                           p.id,
                           p.numero,
                           p.bodega,
                           p.bodccs_nombre,
                           p.proceso,
                           p.modelo,
                           p.vrtotal,
                           placa = p.plac_vh != null ? p.plac_vh : "",
                           color = p.colvh_nombre != null ? p.colvh_nombre : "",
                           vin = p.vin != null ? p.vin : "",
                           ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                           anio = p.anio_vh != null ? p.anio_vh : null,
                           fechaMatricula = p.fecmatricula != null ? p.fecmatricula.Value.ToString() : "",
                           valor = p.valor != null ? p.valor.ToString() : "0",
                           p.asesor,
                           cliente = p.razon_social != null
                               ? p.doc_tercero + "-" + p.razon_social
                               : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero +
                                 " " + p.segapellido_tercero,
                           fecha = p.fecha.ToString(),
                           fechaA = p.fecha_asignacion_planmayor.ToString(),
                           saldo = p.saldo != null ? p.saldo.ToString() : " ",
                           planmayor = p.planmayor.ToString(),
                           fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString(),
                           p.autorizado,
                           p.autorizados
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult buscarEnTramite()
        {
            return View();
        }

        public ActionResult buscarSinEntragar()
        {
            return View();
        }

        public JsonResult buscarSinEntraga()
        {
            var data = from p in context.vw_browserBackOffice
                       where p.facturado && p.fecmatricula != null && p.planmayor != null && p.fecha_entrega == null
                       select new
                       {
                           p.id,
                           p.numero,
                           p.bodega,
                           p.bodccs_nombre,
                           p.proceso,
                           p.modelo,
                           p.vrtotal,
                           placa = p.plac_vh != null ? p.plac_vh : "",
                           color = p.colvh_nombre != null ? p.colvh_nombre : "",
                           vin = p.vin != null ? p.vin : "",
                           ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                           anio = p.anio_vh != null ? p.anio_vh : null,
                           fechaMatricula = p.fecmatricula != null ? p.fecmatricula.Value.ToString() : "",
                           valor = p.valor != null ? p.valor.ToString() : "0",
                           p.asesor,
                           cliente = p.razon_social != null
                               ? p.doc_tercero + "-" + p.razon_social
                               : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero +
                                 " " + p.segapellido_tercero,
                           fecha = p.fecha.ToString(),
                           fechaA = p.fecha_asignacion_planmayor.ToString(),
                           saldo = p.saldo != null ? p.saldo.ToString() : " ",
                           planmayor = p.planmayor.ToString(),
                           fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString(),
                           p.autorizado,
                           p.autorizados
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarClientePropietario(int tipo)
        {
            //1 = nuevo | 0 = usado
            if (tipo == 1)
            {
                var propietario = (from p in context.icb_terceros
                                   where p.doc_tercero == "816003186"
                                   select new
                                   {
                                       p.tercero_id,
                                       nombre = p.prinom_tercero != null
                                           ? "(" + p.doc_tercero + ") - " + p.prinom_tercero + " " + p.segnom_tercero + " " +
                                             p.apellido_tercero + " " + p.segapellido_tercero
                                           : "(" + p.doc_tercero + ") - " + p.razon_social
                                   }).ToList();

                return Json(propietario, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var propietario = from pro in context.tercero_cliente
                                  join p in context.icb_terceros
                                      on pro.tercero_id equals p.tercero_id
                                  select new
                                  {
                                      p.tercero_id,
                                      nombre = p.prinom_tercero != null
                                          ? "(" + p.doc_tercero + ") - " + p.prinom_tercero + " " + p.segnom_tercero + " " +
                                            p.apellido_tercero + " " + p.segapellido_tercero
                                          : "(" + p.doc_tercero + ") - " + p.razon_social
                                  };

                return Json(propietario, JsonRequestBehavior.AllowGet);
            }
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }

        /// <summary>
        ///     Vehiculo para enviar a exhibición, estos vehiculos estan asignados a una vitrina pero pueden ser usados para
        ///     negocio,
        ///     cabe aclarar que el cliente puede o no comprar el vehiculo en exhibición, en caso de que así sea el vehiculo pasa a
        ///     facturación, en caso de que quiera el vehiculo pero no con los accesorios instalados, se devuelve para que sean
        ///     removido
        ///     o se agreguen nuevos accesorios, todo esto con un OT que permita seleccionar el vehículo.
        /// </summary>
        /// <param name="plan_mayor"></param>
        /// <param name="id_bodega"></param>
        /// <returns></returns>
        public JsonResult sendCarExhibition(string plan_mayor, int? id_bodega)
        {
            icb_vehiculo_eventos newEvent = new icb_vehiculo_eventos();

            string respuesta = "";
            int valor = 0;
            if (!string.IsNullOrWhiteSpace(plan_mayor) && id_bodega != null)
            {
                //buscar el evento envio a exhibicion
                icb_sysparameter par1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P120").FirstOrDefault();
                int eventali = par1 != null ? Convert.ToInt32(par1.syspar_value) : 24;
                icb_tpeventos SearchEventali = context.icb_tpeventos.FirstOrDefault(x => x.codigoevento == eventali);
                //buscar el id del asesor que registra el evento
                int idasesor = Convert.ToInt32(Session["user_usuarioid"]);
                //busco si el vehiculo_existe
                icb_vehiculo vehi = context.icb_vehiculo.Where(d => d.plan_mayor == plan_mayor).FirstOrDefault();
                if (vehi != null)
                {
                    //VERIFICO SI YA FUE ENVIADO A ALISTAMiENTO
                    icb_vehiculo_eventos existeenvioalistamiento = context.icb_vehiculo_eventos.AsNoTracking().Where(d =>
                        d.id_tpevento == SearchEventali.tpevento_id && d.planmayor == plan_mayor &&
                        d.evento_estado == false).FirstOrDefault();
                    if (existeenvioalistamiento == null)
                    {
                        int? owner_vh = vehi.propietario;

                        if (owner_vh != null || owner_vh > 0)
                        {
                            newEvent.terceroid = owner_vh;
                        }

                        newEvent.eventofec_creacion = DateTime.Now;
                        newEvent.eventouserid_creacion = idasesor;
                        newEvent.evento_estado = true;
                        newEvent.bodega_id = id_bodega.Value;
                        newEvent.evento_nombre = SearchEventali.tpevento_nombre;
                        newEvent.planmayor = plan_mayor;
                        newEvent.fechaevento = DateTime.Now;
                        newEvent.id_tpevento = SearchEventali.tpevento_id;
                        context.icb_vehiculo_eventos.Add(newEvent);
                        try
                        {
                            int answer_event = context.SaveChanges();
                            if (answer_event > 0)
                            {
                                respuesta = "El vehículo ha sido enviado a exhibicion exitosamente";
                                valor = 1;
                            }
                        }
                        catch (Exception e)
                        {
                            respuesta = e.Message;
                            valor = 0;
                        }
                    }
                    else
                    {
                        respuesta = "El vehículo ya fue enviado a exhibicion";
                        valor = 0;
                    }

                    if (existeenvioalistamiento != null)
                    {
                        newEvent.evento_id = existeenvioalistamiento.evento_id;
                        newEvent.eventofec_actualizacion = DateTime.Now;
                        newEvent.eventouserid_actualizacion = idasesor;
                        newEvent.evento_estado = true;
                        newEvent.evento_nombre = SearchEventali.tpevento_nombre;
                        context.icb_vehiculo_eventos.Attach(newEvent);
                        context.Entry(newEvent).Property(x => x.evento_estado).IsModified = true;
                        context.Entry(newEvent).Property(x => x.eventofec_actualizacion).IsModified = true;
                        context.Entry(newEvent).Property(x => x.eventouserid_actualizacion).IsModified = true;
                        try
                        {
                            int answer_event = context.SaveChanges();
                            if (answer_event > 0)
                            {
                                respuesta = "El vehículo ha sido enviado a exhibicion exitosamente";
                                valor = 1;
                            }
                        }
                        catch (Exception e)
                        {
                            respuesta = e.Message;
                            valor = 0;
                        }
                    }
                }
                else
                {
                    respuesta = "Debe ingresar un plan mayor válido";
                    valor = 0;
                }
            }
            else
            {
                respuesta = "Debe ingresar un plan mayor y una bodega";
                valor = 0;
            }

            var data = new
            {
                respuesta,
                valor
            };

            return Json(data);
        }

        public JsonResult quitarCarroExhibicion(icb_vehiculo_eventos vehiculoenvento, string plan_mayor, int? id_bodega,
            int evento_id)
        {
            string respuesta = "";
            int valor = 0;
            if (!string.IsNullOrWhiteSpace(plan_mayor) && id_bodega != null)
            {
                //buscar el evento envio a exhibicion
                icb_sysparameter par1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P120").FirstOrDefault();
                int eventali = par1 != null ? Convert.ToInt32(par1.syspar_value) : 24;
                icb_tpeventos SearchEventali = context.icb_tpeventos.FirstOrDefault(x => x.codigoevento == eventali);
                //buscar el id del asesor que registra el evento
                int idasesor = Convert.ToInt32(Session["user_usuarioid"]);
                //busco si el vehiculo_existe
                icb_vehiculo vehi = context.icb_vehiculo.Where(d => d.plan_mayor == plan_mayor).FirstOrDefault();
                if (vehi != null)
                {
                    //VERIFICO SI YA FUE ENVIADO A ALISTAMiENTO
                    icb_vehiculo_eventos existeenvioalistamiento = context.icb_vehiculo_eventos.Where(d =>
                        d.id_tpevento == SearchEventali.tpevento_id && d.planmayor == plan_mayor &&
                        d.evento_estado == false).FirstOrDefault();
                    if (existeenvioalistamiento == null)
                    {
                        vehiculoenvento.evento_id = evento_id;
                        vehiculoenvento.eventofec_actualizacion = DateTime.Now;
                        vehiculoenvento.eventouserid_actualizacion = idasesor;
                        vehiculoenvento.evento_estado = false;
                        vehiculoenvento.evento_nombre = SearchEventali.tpevento_nombre;
                        context.icb_vehiculo_eventos.Attach(vehiculoenvento);
                        context.Entry(vehiculoenvento).Property(x => x.evento_estado).IsModified = true;
                        context.Entry(vehiculoenvento).Property(x => x.eventofec_actualizacion).IsModified = true;
                        context.Entry(vehiculoenvento).Property(x => x.eventouserid_actualizacion).IsModified = true;
                        try
                        {
                            int answer_event = context.SaveChanges();
                            if (answer_event > 0)
                            {
                                respuesta = "El vehículo ha sido quitado de exhibicion exitosamente";
                                valor = 1;
                            }
                        }
                        catch (Exception e)
                        {
                            respuesta = e.Message;
                            valor = 0;
                        }
                    }
                    else
                    {
                        respuesta = "Hubo un error al quitar el vehiculo de exhibicion.";
                        valor = 0;
                    }
                }
                else
                {
                    respuesta = "Debe ingresar un plan mayor válido";
                    valor = 0;
                }
            }
            else
            {
                respuesta = "Debe ingresar un plan mayor y una bodega";
                valor = 0;
            }

            var data = new
            {
                respuesta,
                valor
            };

            return Json(data);
        }

        // Este es la función para el browser de vehiculos nuevos, aquí se mostrará los campos de vehiculos que están nuevos e informa en que estado está el vehiculo, posteriormente se creará una función para liberar los vehiculos que estuvieron en negocio pero ya no.

        public JsonResult NewVehicles(int? idCheck)
        {

            List<vw_historial_vehiculos_nuevos> query2 = context.vw_historial_vehiculos_nuevos.ToList();
            var Query = query2.Select(d => new
            {
                receptionDate = d.FechaRecepcionBodega != null
                    ? d.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                payDate = d.FechaCompra != null ? d.FechaCompra : "",
                ActDate = d.icbvhfec_actualizacion != null
                    ? d.icbvhfec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                reception = d.bodccs_nombre,
                contribute = d.numerocotizacion != null ? d.numerocotizacion : "",
                order = d.numeropedido != null ? d.numeropedido : "",
                statusCar = d.estado_ve,
                modelSerial = d.modelovehiculo != null ? d.modelovehiculo : "",
                model = d.modvh_nombre != null ? d.modvh_nombre : "",
                segment = d.segvh_nombre,
                sheet = d.placavehiculo != null ? d.placavehiculo : "",
                flee = d.flota != null ? d.flota.ToString() : "",
                agent = d.user_nombre + " " + d.user_apellido,
                policy = !string.IsNullOrWhiteSpace(d.poliza) ? d.poliza : "",
                days = d.diasInventario != null ? d.diasInventario.Value.ToString() : "",
                receptionChek = d.recepcion,
            }).ToList();
            return Json(Query);
        }

        //Vehiculos usados se mantiene en pausia de forma que estará comentado
        //public JsonResult UsedVehicles(int? idCheck)
        //{

        //    var query2 = context.vw_historial_vehiculos_nuevos.ToList();
        //    var Query = query2.Select(d => new
        //    {
        //        d.plan_mayor,
        //        receptionDate = d.FechaRecepcionBodega != null ? d.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
        //        payDate = d.fecha != null ? d.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",                
        //        ActDate = d.icbvhfec_actualizacion != null ? d.icbvhfec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
        //        reception = d.bodccs_nombre,
        //        contribute = d.cot_numcotizacion,
        //        order = d.numero,
        //        statusCar = d.estado_ve,
        //        model = d.modvh_nombre,
        //        segment = d.segvh_nombre,
        //        sheet = d.placavehiculo != null ? d.placavehiculo : "",
        //        flee = d.flota != null ? d.flota.ToString() : "",
        //        agent = d.user_nombre + " " + d.user_apellido,
        //        policy = !string.IsNullOrWhiteSpace(d.poliza) ? d.poliza : "",
        //        days = d.diasInventario != null ? d.diasInventario.Value.ToString() : "",
        //    }).ToList();

        //Función para liberar vehiculos, estos vehiculos que están en proceso de negocio y por alguna razón ya no van a estar mas en negocio
        public JsonResult liberarCarButton(int id)
        {
            int resultado = 0;
            string mensaje = "";
            vpedido query = context.vpedido.Where(d => d.planmayor != null && d.numero == id).FirstOrDefault();
            //si el resultado de mi busqueda no es null
            if (query != null)
            {
                //cvapturo en variable el plan mayor
                string plan_mayor = query.planmayor;
                //busco el ultimo evento del vehiculo registrado en icb_vehiculo_eventos
                icb_vehiculo_eventos ultimoevento = context.icb_vehiculo_eventos.OrderByDescending(d => d.evento_id)
                    .Where(d => d.planmayor == plan_mayor).FirstOrDefault();
                //creo uno nuevo con las caracteristicss 
                if (ultimoevento != null)
                {
                }

                //al pedido le quito plan mayor
                query.planmayor = null;
                //guardo
                context.Entry(query).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                //retorno
                if (guardar > 0)
                {
                    resultado = 1;
                }
            }

            var data = new
            {
                resultado,
                mensaje
            };

            return Json(data);
        }

        public ActionResult BrowserVhConAverias()
        {
            return View();
        }

        public ActionResult buscarVhConAverias()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            //var info = (from rt in context.vw_referencias_total
            //            join ev in context.icb_vehiculo_eventos
            //            on rt.codigo equals ev.planmayor
            //            select new
            //            {
            //                rt.modvh_nombre,
            //                rt.vin,
            //                rt.codigo,
            //                rt.anio_vh,
            //                rt.colvh_nombre,
            //                ev.placa,
            //                rt.nuevo,
            //                rt.Estado
            //            }).Where(z => z.vin != null && z.nuevo == true && z.Estado == "Activo");

            //var data = info.Select(x => new
            //{
            //    modelo = x.modvh_nombre,
            //    vin = x.vin,
            //    planmayor = x.codigo,
            //    anio = x.anio_vh,
            //    color = x.colvh_nombre,
            //    placa = x.placa
            //});
            string sDate = DateTime.Now.ToString();
            DateTime datevalue = Convert.ToDateTime(sDate);
            int mn = datevalue.Month;
            int yy = datevalue.Year;
            List<icb_vehiculo> datdistinctct = context.icb_vehiculo.Where(p => p.plan_mayor != null && p.plan_mayor != string.Empty).ToList();
            //var data2 = context.vw_referencias_total.Where(v =>
            //    v.ano == yy && v.mes == mn && v.bodega == bodegaActual && v.nuevo == true && v.stock > 0 &&
            //    !datdistinctct.Contains(v.codigo)).ToList();
            var data = datdistinctct.Select(v => new
            {
                modelo = v.modvh_id != null ? v.modelo_vehiculo.modvh_nombre : "",
                planmayor = v.plan_mayor,
                v.vin,
                placa = v.plac_vh != null ? v.plac_vh : "",
                anomodelo = v.anio_vh,
                color = v.colvh_id != null ? context.color_vehiculo.Where(x => x.colvh_id == v.colvh_id).FirstOrDefault().colvh_nombre : "",
                cantidadAverias = context.icb_inspeccionvehiculos.Where(x => x.planmayor == v.plan_mayor).Count(),
                bodega = context.icb_vehiculo_eventos.Where(x => x.planmayor == v.plan_mayor && x.id_tpevento == 2).Count()
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}