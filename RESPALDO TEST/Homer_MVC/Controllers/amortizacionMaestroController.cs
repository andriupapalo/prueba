using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class amortizacionMaestroController : Controller
    {
        // GET: maestroActivos
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        public void listas()
        {
            var centros = from u in context.centro_costo
                          select new
                          {
                              u.centcst_id,
                              u.pre_centcst,
                              u.centcst_nombre
                          };
            List<SelectListItem> itemsCen = new List<SelectListItem>();
            foreach (var item in centros)
            {
                string nombre = "(" + item.pre_centcst + ") " + item.centcst_nombre;
                itemsCen.Add(new SelectListItem { Text = nombre, Value = item.centcst_id.ToString() });
            }

            ViewBag.idcentro = itemsCen;


            var ubicaciones = from u in context.amortizacionfubicacion
                              select new
                              {
                                  u.id,
                                  u.descripcion
                              };
            List<SelectListItem> itemsubi = new List<SelectListItem>();
            foreach (var item in ubicaciones)
            {
                string nombre = item.descripcion;
                itemsubi.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }

            ViewBag.ubicacion = itemsubi;

            var empresas = from u in context.tablaempresa
                           select new
                           {
                               u.id,
                               u.nombre_empresa
                           };
            List<SelectListItem> itemsemp = new List<SelectListItem>();
            foreach (var item in empresas)
            {
                string nombre = item.nombre_empresa;
                itemsemp.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }

            ViewBag.idempresa = itemsemp;

            var users = from u in context.users
                        select new
                        {
                            idTercero = u.user_id,
                            nombre = u.user_nombre,
                            apellidos = u.user_apellido,
                            u.user_numIdent
                        };
            List<SelectListItem> itemsU = new List<SelectListItem>();
            foreach (var item in users)
            {
                string nombre = "(" + item.user_numIdent + ") - " + item.nombre + " " + item.apellidos;
                itemsU.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.idresponsable = itemsU;

            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  pro.prtercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero,
                                  razonSocial = ter.razon_social,
                                  ter.doc_tercero
                              }).ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.doc_tercero + " - " + item.nombreTErcero + " " + item.apellidosTercero + " " +
                             item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.prtercero_id.ToString() });
            }

            ViewBag.idproveedor = items;

            var aseguradoras = from u in context.icb_aseguradoras
                               select new
                               {
                                   u.aseg_id,
                                   u.nombre
                               };
            List<SelectListItem> itemsase = new List<SelectListItem>();
            foreach (var item in aseguradoras)
            {
                string nombre = item.nombre;
                itemsase.Add(new SelectListItem { Text = nombre, Value = item.aseg_id.ToString() });
            }
            //ViewBag.idaseguradora = itemsase;

            var metodos = from u in context.activometodo
                          select new
                          {
                              u.id,
                              u.Descripcion
                          };
            List<SelectListItem> itemsmet = new List<SelectListItem>();
            foreach (var item in metodos)
            {
                string nombre = item.Descripcion;
                itemsmet.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }
            //ViewBag.metododepreciacion = itemsmet;
            //ViewBag.metododeprecniff = itemsmet;

            var clasificaciones = from u in context.amortizacionclasificacion
                                  select new
                                  {
                                      u.id,
                                      u.Descripcion
                                  };
            List<SelectListItem> itemscla = new List<SelectListItem>();
            foreach (var item in clasificaciones)
            {
                string nombre = item.Descripcion;
                itemscla.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }

            ViewBag.clasificacion = itemscla;
            ViewBag.clasificacionniff = itemscla;

            var bajas = from u in context.motivobajaactivo
                        select new
                        {
                            u.id,
                            u.Descripcion
                        };
            List<SelectListItem> itembaja = new List<SelectListItem>();
            foreach (var item in bajas)
            {
                string nombre = item.Descripcion;
                itembaja.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }

            //ViewBag.motivo = itembaja;
        }

        public ActionResult Create(amortizacionMaestroModel activosmaestromodelo)
        {
            listas();
            return View(activosmaestromodelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(amortizacionMaestroModel activosmaestromodelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                amortizacionfijos buscarDato = context.amortizacionfijos.FirstOrDefault(x => x.placa == activosmaestromodelo.placa);
                if (buscarDato == null)
                {
                    amortizacionfijos tablaMaestroActivos = new amortizacionfijos
                    {
                        descripcion = activosmaestromodelo.descripcion,
                        placa = activosmaestromodelo.placa,
                        idcentro = activosmaestromodelo.idcentro,
                        ubicacion = activosmaestromodelo.ubicacion,
                        idempresa = activosmaestromodelo.idempresa,
                        idresponsable = activosmaestromodelo.idresponsable,
                        idproveedor = activosmaestromodelo.idproveedor,
                        fecha_compra = Convert.ToDateTime(activosmaestromodelo.fecha_compra),
                        //tablaMaestroActivos.mantenimiento = activosmaestromodelo.mantenimiento;
                        numeroserial = activosmaestromodelo.numeroserial,
                        valoractivo = Convert.ToDecimal(activosmaestromodelo.valoractivo, miCultura)
                    };
                    //tablaMaestroActivos.vendido = activosmaestromodelo.vendido;
                    //tablaMaestroActivos.finalizado = activosmaestromodelo.finalizado;


                    //if (!String.IsNullOrEmpty(activosmaestromodelo.fecha_vencepoliza))
                    //{
                    //	tablaMaestroActivos.fecha_vencepoliza = Convert.ToDateTime(activosmaestromodelo.fecha_vencepoliza);
                    //}

                    //if (!String.IsNullOrEmpty(activosmaestromodelo.numeropoliza))
                    //{
                    //	tablaMaestroActivos.numeropoliza = activosmaestromodelo.numeropoliza;
                    //}

                    //if (activosmaestromodelo.idaseguradora > 0 && activosmaestromodelo.idaseguradora != null)
                    //{
                    //	tablaMaestroActivos.idaseguradora = activosmaestromodelo.idaseguradora;
                    //}

                    //tablaMaestroActivos.pignorado = activosmaestromodelo.pignorado;

                    //if (!String.IsNullOrEmpty(activosmaestromodelo.fecha_vencegarantia))
                    //{
                    //	tablaMaestroActivos.fecha_vencegarantia = Convert.ToDateTime(activosmaestromodelo.fecha_vencegarantia);
                    //}

                    //if (!String.IsNullOrEmpty(activosmaestromodelo.detalle_garantia))
                    //{
                    //	tablaMaestroActivos.detalle_garantia = activosmaestromodelo.detalle_garantia;
                    //}

                    if (!string.IsNullOrEmpty(activosmaestromodelo.orden_compra))
                    {
                        tablaMaestroActivos.orden_compra = activosmaestromodelo.orden_compra;
                    }

                    tablaMaestroActivos.activo = activosmaestromodelo.activo;

                    tablaMaestroActivos.depreciable = activosmaestromodelo.depreciable;
                    if (activosmaestromodelo.depreciable)
                    {
                        //tablaMaestroActivos.metododepreciacion = activosmaestromodelo.metododepreciacion ?? 0;
                        tablaMaestroActivos.fecha_activacion =
                            Convert.ToDateTime(activosmaestromodelo.fecha_activacion);
                        tablaMaestroActivos.meses_depreciacion = activosmaestromodelo.meses_depreciacion ?? 0;
                        tablaMaestroActivos.fecha_findepreciacion =
                            Convert.ToDateTime(activosmaestromodelo.fecha_findepreciacion);
                        tablaMaestroActivos.clasificacion = activosmaestromodelo.clasificacion ?? 0;
                        tablaMaestroActivos.constantedepre = Convert.ToDecimal(activosmaestromodelo.constantedepre, miCultura);
                        if (string.IsNullOrEmpty(activosmaestromodelo.fechaactualizacion))
                        {
                            tablaMaestroActivos.valorresidual = Convert.ToDecimal(activosmaestromodelo.valoractivo, miCultura);
                        }
                    }


                    tablaMaestroActivos.depreciableniif = activosmaestromodelo.depreciableniif;
                    if (activosmaestromodelo.depreciableniif)
                    {
                        //tablaMaestroActivos.metododeprecniff = activosmaestromodelo.metododeprecniff;
                        tablaMaestroActivos.fecha_activacionniif =
                            Convert.ToDateTime(activosmaestromodelo.fecha_activacionniif);
                        tablaMaestroActivos.meses_depreniff = activosmaestromodelo.meses_depreniff;
                        tablaMaestroActivos.fecha_findepreniff =
                            Convert.ToDateTime(activosmaestromodelo.fecha_findepreniff);
                        tablaMaestroActivos.clasificacionniff = activosmaestromodelo.clasificacionniff;
                        tablaMaestroActivos.constantedepreniif =
                            Convert.ToDecimal(activosmaestromodelo.constantedepreniif);
                        if (string.IsNullOrEmpty(activosmaestromodelo.fechaactualizacion))
                        {
                            tablaMaestroActivos.valorresidualniif = Convert.ToDecimal(activosmaestromodelo.valoractivo, miCultura);
                        }
                    }

                    tablaMaestroActivos.fec_creacion = DateTime.Now;
                    tablaMaestroActivos.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);

                    //***************************************************************************************************
                    //if (activosmaestromodelo.motivo > 0 && activosmaestromodelo.motivo != null)
                    //{
                    //	tablaMaestroActivos.motivo = activosmaestromodelo.motivo;
                    //}
                    //***************************************************************************************************

                    tablaMaestroActivos.estado = activosmaestromodelo.estado;
                    tablaMaestroActivos.razon_inactivo = activosmaestromodelo.razon_inactivo;
                    context.amortizacionfijos.Add(tablaMaestroActivos);
                    context.SaveChanges();

                    TempData["mensaje"] = "La creación de del Activo Fijo fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }
            else
            {
                //TempData["mensaje_error"] = "Errores en la creación del pedido, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listas();
            return View(activosmaestromodelo);
        }

        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            amortizacionfijos act_fij = context.amortizacionfijos.Find(id);
            if (act_fij == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(act_fij.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(act_fij.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            //	activosfijos anio_modelo = new activosfijos();
            amortizacionMaestroModel modelo = new amortizacionMaestroModel
            {
                id = act_fij.id,
                descripcion = act_fij.descripcion,
                placa = act_fij.placa,
                idcentro = act_fij.idcentro,
                ubicacion = act_fij.ubicacion ?? 0,
                idempresa = act_fij.idempresa,
                idresponsable = act_fij.idresponsable,
                idproveedor = act_fij.idproveedor,
                fecha_compra = act_fij.fecha_compra.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                numeroserial = act_fij.numeroserial,
                valoractivo = act_fij.valoractivo.ToString("N0"),

                //mantenimiento = act_fij.mantenimiento,
                //pignorado = act_fij.pignorado,
                //vendido = act_fij.vendido,
                //finalizado = act_fij.finalizado,

                //fecha_vencepoliza = act_fij.fecha_vencepoliza != null ? act_fij.fecha_vencepoliza.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                //numeropoliza = act_fij.numeropoliza != null ? act_fij.numeropoliza : "",
                //idaseguradora = act_fij.idaseguradora != null ? act_fij.idaseguradora ?? 0 : 0,

                //fecha_vencegarantia = act_fij.fecha_vencegarantia != null ? act_fij.fecha_vencegarantia.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                //detalle_garantia = act_fij.detalle_garantia != null ? act_fij.detalle_garantia : "",
                orden_compra = act_fij.orden_compra,
                activo = act_fij.activo,

                depreciable = act_fij.depreciable,
                //metododepreciacion = act_fij.metododepreciacion,
                fecha_activacion = act_fij.fecha_activacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                fecha_findepreciacion = act_fij.fecha_findepreciacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                meses_depreciacion = act_fij.meses_depreciacion,
                //mesesfaltantes = act_fij.mesesfaltantes != null ? act_fij.mesesfaltantes : 0,
                mesesfaltantes = act_fij.mesesfaltantes,
                constantedepre = act_fij.constantedepre != null ? act_fij.constantedepre.Value.ToString("N0") : "",
                //valordepre = act_fij.valordepre != null ? act_fij.valordepre.ToString("N0") : "",
                valordepre = act_fij.valordepre.ToString("N0"),
                valorresidual = act_fij.valorresidual != null ? act_fij.valorresidual.Value.ToString("N0") : "",
                clasificacion = act_fij.clasificacion,

                depreciableniif = act_fij.depreciableniif,
                //metododeprecniff = act_fij.metododeprecniff ?? 0,
                fecha_activacionniif =
                    act_fij.fecha_activacionniif.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                fecha_findepreniff = act_fij.fecha_findepreniff.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                meses_depreniff = act_fij.meses_depreniff,
                //mesesfaltantesniif = act_fij.mesesfaltantesniif != null ? act_fij.mesesfaltantesniif : 0,
                mesesfaltantesniif = act_fij.mesesfaltantesniif,
                constantedepreniif = act_fij.constantedepreniif != null
                    ? act_fij.constantedepreniif.Value.ToString("N0")
                    : "",
                //valordepreniif = act_fij.valordepreniif != null ? act_fij.valordepreniif.ToString("N0") : "",
                valordepreniif = act_fij.valordepreniif.ToString("N0"),
                valorresidualniif = act_fij.valorresidualniif != null
                    ? act_fij.valorresidualniif.Value.ToString("N0")
                    : "",
                clasificacionniff = act_fij.clasificacionniff,


                //motivo = act_fij.motivo != null ? act_fij.motivo ?? 0 : 0,
                estado = act_fij.estado,
                razon_inactivo = act_fij.razon_inactivo != null ? act_fij.razon_inactivo : "",
                fec_actualizacion = act_fij.fec_actualizacion != null
                    ? act_fij.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                user_idactualizacion = act_fij.user_idactualizacion != null ? act_fij.user_idactualizacion ?? 0 : 0

                //sipo = act_fij.idaseguradora != null ? true : false,
            };

            //if (modelo.idaseguradora == null || modelo.idaseguradora == 0)
            //{
            //	ViewBag.SipolizaViene = 0;
            //}
            //else
            //{
            //	ViewBag.SipolizaViene = 1;
            //}
            //BuscarFavoritos(menu);

            var centros = (from u in context.centro_costo
                           select new
                           {
                               u.centcst_id,
                               u.pre_centcst,
                               u.centcst_nombre,
                               nombre = "(" + u.pre_centcst + ") " + u.centcst_nombre
                           }).ToList();
            ViewBag.idcentro = new SelectList(centros, "centcst_id", "nombre", modelo.idcentro);

            var ubicaciones = (from u in context.amortizacionfubicacion
                               select new
                               {
                                   u.id,
                                   nombre = u.descripcion
                               }).ToList();
            ViewBag.ubicacion = new SelectList(ubicaciones, "id", "nombre", modelo.ubicacion);

            var empresas = (from u in context.tablaempresa
                            select new
                            {
                                u.id,
                                nombre = u.nombre_empresa
                            }).ToList();
            ViewBag.idempresa = new SelectList(empresas, "id", "nombre", modelo.idempresa);

            var users = (from u in context.users
                         select new
                         {
                             idTercero = u.user_id,
                             u.user_numIdent,
                             nombre = "(" + u.user_numIdent + ") - " + u.user_nombre + " " + u.user_apellido
                         }).ToList();
            ViewBag.idresponsable = new SelectList(users, "idTercero", "nombre", modelo.idresponsable);

            var proveedores = (from pro in context.tercero_proveedor
                               join ter in context.icb_terceros
                                   on pro.tercero_id equals ter.tercero_id
                               select new
                               {
                                   pro.prtercero_id,
                                   nombreTErcero = ter.prinom_tercero,
                                   apellidosTercero = ter.apellido_tercero,
                                   razonSocial = ter.razon_social,
                                   ter.doc_tercero,
                                   nombre = ter.doc_tercero + " - " + ter.prinom_tercero + " " + ter.apellido_tercero + " " +
                                            ter.razon_social
                               }).ToList();
            ViewBag.idproveedor = new SelectList(proveedores, "prtercero_id", "nombre", modelo.idproveedor);

            var aseguradoras = (from u in context.icb_aseguradoras
                                select new
                                {
                                    u.aseg_id,
                                    u.nombre
                                }).ToList();
            //ViewBag.idaseguradora = new SelectList(aseguradoras, "aseg_id", "nombre", modelo.idaseguradora);

            var metodos = (from u in context.activometodo
                           select new
                           {
                               u.id,
                               nombre = u.Descripcion
                           }).ToList();
            //ViewBag.metododepreciacion = new SelectList(metodos, "id", "nombre", modelo.metododepreciacion);
            //ViewBag.metododeprecniff = new SelectList(metodos, "id", "nombre", modelo.metododeprecniff);

            var clasificaciones = (from u in context.amortizacionclasificacion
                                   select new
                                   {
                                       u.id,
                                       nombre = u.Descripcion
                                   }).ToList();

            ViewBag.clasificacion = new SelectList(clasificaciones, "id", "nombre", modelo.clasificacion);
            ViewBag.clasificacionniff = new SelectList(clasificaciones, "id", "nombre", modelo.clasificacionniff);

            var bajas = (from u in context.motivobajaactivo
                         select new
                         {
                             u.id,
                             nombre = u.Descripcion
                         }).ToList();

            //ViewBag.motivo = new SelectList(bajas, "id", "nombre", modelo.motivo);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(amortizacionMaestroModel activosmaestromodelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.amortizacionfijos
                           where a.placa == activosmaestromodelo.placa && a.id == activosmaestromodelo.id
                           select a.placa).Count();

                if (nom == 1)
                {
                    //var modeloActual = context.activosfijos.FirstOrDefault(x => x.descripcion == activosmaestromodelo.descripcion);
                    amortizacionfijos modeloActual =
                        context.amortizacionfijos.FirstOrDefault(x => x.placa == activosmaestromodelo.placa);
                    modeloActual.descripcion = activosmaestromodelo.descripcion;
                    modeloActual.idcentro = activosmaestromodelo.idcentro;
                    modeloActual.ubicacion = activosmaestromodelo.ubicacion;
                    modeloActual.idempresa = activosmaestromodelo.idempresa;
                    modeloActual.idresponsable = activosmaestromodelo.idresponsable;
                    modeloActual.idproveedor = activosmaestromodelo.idproveedor;
                    //modeloActual.fecha_compra = Convert.ToDateTime(activosmaestromodelo.fecha_compra);
                    //modeloActual.orden_compra = activosmaestromodelo.orden_compra;
                    modeloActual.numeroserial = activosmaestromodelo.numeroserial;

                    //modeloActual.mantenimiento = activosmaestromodelo.mantenimiento;
                    //modeloActual.pignorado = activosmaestromodelo.pignorado;
                    //modeloActual.vendido = activosmaestromodelo.vendido;
                    //modeloActual.finalizado = activosmaestromodelo.finalizado;
                    //if (modeloActual.motivo != null)
                    //{
                    //	modeloActual.motivo = activosmaestromodelo.motivo;
                    //}
                    //modeloActual.motivo = activosmaestromodelo.motivo != null ? activosmaestromodelo.motivo:0;

                    if (activosmaestromodelo.sipo)
                    {
                        //modeloActual.fecha_vencepoliza = Convert.ToDateTime(activosmaestromodelo.fecha_vencepoliza);
                        //modeloActual.numeropoliza = activosmaestromodelo.numeropoliza;
                        //modeloActual.idaseguradora = activosmaestromodelo.idaseguradora;
                    }


                    //modeloActual.fecha_vencegarantia = Convert.ToDateTime(activosmaestromodelo.fecha_vencegarantia);
                    //modeloActual.detalle_garantia = activosmaestromodelo.detalle_garantia;

                    modeloActual.activo = activosmaestromodelo.activo;
                    //modeloActual.valoractivo = Convert.ToDecimal(activosmaestromodelo.valoractivo);

                    modeloActual.depreciable = activosmaestromodelo.depreciable;
                    if (activosmaestromodelo.depreciable)
                    {
                        //modeloActual.metododepreciacion = activosmaestromodelo.metododepreciacion ?? 0;
                        //modeloActual.fecha_activacion = Convert.ToDateTime(activosmaestromodelo.fecha_activacion);
                        //modeloActual.meses_depreciacion = activosmaestromodelo.meses_depreciacion??0;
                        //modeloActual.fecha_findepreciacion = Convert.ToDateTime(activosmaestromodelo.fecha_vencepoliza);
                        //modeloActual.constantedepre = Convert.ToDecimal(activosmaestromodelo.constantedepre);
                        modeloActual.clasificacion = activosmaestromodelo.clasificacion ?? 0;
                    }
                    //modeloActual.valorresidual = Convert.ToDecimal(activosmaestromodelo.valorresidual);
                    modeloActual.depreciableniif = activosmaestromodelo.depreciableniif;
                    if (activosmaestromodelo.depreciableniif)
                    {
                        //modeloActual.metododeprecniff = activosmaestromodelo.metododeprecniff;
                        //modeloActual.fecha_activacionniif = Convert.ToDateTime(activosmaestromodelo.fecha_activacionniif);
                        //modeloActual.meses_depreniff = activosmaestromodelo.meses_depreniff;
                        //modeloActual.fecha_findepreniff = Convert.ToDateTime(activosmaestromodelo.fecha_findepreniff);
                        //modeloActual.constantedepreniif = Convert.ToDecimal(activosmaestromodelo.constantedepreniif);
                        modeloActual.clasificacionniff = activosmaestromodelo.clasificacionniff;
                    }
                    //modeloActual.valorresidualniif = Convert.ToDecimal(activosmaestromodelo.valorresidualniif);

                    modeloActual.estado = activosmaestromodelo.estado;
                    modeloActual.razon_inactivo = activosmaestromodelo.razon_inactivo;
                    modeloActual.fec_actualizacion = DateTime.Now;
                    modeloActual.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del Activo Fijo fue exitoso!";
                    //	ConsultaDatosCreacion(modeloActual);
                    //return RedirectToAction("Create");
                    //	return View(activosmaestromodelo);
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                }
            }

            //var modeloAux = context.activosfijos.FirstOrDefault(x => x.descripcion == activosmaestromodelo.descripcion);
            amortizacionfijos modeloAux = context.amortizacionfijos.FirstOrDefault(x => x.placa == activosmaestromodelo.placa);
            ConsultaDatosCreacion(modeloAux);
            var centros = (from u in context.centro_costo
                           select new
                           {
                               u.centcst_id,
                               u.pre_centcst,
                               u.centcst_nombre,
                               nombre = "(" + u.pre_centcst + ") " + u.centcst_nombre
                           }).ToList();
            ViewBag.idcentro = new SelectList(centros, "centcst_id", "nombre", activosmaestromodelo.idcentro);

            var ubicaciones = (from u in context.amortizacionfubicacion
                               select new
                               {
                                   u.id,
                                   nombre = u.descripcion
                               }).ToList();
            ViewBag.ubicacion = new SelectList(ubicaciones, "id", "nombre", activosmaestromodelo.ubicacion);

            var empresas = (from u in context.tablaempresa
                            select new
                            {
                                u.id,
                                nombre = u.nombre_empresa
                            }).ToList();
            ViewBag.idempresa = new SelectList(empresas, "id", "nombre", activosmaestromodelo.idempresa);

            var users = (from u in context.users
                         select new
                         {
                             idTercero = u.user_id,
                             u.user_numIdent,
                             nombre = "(" + u.user_numIdent + ") - " + u.user_nombre + " " + u.user_apellido
                         }).ToList();
            ViewBag.idresponsable = new SelectList(users, "idTercero", "nombre", activosmaestromodelo.idresponsable);

            var proveedores = (from pro in context.tercero_proveedor
                               join ter in context.icb_terceros
                                   on pro.tercero_id equals ter.tercero_id
                               select new
                               {
                                   pro.prtercero_id,
                                   nombreTErcero = ter.prinom_tercero,
                                   apellidosTercero = ter.apellido_tercero,
                                   razonSocial = ter.razon_social,
                                   ter.doc_tercero,
                                   nombre = ter.doc_tercero + " - " + ter.prinom_tercero + " " + ter.apellido_tercero + " " +
                                            ter.razon_social
                               }).ToList();
            ViewBag.idproveedor =
                new SelectList(proveedores, "prtercero_id", "nombre", activosmaestromodelo.idproveedor);

            var aseguradoras = (from u in context.icb_aseguradoras
                                select new
                                {
                                    u.aseg_id,
                                    u.nombre
                                }).ToList();
            //ViewBag.idaseguradora = new SelectList(aseguradoras, "aseg_id", "nombre", activosmaestromodelo.idaseguradora);

            var metodos = (from u in context.activometodo
                           select new
                           {
                               u.id,
                               nombre = u.Descripcion
                           }).ToList();
            //ViewBag.metododepreciacion = new SelectList(metodos, "id", "nombre", activosmaestromodelo.metododepreciacion);
            //ViewBag.metododeprecniff = new SelectList(metodos, "id", "nombre", activosmaestromodelo.metododeprecniff);

            var clasificaciones = (from u in context.amortizacionclasificacion
                                   select new
                                   {
                                       u.id,
                                       nombre = u.Descripcion
                                   }).ToList();

            ViewBag.clasificacion = new SelectList(clasificaciones, "id", "nombre", activosmaestromodelo.clasificacion);
            ViewBag.clasificacionniff =
                new SelectList(clasificaciones, "id", "nombre", activosmaestromodelo.clasificacionniff);

            var bajas = (from u in context.motivobajaactivo
                         select new
                         {
                             u.id,
                             nombre = u.Descripcion
                         }).ToList();

            //ViewBag.motivo = new SelectList(bajas, "id", "nombre", activosmaestromodelo.motivo);
            return RedirectToAction("Create");
            //return View(activosmaestromodelo);
        }

        public void ConsultaDatosCreacion(amortizacionfijos activo)
        {
            if (activo != null)
            {
                //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
                users creator = context.users.Find(activo.userid_creacion);
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

                users modificator = context.users.Find(activo.user_idactualizacion);
                if (modificator != null)
                {
                    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }
        }


        public JsonResult BuscarActivosPaginados()
        {
            var buscarDepreciados = (from activos in context.amortizacionfijos
                                     join centros in context.centro_costo
                                         on activos.idcentro equals centros.centcst_id
                                     join ubica in context.amortizacionfubicacion
                                         on activos.ubicacion equals ubica.id
                                     // where activos.fechaactualizacion != null
                                     select new
                                     {
                                         activos.id,
                                         activos.descripcion,
                                         activos.placa,
                                         centro = "(" + centros.pre_centcst + ") " + centros.centcst_nombre,
                                         ubicacion = ubica.descripcion,
                                         activos.meses_depreciacion,
                                         activos.mesesfaltantes,
                                         activos.constantedepre,
                                         activos.valoractivo,
                                         activos.valorresidual,
                                         activos.fecha_compra,
                                         activos.fecha_activacion,
                                         activos.fechaactualizacion
                                     }).ToList();
            var data = buscarDepreciados.Select(c => new
            {
                c.id,
                c.descripcion,
                c.placa,
                c.centro,
                c.ubicacion,
                mesdep = c.meses_depreciacion.ToString("N0"),
                mesfal = c.mesesfaltantes.ToString("N0"),
                constante = c.constantedepre != null ? c.constantedepre.Value.ToString("N0") : "",
                valoractivo = c.valoractivo.ToString("N0"),
                valorresid = c.valorresidual != null ? c.valorresidual.Value.ToString("N0") : "",
                fecha = c.fechaactualizacion != null
                    ? c.fechaactualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                fechaC = c.fecha_compra != null ? c.fecha_compra.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                fechaA = c.fecha_activacion != null
                    ? c.fecha_activacion.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : ""
                //estadoUsuario = c.user_estado == true ? "Activo" : "Inactivo"
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}