using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class modelo_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void ParametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 27);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        public ActionResult Create(int? menu)
        {
            ViewBag.mar_vh_id = new SelectList(context.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id",
                "marcvh_nombre");
            ViewBag.seg_vh_id = new SelectList(context.segmento_vehiculo.OrderBy(x => x.segvh_nombre), "segvh_id",
                "segvh_nombre");
            ViewBag.grupo_id = new SelectList(context.vgrupo.OrderBy(x => x.nombre), "grupo_id", "nombre");
            ViewBag.clase_id = new SelectList(context.vclase.OrderBy(x => x.nombre), "clase_id", "nombre");
            ViewBag.tipo_id = new SelectList(context.vtipo.OrderBy(x => x.nombre), "tipo_id", "nombre");
            ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo.OrderBy(x => x.tpmot_nombre), "tpmot_id",
                "tpmot_nombre");
            ViewBag.perfil_id = new SelectList(context.vperfil.OrderBy(x => x.nombre), "perfil_id", "nombre");
            ViewBag.unidadcarga = new SelectList(context.vunidadcarga.OrderBy(x => x.unidad), "id", "unidad");
            ViewBag.tipocaja = new SelectList(context.tpcaja_vehiculo.OrderBy(x => x.tpcaj_nombre), "tpcaj_id",
                "tpcaj_nombre");
            ViewBag.modelogkit = new SelectList(context.vmodelog.OrderBy(x => x.Descripcion), "id", "Descripcion");
            ViewBag.clasificacion = new SelectList(context.clasificacion_vehiculo.OrderBy(x => x.clavh_nombre),
                "clavh_id", "clavh_nombre");

            ModeloVehiculosModel modVh = new ModeloVehiculosModel();

            ViewBag.porcentaje_iva =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id", "porcentaje");
            ViewBag.porcentaje_compra = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"), "id",
                "porcentaje");
            ViewBag.porcentaje_impoconsumo =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje");
            ViewBag.idequipamiento =
                new SelectList(context.vequipamiento.Where(x => x.estado).OrderByDescending(x => x.codigo), "id",
                    "codigo");

            modVh.modvh_estado = true;
            modVh.modvhrazoninactivo = "No aplica";
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View(modVh);
        }

        // POST: mod_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ModeloVehiculosModel mod_vh, int? menu)
        {
            ViewBag.modelogkit = new SelectList(context.vmodelog, "id", "Descripcion");
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                modelo_vehiculo modSearch = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == mod_vh.modvh_codigo);

                if (modSearch == null)
                {
                    modelo_vehiculo nuevo = new modelo_vehiculo
                    {

                        //mod_vh.porcentaje_iva = Convert.ToDecimal(Request["textIVAVenta"]);
                        //mod_vh.impuesto_Consumo = Convert.ToDecimal(Request["textImpo"]);
                        modvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        modvhfec_creacion = DateTime.Now,
                        mar_vh_id = mod_vh.mar_vh_id,
                        seg_vh_id = mod_vh.seg_vh_id,
                        capacidad = mod_vh.capacidad,
                        grupo = mod_vh.grupo_id,
                        tipo = mod_vh.tipo_id,
                        clase = mod_vh.clase_id,
                        cilindraje = mod_vh.cilindraje,
                        combustible = mod_vh.tpmot_id,
                        perfil = mod_vh.perfil_id,
                        unidadcarga = mod_vh.unidadcarga,
                        modvh_codigo = mod_vh.modvh_codigo,
                        modvh_nombre = mod_vh.modvh_nombre,
                        modvh_estado = mod_vh.modvh_estado,
                        modvhrazoninactivo = mod_vh.modvhrazoninactivo,
                        diaslibrescaplan = mod_vh.diaslibrescaplan,
                        diaslibresgmac = mod_vh.diaslibresgmac,
                        modelogkit = mod_vh.modelogkit,
                        tipocaja = mod_vh.tipocaja,
                        clasificacion = mod_vh.clasificacion,
                        idequipamiento = mod_vh.idequipamiento
                    };

                    context.modelo_vehiculo.Add(nuevo);
                    bool guardaModelo = context.SaveChanges() > 0;

                    //context.anio_modelo.Add(new anio_modelo()
                    //{
                    //    codigo_modelo = mod_vh.modvh_codigo,
                    //    anio = mod_vh.anio_modelo ?? 0,
                    //    valor = Convert.ToDecimal(mod_vh.valor_modelo),
                    //    descripcion = mod_vh.modvh_nombre,
                    //    porcentaje_iva = Convert.ToDecimal(Request["textIVAVenta"]),
                    //    idporcentajeiva = Convert.ToInt32(Request["valIVAVenta"]),
                    //    porcentaje_compra = Convert.ToDecimal(Request["textCompra"]),
                    //    idporcentajecompra = Convert.ToInt32(Request["valCompra"]),
                    //    impuesto_consumo = Convert.ToDecimal(Request["textImpo"]),
                    //    idporcentajeimpoconsumo = Convert.ToInt32(Request["valImpo"])
                    //});
                    //var guardaAnioModelo = context.SaveChanges();

                    if (guardaModelo)
                    {
                        TempData["mensaje"] = "El registro del nuevo modelo de vehiculo fue exitoso!";
                        ViewBag.mar_vh_id = new SelectList(context.marca_vehiculo.OrderBy(x => x.marcvh_nombre),
                            "marcvh_id", "marcvh_nombre");
                        ViewBag.seg_vh_id = new SelectList(context.segmento_vehiculo.OrderBy(x => x.segvh_nombre),
                            "segvh_id", "segvh_nombre");
                        ViewBag.grupo_id = new SelectList(context.vgrupo.OrderBy(x => x.nombre), "grupo_id", "nombre");
                        ViewBag.clase_id = new SelectList(context.vclase.OrderBy(x => x.nombre), "clase_id", "nombre");
                        ViewBag.tipo_id = new SelectList(context.vtipo.OrderBy(x => x.nombre), "tipo_id", "nombre");
                        ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo.OrderBy(x => x.tpmot_nombre),
                            "tpmot_id", "tpmot_nombre");
                        ViewBag.perfil_id =
                            new SelectList(context.vperfil.OrderBy(x => x.nombre), "perfil_id", "nombre");
                        ViewBag.unidadcarga =
                            new SelectList(context.vunidadcarga.OrderBy(x => x.unidad), "id", "unidad");
                        ViewBag.tipocaja = new SelectList(context.tpcaja_vehiculo.OrderBy(x => x.tpcaj_nombre),
                            "tpcaj_id", "tpcaj_nombre");
                        ViewBag.clasificacion =
                            new SelectList(context.clasificacion_vehiculo.OrderBy(x => x.clavh_nombre), "clavh_id",
                                "clavh_nombre");
                        ViewBag.porcentaje_iva = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"),
                            "id", "porcentaje");
                        ViewBag.porcentaje_compra =
                            new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"), "id",
                                "porcentaje");
                        ViewBag.porcentaje_impoconsumo =
                            new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id",
                                "porcentaje");
                        ViewBag.idequipamiento =
                            new SelectList(context.vequipamiento.Where(x => x.estado).OrderByDescending(x => x.codigo),
                                "id", "codigo");

                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ViewBag.mar_vh_id = new SelectList(context.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id",
                "marcvh_nombre");
            ViewBag.seg_vh_id = new SelectList(context.segmento_vehiculo.OrderBy(x => x.segvh_nombre), "segvh_id",
                "segvh_nombre");
            ViewBag.grupo_id = new SelectList(context.vgrupo.OrderBy(x => x.nombre), "grupo_id", "nombre");
            ViewBag.clase_id = new SelectList(context.vclase.OrderBy(x => x.nombre), "clase_id", "nombre");
            ViewBag.tipo_id = new SelectList(context.vtipo.OrderBy(x => x.nombre), "tipo_id", "nombre");
            ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo.OrderBy(x => x.tpmot_nombre), "tpmot_id",
                "tpmot_nombre");
            ViewBag.perfil_id = new SelectList(context.vperfil.OrderBy(x => x.nombre), "perfil_id", "nombre");
            ViewBag.unidadcarga = new SelectList(context.vunidadcarga.OrderBy(x => x.unidad), "id", "unidad");
            ViewBag.tipocaja = new SelectList(context.tpcaja_vehiculo.OrderBy(x => x.tpcaj_nombre), "tpcaj_id",
                "tpcaj_nombre");
            ViewBag.clasificacion = new SelectList(context.clasificacion_vehiculo.OrderBy(x => x.clavh_nombre),
                "clavh_id", "clavh_nombre");
            ViewBag.idequipamiento =
                new SelectList(context.vequipamiento.Where(x => x.estado).OrderByDescending(x => x.codigo), "id",
                    "codigo");

            ViewBag.porcentaje_iva =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id", "porcentaje");
            ViewBag.porcentaje_compra = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"), "id",
                "porcentaje");
            ViewBag.porcentaje_impoconsumo =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje");

            BuscarFavoritos(menu);
            return View(mod_vh);
        }

        public JsonResult buscarDescripcion(int equipamiento)
        {
            string data = context.vequipamiento.FirstOrDefault(x => x.id == equipamiento).Descripcion;

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //    TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

        //    var enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 27);
        //    string enlaces = "";
        //    foreach (var item in enlacesBuscar)
        //    {
        //        var buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
        //        enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
        //    }
        //    ViewBag.nombreEnlaces = enlaces;
        //    BuscarFavoritos(menu);
        //    return View(mod_vh);
        //}


        // GET: mod_vh/Edit/5
        public ActionResult update(string id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            modelo_vehiculo mod_vh = context.modelo_vehiculo.Find(id);
            if (mod_vh == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(mod_vh.modvhuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(mod_vh.modvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            ViewBag.mar_vh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre", mod_vh.mar_vh_id);
            ViewBag.seg_vh_id = new SelectList(context.segmento_vehiculo, "segvh_id", "segvh_nombre", mod_vh.seg_vh_id);
            ViewBag.grupo_id = new SelectList(context.vgrupo, "grupo_id", "nombre", mod_vh.grupo);
            ViewBag.clase_id = new SelectList(context.vclase, "clase_id", "nombre", mod_vh.clase);
            ViewBag.tipo_id = new SelectList(context.vtipo, "tipo_id", "nombre", mod_vh.tipo);
            ViewBag.modvh_codigo =
                new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre", mod_vh.modvh_codigo);
            ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo, "tpmot_id", "tpmot_nombre", mod_vh.combustible);
            ViewBag.perfil_id = new SelectList(context.vperfil, "perfil_id", "nombre", mod_vh.perfil);
            ViewBag.unidadcarga = new SelectList(context.vunidadcarga, "id", "unidad", mod_vh.unidadcarga);
            ViewBag.tipocaja = new SelectList(context.tpcaja_vehiculo, "tpcaj_id", "tpcaj_nombre", mod_vh.tipocaja);
            ViewBag.modelogkit = new SelectList(context.vmodelog, "id", "Descripcion", mod_vh.modelogkit);
            ViewBag.clasificacion = new SelectList(context.clasificacion_vehiculo.OrderBy(x => x.clavh_nombre),
                "clavh_id", "clavh_nombre", mod_vh.clasificacion);
            ViewBag.porcentaje_iva = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id",
                "porcentaje", mod_vh.anio_modelo);
            ViewBag.porcentaje_compra = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"), "id",
                "porcentaje", mod_vh.anio_modelo);
            ViewBag.porcentaje_impoconsumo =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje",
                    mod_vh.anio_modelo);

            ViewBag.porcentaje_iva_modal = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id",
                "porcentaje", mod_vh.anio_modelo);
            ViewBag.porcentaje_compra_modal = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"),
                "id", "porcentaje", mod_vh.anio_modelo);
            ViewBag.porcentaje_impoconsumo_modal = new SelectList(
                context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje", mod_vh.anio_modelo);
            ViewBag.idequipamiento =
                new SelectList(context.vequipamiento.Where(x => x.estado).OrderByDescending(x => x.codigo), "id",
                    "codigo", mod_vh.idequipamiento);

            ParametrosBusqueda();
            anio_modelo anio_modelo = new anio_modelo();
            ModeloVehiculosModel modelo = new ModeloVehiculosModel
            {
                modvh_nombre = mod_vh.modvh_nombre,
                modvh_estado = mod_vh.modvh_estado,
                modvh_codigo = mod_vh.modvh_codigo,
                capacidad = mod_vh.capacidad ?? 0,
                clase_id = mod_vh.clase ?? 0,
                cilindraje = mod_vh.cilindraje ?? 0,
                modvhfec_creacion = mod_vh.modvhfec_creacion,
                modvhfec_actualizacion = mod_vh.modvhfec_actualizacion ?? null,
                modvhuserid_creacion = mod_vh.modvhuserid_creacion,
                modvhuserid_actualizacion = mod_vh.modvhuserid_actualizacion ?? 0,
                perfil_id = mod_vh.perfil ?? 0,
                seg_vh_id = mod_vh.seg_vh_id,
                tipo_id = mod_vh.tipo ?? 0,
                tpmot_id = mod_vh.combustible ?? 0,
                grupo_id = mod_vh.grupo ?? 0,
                mar_vh_id = mod_vh.mar_vh_id,
                unidadcarga = mod_vh.unidadcarga,
                diaslibrescaplan = mod_vh.diaslibrescaplan,
                diaslibresgmac = mod_vh.diaslibresgmac,
                modelogkit = mod_vh.modelogkit,
                tipocaja = mod_vh.tipocaja,
                clasificacion = mod_vh.clasificacion,
                id_porcentaje_iva_modal = anio_modelo.idporcentajeiva,
                id_porcentaje_compra_modal = anio_modelo.idporcentajecompra,
                id_impoconsumo_modal = anio_modelo.idporcentajeimpoconsumo,
                porcentaje_iva_modal = anio_modelo.porcentaje_iva,
                porcentaje_compra_modal = Convert.ToInt32(anio_modelo.porcentaje_compra),
                impuesto_Consumo_modal = anio_modelo.impuesto_consumo,
                idequipamiento = mod_vh.idequipamiento
            };
            BuscarFavoritos(menu);
            return View(modelo);
        }

        // POST: mod_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(ModeloVehiculosModel mod_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //var modSearch = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_nombre == mod_vh.modvh_nombre
                //&& x.seg_vh_id==mod_vh.seg_vh_id && x.mar_vh_id==mod_vh.mar_vh_id && x.modvh_estado==mod_vh.modvh_estado);
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int modBuscado = (from a in context.modelo_vehiculo
                                  where a.modvh_nombre == mod_vh.modvh_nombre && a.modvh_codigo == mod_vh.modvh_codigo
                                  select a.modvh_nombre).Count();

                if (modBuscado == 1)
                {
                    modelo_vehiculo modeloActual =
                        context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == mod_vh.modvh_codigo);
                    modeloActual.modvhfec_actualizacion = DateTime.Now;
                    modeloActual.modvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modeloActual.capacidad = mod_vh.capacidad;
                    modeloActual.grupo = mod_vh.grupo_id;
                    modeloActual.tipo = mod_vh.tipo_id;
                    modeloActual.clase = mod_vh.clase_id;
                    modeloActual.cilindraje = mod_vh.cilindraje;
                    modeloActual.combustible = mod_vh.tpmot_id;
                    modeloActual.perfil = mod_vh.perfil_id;
                    modeloActual.unidadcarga = mod_vh.unidadcarga;
                    modeloActual.diaslibrescaplan = mod_vh.diaslibrescaplan;
                    modeloActual.diaslibresgmac = mod_vh.diaslibresgmac;
                    modeloActual.modelogkit = mod_vh.modelogkit;
                    modeloActual.tipocaja = mod_vh.tipocaja;
                    modeloActual.seg_vh_id = mod_vh.seg_vh_id;
                    modeloActual.idequipamiento = mod_vh.idequipamiento;

                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();

                    ConsultaDatosCreacion(modeloActual);
                    TempData["mensaje"] = "La actualización del modelo fue exitoso!";

                    ModeloVehiculosModel modVh = new ModeloVehiculosModel();
                    listasDesplegables(mod_vh);
                    ParametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View(mod_vh);
                }

                {
                    int nom2 = (from a in context.modelo_vehiculo
                                where a.modvh_nombre == mod_vh.modvh_nombre
                                select a.modvh_nombre).Count();
                    if (nom2 == 0)
                    {
                        modelo_vehiculo modeloActual =
                            context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == mod_vh.modvh_codigo);
                        modeloActual.modvhfec_actualizacion = DateTime.Now;
                        modeloActual.modvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        modeloActual.capacidad = mod_vh.capacidad;
                        modeloActual.grupo = mod_vh.grupo_id;
                        modeloActual.tipo = mod_vh.tipo_id;
                        modeloActual.clase = mod_vh.clase_id;
                        modeloActual.cilindraje = mod_vh.cilindraje;
                        modeloActual.combustible = mod_vh.combustible_id;
                        modeloActual.perfil = mod_vh.perfil_id;
                        modeloActual.modvh_nombre = mod_vh.modvh_nombre;
                        modeloActual.unidadcarga = mod_vh.unidadcarga;
                        modeloActual.diaslibrescaplan = mod_vh.diaslibrescaplan;
                        modeloActual.diaslibresgmac = mod_vh.diaslibresgmac;
                        modeloActual.modelogkit = mod_vh.modelogkit;
                        modeloActual.seg_vh_id = mod_vh.seg_vh_id;
                        modeloActual.idequipamiento = mod_vh.idequipamiento;

                        context.Entry(modeloActual).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(modeloActual);
                        TempData["mensaje"] = "La actualización del modelo fue exitoso!";


                        ParametrosBusqueda();
                        listasDesplegables(mod_vh);
                        System.Collections.Generic.List<codigo_iva> ivasm = context.codigo_iva.ToList();
                        var ivas2p = ivasm.Select(x => new
                        {
                            id = x.porcentaje,
                            nombre = x.Descripcion + " " + x.porcentaje
                        }).OrderBy(x => x.nombre).ToList();
                        ViewBag.porcentaje_iva = new SelectList(ivas2p, "id", "nombre");

                        BuscarFavoritos(menu);
                        return View(mod_vh);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }
            else
            {
                System.Collections.Generic.List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                TempData["mensaje_error"] = "Error en el modelo, por favor valide!";
            }

            listasDesplegables(mod_vh);
            modelo_vehiculo modeloActualAux = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == mod_vh.modvh_codigo);
            System.Collections.Generic.List<codigo_iva> ivas = context.codigo_iva.ToList();
            var ivas2 = ivas.Select(x => new
            {
                id = x.porcentaje,
                nombre = x.Descripcion + " " + x.porcentaje
            }).OrderBy(x => x.nombre).ToList();
            ViewBag.porcentaje_iva = new SelectList(ivas2, "id", "nombre");

            ConsultaDatosCreacion(modeloActualAux);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View(mod_vh);
        }

        public void listasDesplegables(ModeloVehiculosModel modelo)
        {
            ViewBag.modelogkit = new SelectList(context.vmodelog, "id", "Descripcion");
            ViewBag.mar_vh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            ViewBag.seg_vh_id = new SelectList(context.segmento_vehiculo, "segvh_id", "segvh_nombre");
            ViewBag.grupo_id = new SelectList(context.vgrupo, "grupo_id", "nombre");
            ViewBag.clase_id = new SelectList(context.vclase, "clase_id", "nombre");
            ViewBag.tipo_id = new SelectList(context.vtipo, "tipo_id", "nombre");
            ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo, "tpmot_id", "tpmot_nombre");
            ViewBag.perfil_id = new SelectList(context.vperfil, "perfil_id", "nombre");
            ViewBag.unidadcarga = new SelectList(context.vunidadcarga, "id", "unidad");
            ViewBag.tipocaja = new SelectList(context.tpcaja_vehiculo, "tpcaj_id", "tpcaj_nombre");
            ViewBag.clasificacion = new SelectList(context.clasificacion_vehiculo.OrderBy(x => x.clavh_nombre),
                "clavh_id", "clavh_nombre");
            ViewBag.porcentaje_impoconsumo =
                new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje",
                    modelo.id_impoconsumo);
            ViewBag.porcentaje_iva = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id",
                "porcentaje", modelo.id_porcentaje_iva);
            ViewBag.porcentaje_compra = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"), "id",
                "porcentaje", modelo.id_porcentaje_compra);
            ViewBag.idequipamiento =
                new SelectList(context.vequipamiento.Where(x => x.estado).OrderByDescending(x => x.codigo), "id",
                    "codigo", modelo.idequipamiento);

            ViewBag.porcentaje_iva_modal = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Venta"), "id",
                "porcentaje", modelo.anio_modelo);
            ViewBag.porcentaje_compra_modal = new SelectList(context.codigo_iva.Where(x => x.Descripcion == "Compra"),
                "id", "porcentaje", modelo.anio_modelo);
            ViewBag.porcentaje_impoconsumo_modal = new SelectList(
                context.codigo_iva.Where(x => x.Descripcion == "Impoconsumo"), "id", "porcentaje", modelo.anio_modelo);
        }

        public void ConsultaDatosCreacion(modelo_vehiculo mod_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(mod_vh.modvhuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(mod_vh.modvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarModelosPaginados()
        {
            var data = (from modelo in context.modelo_vehiculo
                        join marca in context.marca_vehiculo
                            on modelo.mar_vh_id equals marca.marcvh_id
                        join segmento in context.segmento_vehiculo
                            on modelo.seg_vh_id equals segmento.segvh_id into ps
                        from segmento in ps.DefaultIfEmpty()
                        select new
                        {
                            modelo.modvh_codigo,
                            modelo.modvh_nombre,
                            modvh_estado = modelo.modvh_estado ? "Activo" : "Inactivo",
                            marca.marcvh_nombre,
                            segvh_nombre = segmento.segvh_nombre ?? ""
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarAnioModelo(string modelo, int anioN, decimal valor /*, string descripcion*/, int iva,
            int? impuestoConsumo, int? compra)
        {
            anio_modelo buscaAnioModelo = context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == modelo && x.anio == anioN);

            bool result = false;
            if (buscaAnioModelo == null)
            {
                modelo_vehiculo buscarModelo = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == modelo);
                codigo_iva porcentajeCompra = context.codigo_iva.FirstOrDefault(y => y.id == compra);
                codigo_iva porcentajeVenta = context.codigo_iva.FirstOrDefault(y => y.id == iva);
                codigo_iva porcentajeImpo = context.codigo_iva.FirstOrDefault(y => y.id == impuestoConsumo);
                anio_modelo modelox = new anio_modelo
                {
                    anio = anioN,
                    codigo_modelo = modelo,
                    descripcion = buscarModelo != null ? buscarModelo.modvh_nombre : "",
                    valor = valor,
                    idporcentajecompra = compra ?? 0,
                    idporcentajeiva = iva,
                    idporcentajeimpoconsumo = impuestoConsumo ?? 0,
                    porcentaje_compra = Convert.ToDecimal(porcentajeCompra.porcentaje),
                    porcentaje_iva = Convert.ToDecimal(porcentajeVenta.porcentaje),
                    impuesto_consumo = Convert.ToDecimal(porcentajeImpo.porcentaje)
                };
                context.anio_modelo.Add(modelox);
                int result2 = context.SaveChanges();
                if (result2 > 0)
                {
                    result = true;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarAnioModelo(int? idAnioModelo, int anio, string descripcion, decimal valor,
            decimal? iva, decimal? impuestoConsumo, decimal? compra, int? idiva, int? idimpuestoConsumo, int? idcompra)
        {
            anio_modelo buscaAnioModelo = context.anio_modelo.FirstOrDefault(x => x.anio_modelo_id == idAnioModelo);
            anio_modelo buscarAnioRepetido = context.anio_modelo.FirstOrDefault(x =>
                x.codigo_modelo == buscaAnioModelo.codigo_modelo && x.anio == anio && x.anio_modelo_id != idAnioModelo);
            bool result = false;
            if (buscarAnioRepetido == null)
            {
                if (buscaAnioModelo != null)
                {
                    buscaAnioModelo.anio = anio;
                    buscaAnioModelo.descripcion = descripcion;
                    buscaAnioModelo.valor = valor;

                    buscaAnioModelo.porcentaje_iva = iva ?? 0;
                    buscaAnioModelo.impuesto_consumo = impuestoConsumo ?? 0;
                    buscaAnioModelo.porcentaje_compra = compra ?? 0;

                    buscaAnioModelo.idporcentajeiva = idiva ?? 0;
                    buscaAnioModelo.idporcentajeimpoconsumo = idimpuestoConsumo ?? 0;
                    buscaAnioModelo.idporcentajecompra = idcompra ?? 0;
                    context.Entry(buscaAnioModelo).State = EntityState.Modified;
                    result = context.SaveChanges() > 0;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerDetallesAnioModelo(int anioModeloId)
        {
            anio_modelo anioModelo = context.anio_modelo.Where(x => x.anio_modelo_id == anioModeloId).FirstOrDefault();
            var result = new
            {
                anioModelo.anio_modelo_id,
                anioModelo.anio,
                anioModelo.descripcion,
                anioModelo.valor,

                porcentaje_iva = anioModelo.idporcentajeiva,
                porcentaje_compra = anioModelo.idporcentajecompra,
                impoconsumo = anioModelo.idporcentajeimpoconsumo
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult actualizarTablaAnios(string codigoModelo)
        {
            System.Collections.Generic.List<anio_modelo> anios = context.anio_modelo.Where(x => x.codigo_modelo == codigoModelo).ToList();
            var result = anios.Select(x => new
            {
                x.anio_modelo_id,
                x.anio,
                x.descripcion,
                x.valor,
                x.porcentaje_iva,
                x.impuesto_consumo,
                x.porcentaje_compra
            });
            return Json(result, JsonRequestBehavior.AllowGet);
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
    }
}