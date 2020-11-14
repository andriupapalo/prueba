using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class v_creditosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        private bool CheckFileType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            switch (ext.ToLower())
            {
                case ".pdf":
                    return true;
                case ".doc":
                    return true;
                case ".docx":
                    return true;
                case ".odf":
                    return true;
                case ".jpg":
                    return true;
                case ".gif":
                    return true;
                case ".png":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        public JsonResult Getfinanciera(int plan_id) 
        {

            var bodegaid = Convert.ToInt32(Session["user_bodega"]);
            var planfinancierobodega = db.planfinancierobodega.FirstOrDefault(x => x.idplanfinanciero == plan_id && x.idbodega == bodegaid);
            if (planfinancierobodega != null)
            {
                var icb_plan_financiero = db.icb_plan_financiero.FirstOrDefault(x => x.plan_id== plan_id);
                var icb_unidad_financiera = db.icb_unidad_financiera.Where(x => x.financiera_id == icb_plan_financiero.idfinanciera).Select(x=>new {id=x.financiera_id,nombre=x.financiera_nombre }).ToList();

                return Json(icb_unidad_financiera, JsonRequestBehavior.AllowGet);
            }

            //ViewBag.financiera_id = new SelectList(db.icb_unidad_financiera, "financiera_id", "financiera_nombre", 8);
            //var datos= db.icb_unidad_financiera.FirstOrDefault(x => x.financiera_id == plan_id);
            //ViewBag.idmotcomision = new SelectList(db.vmotivocomision.Where(x => x.estado).OrderBy(x => x.motivo), "id", "motivo");


            return Json(null, JsonRequestBehavior.AllowGet);
        } 
        
        //= new SelectList(, "financiera_id", "financiera_nombre", v_creditos.financiera_id);
        public void listas(v_creditosform v_creditos)
        {
            int tercero = 0;
            int tercero2 = 0;
            vinfcredito infocredito = db.vinfcredito.FirstOrDefault(x => x.id == v_creditos.infocredito_id);
            if (infocredito != null)
            {
                ViewBag.salario = infocredito.ingsalario;
                ViewBag.comisiones = infocredito.ingcomision;
                ViewBag.arriendo = infocredito.ingarriendo;
                ViewBag.otros = infocredito.ingotros;
                ViewBag.prestamo = infocredito.egrprestamo;
                ViewBag.tarjetas = infocredito.egrtarjeta;
                ViewBag.hipotecas = infocredito.egrhipoteca;
                ViewBag.sostenimiento = infocredito.egrsostenimiento;
                tercero = infocredito.tercero;
                tercero2 = infocredito.tercero2 ?? 0;
            }

            var asesores = from u in db.users
                           where u.rol_id == 4
                           select new
                           {
                               nombre = u.user_nombre + " " + u.user_apellido,
                               u.user_id
                           };

            List<SelectListItem> listAsesores = new List<SelectListItem>();
            foreach (var item in asesores)
            {
                listAsesores.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.user_id.ToString()
                });
            }

            var cliente = (from t in db.icb_terceros
                               //join tp in db.tercero_cliente
                               //on t.tercero_id equals tp.tercero_id //se quita el join con cliente el dia 26/02/2019 a peticion del ing. Jairo que solicita que se muestren todos los terceros y no solo clientes
                           join dir in db.vw_dirtercero
                               on t.tercero_id equals dir.idtercero into dtt //into cppcp
                           from dir in dtt.DefaultIfEmpty()
                           select new
                           {
                               nombre = t.prinom_tercero != null
                                   ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                     " " + t.segapellido_tercero
                                   : t.doc_tercero + " " + t.razon_social,
                               t.tercero_id,
                               t.email_tercero,
                               t.telf_tercero,
                               t.celular_tercero,
                               dir.direccion
                           }).ToList();

            List<SelectListItem> lista_cliente = new List<SelectListItem>();
            foreach (var item in cliente)
            {
                lista_cliente.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == tercero ? true : false
                });
            }

            List<SelectListItem> lista_cliente2 = new List<SelectListItem>();
            foreach (var item in cliente)
            {
                lista_cliente2.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == tercero2 ? true : false
                });
            }

            ViewBag.asesor_id = listAsesores;
            ViewBag.tercero = lista_cliente;
            ViewBag.tercero2 = lista_cliente2;

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            // ViewBag.bodega = bodega;
            //var planesdeestabodega = (from p in db.icb_plan_financiero
            //                          join b in db.planfinancierobodega
            //                          on p.plan_id equals b.idplanfinanciero
            //                          where b.idbodega == bodegaActual
            //                          select new
            //                          {
            //                              nombre = p.plan_nombre,
            //                              p.plan_id
            //                          }).ToList();
            //ViewBag.plan_id = new SelectList(planesdeestabodega, "plan_id", "nombre");

            ViewBag.plan_id = new SelectList(db.icb_plan_financiero, "plan_id", "plan_nombre",v_creditos.plan_id);
            ViewBag.estadoc = new SelectList(db.estados_credito, "codigo", "descripcion", v_creditos.estadoc);
            ViewBag.financiera_id = new SelectList(db.icb_unidad_financiera, "financiera_id", "financiera_nombre",v_creditos.financiera_id);

            //var pedidos = (from ped in db.vpedido
            //                  select new
            //                  {
            //                      id = ped.id,
            //                      numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
            //                  }).ToList();
            //var pedidosold = (from ped in db.vpedido
            //               select new
            //               {
            //                   id = ped.id,
            //                   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
            //               }).ToList();

            List<int?> pedidosotro = (from cc in db.v_creditos
                                      join ec2 in db.estados_credito
                                          on cc.estadoc equals ec2.codigo //into cppcp
                                                                          //  from ec2 in cppcp.DefaultIfEmpty()
                                      where cc.estadoc == "C" && cc.pedido != null
                                      select cc.pedido).ToList();

            var pedidos = (from ped in db.vpedido
                           join ped_pg in db.vpedpago
                               on ped.id equals ped_pg.idpedido
                           where ped_pg.condicion == 1 && !pedidosotro.Contains(ped.id)
                           select new
                           {
                               ped.id,
                               numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null
                                            ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " +
                                              ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " +
                                              ped.icb_terceros.segapellido_tercero
                                            : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                               ped_pg.valor
                           }).OrderBy(x => x.id).ToList();
            pedidos.OrderByDescending(d => d.numero);

            ViewBag.pedido = new SelectList(pedidos, "id", "numero", v_creditos.pedido);
            ViewBag.motivodesiste = new SelectList(db.vcredmotdesistio, "id", "motivo", v_creditos.motivodesiste);
            ViewBag.vehiculo = new SelectList(db.modelo_vehiculo, "modvh_codigo", "modvh_nombre", v_creditos.vehiculo2);
            ViewBag.idmotcomision = new SelectList(db.vmotivocomision.Where(x => x.estado).OrderBy(x => x.motivo), "id",
                "motivo");
        }



        public JsonResult BuscarPlanXFinanciera(int finan)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int Financiera = finan;
            var data = (from p in db.icb_plan_financiero
                        join b in db.planfinancierobodega
                            on p.plan_id equals b.idplanfinanciero
                        where p.idfinanciera == Financiera && b.idbodega == bodegaActual
                        select new
                        {
                            nombre = p.plan_nombre,
                            p.plan_id
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DatosCliente(int idtercero)
        {
            var data = (from t in db.icb_terceros
                        join tp in db.tercero_cliente
                            on t.tercero_id equals tp.tercero_id
                        join dir in db.vw_dirtercero
                            on t.tercero_id equals dir.idtercero into dtt
                        from dir in dtt.DefaultIfEmpty()
                        where t.tercero_id == idtercero
                        select new
                        {
                            nombre = t.prinom_tercero != null
                                ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                  " " + t.segapellido_tercero
                                : t.doc_tercero + t.razon_social,
                            t.tercero_id,
                            t.email_tercero,
                            telefono = t.prinom_tercero != null ? t.celular_tercero : t.telf_tercero,
                            dir.direccion
                        }).FirstOrDefault();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFinanciera(int? idPlan)
        {
            if (idPlan != null)
            {
                var data = (from pb in db.icb_plan_financiero
                            join uf in db.icb_unidad_financiera
                                on pb.idfinanciera equals uf.financiera_id into dtt
                            from uf in dtt.DefaultIfEmpty()
                            where pb.plan_id == idPlan
                            select new
                            {
                                uf.financiera_id,
                                uf.financiera_nombre
                            }).ToList();
                ViewBag.financiera_id = new SelectList(data, "financiera_id", "financiera_nombre");

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        // GET: v_creditos/Create
        public ActionResult Create(int? menu)
        {
            v_creditosform vcreditos = new v_creditosform();

            listas(vcreditos);

            object bodega = Session["user_bodega"];
            ViewBag.bodega = bodega;
            
            BuscarFavoritos(menu);
            return View(vcreditos);
        }

        // POST: v_creditos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(v_creditosform v_creditos, int? menu)
        {
            if (ModelState.IsValid)
            {
                v_creditos buscarPedido = db.v_creditos.FirstOrDefault(x =>
                    x.pedido == v_creditos.pedido && x.financiera_id == v_creditos.financiera_id &&
                    v_creditos.pedido != null);
                if (buscarPedido == null)
                {
                    string salario = Request["salario"];
                    string comisiones = Request["comisiones"];
                    string arriendo = Request["arriendo"];
                    string otros = Request["otros"];
                    string prestamo = Request["prestamo"];
                    string tarjetas = Request["tarjetas"];
                    string hipotecas = Request["hipotecas"];
                    string sostenimiento = Request["sostenimiento"];
                    int? idmotcomision = 0;
                    if (Request["idmotcomision"] != "")
                    {
                        idmotcomision = Convert.ToInt32(Request["idmotcomision"]);
                    }
                    else
                    {
                        idmotcomision = null;
                    }

                    vinfcredito infocredito = new vinfcredito();
                    if (!string.IsNullOrEmpty(hipotecas))
                    {
                        infocredito.egrhipoteca = Convert.ToDecimal(hipotecas, new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(prestamo))
                    {
                        infocredito.egrprestamo = Convert.ToDecimal(prestamo,new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(salario))
                    {
                        infocredito.ingsalario = salario;
                    }

                    if (!string.IsNullOrEmpty(comisiones))
                    {
                        infocredito.ingcomision = Convert.ToDecimal(comisiones, new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(arriendo))
                    {
                        infocredito.ingarriendo = Convert.ToDecimal(arriendo, new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(otros))
                    {
                        infocredito.ingotros = Convert.ToDecimal(otros, new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(tarjetas))
                    {
                        infocredito.egrtarjeta = Convert.ToDecimal(tarjetas, new CultureInfo("is-IS"));
                    }

                    if (!string.IsNullOrEmpty(sostenimiento))
                    {
                        infocredito.egrsostenimiento = Convert.ToDecimal(sostenimiento, new CultureInfo("is-IS"));
                    }

                    infocredito.fec_creacion = DateTime.Now;
                    infocredito.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    if (!string.IsNullOrEmpty(Request["idtercero"]))
                    {
                        infocredito.tercero = Convert.ToInt32(Request["idtercero"]);
                    }

                    if (!string.IsNullOrEmpty(Request["tercero2"]))
                    {
                        infocredito.tercero2 = Convert.ToInt32(Request["tercero2"]);
                    }

                    db.vinfcredito.Add(infocredito);
                    db.SaveChanges();

                    int infocredito_id = db.vinfcredito.OrderByDescending(x => x.fec_creacion).FirstOrDefault().id;
                    v_creditos.infocredito_id = infocredito_id;
                    v_creditos.fec_creacion = Convert.ToString(DateTime.Now);
                    v_creditos.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    v_creditos.bodegaid = Convert.ToInt32(Session["user_bodega"]);
                    v_creditos.vehiculo = Request["idvehiculo"];
                    v_creditos credito = new v_creditos
                    {
                        asesor_id = v_creditos.asesor_id,
                        bodegaid = v_creditos.bodegaid,
                        comison = v_creditos.comison,
                        concesionarioid = v_creditos.concesionarioid,
                        cuota_inicial = string.IsNullOrWhiteSpace(v_creditos.cuota_inicial)
                            ? 0
                            : Convert.ToDecimal(v_creditos.cuota_inicial, new CultureInfo("is-IS")),
                        detalle = v_creditos.detalle,
                        estado = true,
                        estadoc = v_creditos.estadoc,
                        //fec_actualizacion = !string.IsNullOrWhiteSpace(v_creditos.fec_actualizacion)?Convert.ToDateTime(v_creditos.fec_actualizacion) : null,
                        //fec_aprobacion = v_creditos.fec_aprobacion != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
                        //fec_confirmacion = v_creditos.fec_confirmacion != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
                        fec_creacion = !string.IsNullOrWhiteSpace(v_creditos.fec_creacion)
                            ? Convert.ToDateTime(v_creditos.fec_creacion)
                            : DateTime.Now,
                        //v_creditos.fec_creacion != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
                        /*fec_desembolso = v_creditos.fec_desembolso != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
						fec_desistimiento = v_creditos.fec_desistimiento != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",*/
                        fec_entdocumentos = !string.IsNullOrWhiteSpace(v_creditos.fec_entdocumentos)
                            ? Convert.ToDateTime(v_creditos.fec_entdocumentos)
                            : DateTime.Now,
                        fec_envdocumentos = !string.IsNullOrWhiteSpace(v_creditos.fec_envdocumentos)
                            ? Convert.ToDateTime(v_creditos.fec_envdocumentos)
                            : DateTime.Now,
                        /*fec_facturacomision = v_creditos.fec_facturacomision != null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
						fec_negacion = v_creditos.fec_negacion,*/
                        fec_solicitud = !string.IsNullOrWhiteSpace(v_creditos.fec_solicitud)
                            ? Convert.ToDateTime(v_creditos.fec_solicitud)
                            : DateTime.Now,
                        financiera_id = v_creditos.financiera_id,
                        id_licencia = v_creditos.id_licencia,
                        infocredito_id = v_creditos.infocredito_id,
                        motivodesiste = v_creditos.motivodesiste,
                        numfactura = v_creditos.numfactura,
                        num_aprobacion = v_creditos.num_aprobacion,
                        pedido = v_creditos.pedido,
                        plan_id = v_creditos.plan_id,
                        plazo = v_creditos.plazo,
                        razon_inactivo = v_creditos.razon_inactivo,
                        toma_credito = v_creditos.toma_credito,
                        userid_creacion = v_creditos.userid_creacion,
                        user_idactualizacion = v_creditos.user_idactualizacion,
                        valor_comision = string.IsNullOrWhiteSpace(v_creditos.valor_comision)
                            ? 0
                            : Convert.ToDecimal(v_creditos.valor_comision, new CultureInfo("is-IS")),
                        vaprobado = string.IsNullOrWhiteSpace(v_creditos.vaprobado)
                            ? 0
                            : Convert.ToDecimal(v_creditos.vaprobado, new CultureInfo("is-IS")),
                        vehiculo = v_creditos.vehiculo,
                        vsolicitado = string.IsNullOrWhiteSpace(v_creditos.vsolicitado)
                            ? 0
                            : Convert.ToDecimal(v_creditos.vsolicitado, new CultureInfo("is-IS")),
                        poliza = v_creditos.poliza != null ? v_creditos.poliza : "",
                        idmotcomision = idmotcomision
                    };
                    db.v_creditos.Add(credito);

                    #region Seguimiento

                    string financiera = db.icb_unidad_financiera
                        .FirstOrDefault(x => x.financiera_id == v_creditos.financiera_id).financiera_nombre;
                    db.seguimientotercero.Add(new seguimientotercero
                    {
                        idtercero = infocredito.tercero,
                        tipo = 13,
                        nota = "Se genero solicitud de credito con la financiera: " + financiera,
                        fecha = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });

                    if (v_creditos.pedido != null)
                    {
                        int? cotizacion = db.vpedido.FirstOrDefault(x => x.id == v_creditos.pedido).idcotizacion;
                        if (cotizacion != null)
                        {
                            vcotseguimiento seguimiento = new vcotseguimiento
                            {
                                cot_id = Convert.ToInt32(cotizacion),
                                fecha = DateTime.Now,
                                responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                Notas = "Se genero solicitud de credito con la financiera " + financiera,
                                Motivo = null,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                estado = true,
                                tipo_seguimiento = 13
                            };
                            db.vcotseguimiento.Add(seguimiento);
                        }
                    }

                    #endregion

                    int guardar = db.SaveChanges();
                    int credito_id = db.v_creditos.OrderByDescending(x => x.fec_creacion).FirstOrDefault().Id;
                    int cantidadLineas = Convert.ToInt32(Request["listaReferencias"]);
                    for (int i = 0; i < cantidadLineas; i++)
                    {
                        refpersonales_cred referencia = new refpersonales_cred
                        {
                            credito_id = credito_id,
                            nombres = Request["nombreTabla" + i],
                            apellidos = Request["apellidosTabla" + i],
                            telefono = Request["telefonoTabla" + i],
                            celular = Request["celularTabla" + i],
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        };
                        db.refpersonales_cred.Add(referencia);
                        db.SaveChanges();
                    }

                    int cantidadDocumentos = db.vdocumentoscredito.ToList().Count;
                    List<vdocumentoscredito> documentos = db.vdocumentoscredito.ToList();
                    for (int j = 0; j < cantidadDocumentos; j++)
                    {
                        vdocrequeridos_credito documento = new vdocrequeridos_credito
                        {
                            credito_id = credito_id,
                            documento_id = documentos[j].id,
                            entregado = false,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        };
                        db.vdocrequeridos_credito.Add(documento);
                        db.SaveChanges();
                    }

                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "Registro Creado Correctamente";
                    }

                    listas(v_creditos);
                    BuscarFavoritos(menu);

                    return RedirectToAction("Create", new { id = credito.Id, menu });
                }

                TempData["mensaje_error"] =
                    "Ya existe una solicitud de credito con el pedido y la financiera seleccionados, por favor valide ";
                listas(v_creditos);
                BuscarFavoritos(menu);
                return View(v_creditos);
            }

            TempData["mensaje_error"] = "Error al registrar los datos ingresados, por favor valide";
            listas(v_creditos);
            BuscarFavoritos(menu);
            return View(v_creditos);
        }

        [HttpPost]
        public JsonResult subirArchivo()
        {
            string id = Request.Form["id"];
            string tipoconsulta = Request.Form["tipo"];
            System.Web.HttpPostedFileBase observacion = Request.Files["observacion"];
            string ruta = "";
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(tipoconsulta) && observacion.ContentLength > 0)
            {
                //busco la consulta
                bool conver = int.TryParse(id, out int idconsulta);
                bool conver2 = int.TryParse(tipoconsulta, out int tipocon);
                vdocrequeridos_credito consulta = db.vdocrequeridos_credito.Where(d => d.id == tipocon).FirstOrDefault();
                if (consulta != null)
                {
                    //busco el vehiculo_consultado
                    int vehiculox = consulta.id;
                    /*if (vehiculox != null)
                    {*/

                        //guardo el archivo
                        string archivo = observacion.FileName;
                        if (CheckFileType(observacion.FileName) == false)
                        {
                            return Json(3, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //guardo el archivo
                            ruta = "documentosCredito/" + idconsulta + "_" + tipocon + "_" + archivo;
                            string path = Server.MapPath("~/Content/documentosCredito/" + idconsulta + "_" + tipocon + "_" + archivo);
                            observacion.SaveAs(path);
                            //consulta
                            vdocrequeridos_credito docrequerido = db.vdocrequeridos_credito.Where(x => x.id == tipocon).FirstOrDefault();
                            docrequerido.ruta_documento = ruta;
                            docrequerido.entregado = true;
                            docrequerido.fecha = DateTime.Now;
                            docrequerido.fec_actualizacion = DateTime.Now;
                            docrequerido.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                            db.Entry(docrequerido).State = EntityState.Modified;

                            int guardado = db.SaveChanges();
                            return Json(1, JsonRequestBehavior.AllowGet);
                        }

                   /* }
                    else
                    {
                        return Json(0, JsonRequestBehavior.AllowGet);

                    }*/
                }
                else
                {
                    return Json(0, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult eliminarDoc(int? id)
        {
            int valor = 0;
            string respuesta = "";

            if (id != null)
            {
                //busco si el id en cuestion exist
                vdocrequeridos_credito existe = db.vdocrequeridos_credito.Where(d => d.id == id).FirstOrDefault();
                if (existe != null)
                {
                    //procedo a borrar el archivo
                    string path = Server.MapPath("~/Content/" + existe.ruta_documento);
                    // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    existe.ruta_documento = null;
                    existe.entregado = false;
                    db.Entry(existe).State = EntityState.Modified;
                    db.SaveChanges();
                    valor = 1;
                    respuesta = "Archivo eliminado satisfactoriamente";
                }
                else
                {
                    respuesta = "El identificador de archivo no está registrado en la base de datos";
                }
            }
            else
            {
                respuesta = "No se proporcionó un identificador de documento válido";
            }
            var data = new
            {
                valor,
                respuesta
            };
            return Json(data);
        }
        public JsonResult buscarArchivo(int id)
        {
            string docCredito = db.vdocrequeridos_credito.Where(x => x.id == id).FirstOrDefault().ruta_documento;
            return Json(docCredito, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BuscarDocumentos(int id)
        {
            List<vdocrequeridos_credito> documentos = db.vdocrequeridos_credito.Where(x => x.credito_id == id && x.entregado == true).ToList();

            var cargados = documentos.Select(x => new
            {
                x.id,
                nombre = x.vdocumentoscredito.nombre,
                fecha = x.fecha != null ? x.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
            });

            List<vdocrequeridos_credito> documentos2 = db.vdocrequeridos_credito.Where(x => x.credito_id == id && x.entregado == false).ToList();

            var sincargar = documentos2.Select(x => new
            {
                id = x.id,
                nombre = x.vdocumentoscredito.nombre,
            });

            var data2 = (from c in db.v_creditos
                         join info in db.vinfcredito
                             on c.infocredito_id equals info.id
                         join t in db.icb_terceros
                             on info.tercero equals t.tercero_id
                         join ec in db.estados_credito
                             on c.estadoc equals ec.codigo
                         where c.Id == id
                         select new
                         {
                             id = c.Id,
                             nombre = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + ") " +
                                      t.razon_social,
                             financiera = c.icb_unidad_financiera.financiera_nombre,
                             c.fec_solicitud,
                             estadoc = ec.descripcion,
                             pedido = c.pedido != null ? c.vpedido.numero.Value.ToString() : "",
                             iestadoc = c.estadoc,
                         }).ToList();
            List<bDatos> data = data2.Select(c => new bDatos
            {
                id = c.id,
                nombre = c.nombre,
                financiera = c.financiera,
                fec_solicitud = c.fec_solicitud != null
                    ? c.fec_solicitud.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                estadoc = c.estadoc,
                pedido = c.pedido,
            }).ToList();

            return Json(new { cargados, sincargar, info = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AgregarRefPer(int id, string nombres, string apellidos, string telefono, string celular)
        {

            refpersonales_cred referencia = new refpersonales_cred
            {
                credito_id = id,
                nombres = nombres,
                apellidos = apellidos,
                telefono = telefono,
                celular = celular,
                fec_creacion = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
            };
            db.refpersonales_cred.Add(referencia);
            int result = db.SaveChanges();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditarRefPer(int id, string nombres, string apellidos, string telefono, string celular)
        {
            refpersonales_cred referencia = db.refpersonales_cred.Where(x => x.id == id).FirstOrDefault();

            referencia.nombres = nombres;
            referencia.apellidos = apellidos;
            referencia.telefono = telefono;
            referencia.celular = celular;
            referencia.fec_actualizacion = DateTime.Now;
            referencia.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

            db.Entry(referencia).State = EntityState.Modified;
            int result = db.SaveChanges();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferenciasPer(int id)
        {
            List<refpersonales_cred> referenciasper = db.refpersonales_cred.Where(x => x.credito_id == id).ToList();

            var data = referenciasper.Select(x => new
            {
                x.id,
                x.nombres,
                x.apellidos,
                x.telefono,
                x.celular
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult modalReferencia(int id)
        {
            List<refpersonales_cred> referenciasper = db.refpersonales_cred.Where(x => x.id == id).ToList();

            var data = referenciasper.Select(x => new
            {
                x.id,
                x.nombres,
                x.apellidos,
                x.telefono,
                x.celular
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarReferenciaPer(int id)
        {
            int result = 0;
            refpersonales_cred referenciasper = db.refpersonales_cred.Where(x => x.id == id).FirstOrDefault();
            if (referenciasper != null)
            {
                db.refpersonales_cred.Remove(referenciasper);
                result = db.SaveChanges();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: v_creditos/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            v_creditos v_creditos = db.v_creditos.Find(id);

            if (v_creditos == null)
            {
                return HttpNotFound();
            }

            vinfcredito infocredito = db.vinfcredito.FirstOrDefault(x => x.id == v_creditos.infocredito_id);
            icb_terceros info_tercero = db.icb_terceros.FirstOrDefault(x => x.tercero_id == infocredito.tercero);
            vw_dirtercero dir_tercero = db.vw_dirtercero.FirstOrDefault(x => x.idtercero == infocredito.tercero);
            vpedido monto_vehiculo = db.vpedido.FirstOrDefault(x => x.id == v_creditos.pedido);

            //var Wcorreo = info_tercero.email_tercero != null ? info_tercero.email_tercero

            v_creditosform credito = new v_creditosform
            {
                asesor_id = v_creditos.asesor_id,
                bodegaid = v_creditos.bodegaid,
                comison = v_creditos.comison,
                concesionarioid = v_creditos.concesionarioid,
                cuota_inicial = v_creditos.cuota_inicial != null ? v_creditos.cuota_inicial.ToString() : "",
                detalle = v_creditos.detalle,
                estado = v_creditos.estado,
                estadoc = v_creditos.estadoc,
                fec_actualizacion = v_creditos.fec_actualizacion != null
                    ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_aprobacion = v_creditos.fec_aprobacion != null
                    ? v_creditos.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_confirmacion = v_creditos.fec_confirmacion != null
                    ? v_creditos.fec_confirmacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_creacion = v_creditos.fec_creacion != null
                    ? v_creditos.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_desembolso = v_creditos.fec_desembolso != null
                    ? v_creditos.fec_desembolso.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_desistimiento = v_creditos.fec_desistimiento != null
                    ? v_creditos.fec_desistimiento.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_entdocumentos = v_creditos.fec_entdocumentos != null
                    ? v_creditos.fec_entdocumentos.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_envdocumentos = v_creditos.fec_envdocumentos != null
                    ? v_creditos.fec_envdocumentos.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_facturacomision = v_creditos.fec_facturacomision != null
                    ? v_creditos.fec_facturacomision.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_negacion = v_creditos.fec_negacion != null
                    ? v_creditos.fec_negacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_solicitud = v_creditos.fec_solicitud != null
                    ? v_creditos.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                financiera_id = v_creditos.financiera_id,
                id_licencia = v_creditos.id_licencia,
                infocredito_id = v_creditos.infocredito_id,
                motivodesiste = v_creditos.motivodesiste,
                numfactura = v_creditos.numfactura,
                num_aprobacion = v_creditos.num_aprobacion,
                pedido = v_creditos.pedido,
                plan_id = v_creditos.plan_id,
                plazo = v_creditos.plazo,
                razon_inactivo = v_creditos.razon_inactivo,
                toma_credito = v_creditos.toma_credito,
                userid_creacion = v_creditos.userid_creacion,
                user_idactualizacion = v_creditos.user_idactualizacion,
                valor_comision = v_creditos.valor_comision != null ? v_creditos.valor_comision.ToString() : "",
                vaprobado = v_creditos.vaprobado != null ? Math.Round((double)v_creditos.vaprobado).ToString() : "",
                vehiculo = v_creditos.vehiculo,
                vehiculo2 = v_creditos.vehiculo,
                vsolicitado = v_creditos.vsolicitado != null
                    ? Math.Round((double)v_creditos.vsolicitado).ToString()
                    : "",
                Id = v_creditos.Id,
                tercero = infocredito.tercero,
                correo_tercero = info_tercero.email_tercero != null ? info_tercero.email_tercero : "",
                telefono_tercero = info_tercero.prinom_tercero != null ? info_tercero.celular_tercero != null
                        ?
                        info_tercero.celular_tercero
                        : "" :
                    info_tercero.telf_tercero != null ? info_tercero.telf_tercero : "",
                direccion_tercero =
                    dir_tercero != null ? dir_tercero.direccion != null ? dir_tercero.direccion : "" : "",
                precio_vehiculo = monto_vehiculo != null
                    ? monto_vehiculo.vrtotal != null ? monto_vehiculo.vrtotal.Value.ToString("N2") : ""
                    : "",
                poliza = v_creditos.poliza
            };
            listas(credito);
            ConsultaDatosCreacion(credito);
            BuscarFavoritos(menu);
            // ojo
            var pedidos = (from ped in db.vpedido
                           join ped_pg in db.vpedpago
                               on ped.id equals ped_pg.idpedido
                           where ped.nit == infocredito.tercero
                                 && ped.modelo == v_creditos.vehiculo
                                 && ped_pg.condicion == 1
                           select new
                           {
                               ped.id,
                               numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null
                                            ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " +
                                              ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " +
                                              ped.icb_terceros.segapellido_tercero
                                            : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                               ped_pg.valor
                           }).ToList();
            ViewBag.numeropedidos = pedidos.Count();
            pedidos.OrderByDescending(d => d.numero);
            ViewBag.pedido = new SelectList(pedidos, "id", "numero", v_creditos.pedido);

            var planbusca = (from pd in db.icb_plan_financiero
                             where pd.plan_id == credito.plan_id
                             select new
                             {
                                 pd.plan_id,
                                 pd.plan_nombre
                             }).ToList();


            if (infocredito.ingsalario != null)
            {
                string[] salario = infocredito.ingsalario.Split('-');
                if (salario.Length > 1)
                {
                    string de = salario[0];
                    string hasta = salario[1];
                    ViewBag.salariode = de;
                    ViewBag.salariohasta = hasta;
                    ViewBag.infsalario = 1;
                }
                else
                {
                    ViewBag.infosalario = 2;
                    ViewBag.salario = infocredito.ingsalario;
                }
            }
            else
            {
                ViewBag.infosalario = 0;
            }




            ViewBag.plan_id = new SelectList(planbusca, "plan_id", "plan_nombre");
            ViewBag.salario = infocredito != null
                ? infocredito.ingsalario != null ? infocredito.ingsalario : ""
                : "";
            ViewBag.comisiones = infocredito != null
                ? infocredito.ingcomision != null ? infocredito.ingcomision.Value.ToString("N0") : ""
                : "";
            ViewBag.arriendo = infocredito != null
                ? infocredito.ingarriendo != null ? infocredito.ingarriendo.Value.ToString("N0") : ""
                : "";
            ViewBag.otros = infocredito != null
                ? infocredito.ingotros != null ? infocredito.ingotros.Value.ToString("N0") : ""
                : "";

            return View(credito);
        }

        public ActionResult creditosBackOffice(int? menu)
        {
            v_creditosform vcreditos = new v_creditosform();
            listas(vcreditos);
            BuscarFavoritos(menu);
            return View();
        }

        // POST: v_creditos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(v_creditosform credito, int? menu)
        {
            string idFinanciera2 = Request["financiera_id2"];
            vinfcredito infocredito = db.vinfcredito.FirstOrDefault(x => x.id == credito.infocredito_id);
            int idBuscado = idFinanciera2 != null ? Convert.ToInt32(idFinanciera2) : 0;
            credito.financiera_id = idBuscado;
            if (infocredito.ingsalario != null)
            {
                string[] salario = infocredito.ingsalario.Split('-');
                if (salario.Length > 1)
                {
                    string de = salario[0];
                    string hasta = salario[1];
                    ViewBag.salariode = de;
                    ViewBag.salariohasta = hasta;
                    ViewBag.infsalario = 1;
                }
                else
                {
                    ViewBag.infosalario = 2;
                    ViewBag.salario = infocredito.ingsalario;
                }
            }
            else
            {
                ViewBag.infosalario = 0;
            }

            if (ModelState.IsValid)
            {
                credito.fec_actualizacion = Convert.ToString(DateTime.Now);
                credito.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                v_creditos v_creditos = db.v_creditos.Where(d => d.Id == credito.Id).FirstOrDefault();
                //cargo datos desde el formulario al objeto v_creditos
                v_creditos.asesor_id = credito.asesor_id;
                v_creditos.bodegaid = credito.bodegaid;
                v_creditos.comison = credito.comison;
                v_creditos.concesionarioid = credito.concesionarioid;
                v_creditos.cuota_inicial = string.IsNullOrWhiteSpace(credito.cuota_inicial)
                    ? 0
                    : Convert.ToDecimal(credito.cuota_inicial, new CultureInfo("is-IS"));
                v_creditos.detalle = credito.detalle;
                v_creditos.estado = credito.estado;
                v_creditos.estadoc = credito.estadoc;

                if (credito.estadoc == "N")
                {
                    v_creditos.idmotnegacion = Convert.ToInt32(Request["motNegacion"]);
                }

                //v_creditos.fec_actualizacion = credito.fec_actualizacion;
                v_creditos.fec_actualizacion = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(credito.fec_aprobacion))
                {
                    v_creditos.fec_aprobacion = Convert.ToDateTime(credito.fec_aprobacion);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_confirmacion))
                {
                    v_creditos.fec_confirmacion = Convert.ToDateTime(credito.fec_confirmacion);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_creacion))
                {
                    v_creditos.fec_creacion = Convert.ToDateTime(credito.fec_creacion);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_desembolso))
                {
                    v_creditos.fec_desembolso = Convert.ToDateTime(credito.fec_desembolso);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_desistimiento))
                {
                    v_creditos.fec_desistimiento = Convert.ToDateTime(credito.fec_desistimiento);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_entdocumentos))
                {
                    v_creditos.fec_entdocumentos = Convert.ToDateTime(credito.fec_entdocumentos);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_envdocumentos))
                {
                    v_creditos.fec_envdocumentos = Convert.ToDateTime(credito.fec_envdocumentos);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_facturacomision))
                {
                    v_creditos.fec_facturacomision = Convert.ToDateTime(credito.fec_facturacomision);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_negacion))
                {
                    v_creditos.fec_negacion = Convert.ToDateTime(credito.fec_negacion);
                }

                if (!string.IsNullOrWhiteSpace(credito.fec_solicitud))
                {
                    v_creditos.fec_solicitud = Convert.ToDateTime(credito.fec_solicitud);
                }

                v_creditos.financiera_id = credito.financiera_id != 0 ? credito.financiera_id : credito.financiera_id2;
                v_creditos.id_licencia = credito.id_licencia;
                v_creditos.infocredito_id = credito.infocredito_id;
                v_creditos.motivodesiste = credito.motivodesiste;
                v_creditos.numfactura = credito.numfactura;
                v_creditos.num_aprobacion = credito.num_aprobacion;
                v_creditos.pedido = credito.pedido;
                v_creditos.plan_id = credito.plan_id;
                v_creditos.plazo = credito.plazo;
                v_creditos.razon_inactivo = credito.razon_inactivo;
                v_creditos.toma_credito = credito.toma_credito;
                v_creditos.userid_creacion = credito.userid_creacion;
                v_creditos.user_idactualizacion = credito.user_idactualizacion;
                v_creditos.valor_comision = string.IsNullOrWhiteSpace(credito.valor_comision)
                    ? 0
                    : Convert.ToDecimal(credito.valor_comision, new CultureInfo("is-IS"));
                v_creditos.vaprobado = string.IsNullOrWhiteSpace(credito.vaprobado)
                    ? 0
                    : Convert.ToDecimal(credito.vaprobado, new CultureInfo("is-IS"));
                v_creditos.vehiculo = credito.vehiculo2;
                v_creditos.vsolicitado = string.IsNullOrWhiteSpace(credito.vsolicitado)
                    ? 0
                    : Convert.ToDecimal(credito.vsolicitado, new CultureInfo("is-IS"));
                v_creditos.poliza = credito.poliza != null ? credito.poliza : "";

                //Calculo de comision
                if (v_creditos.fec_confirmacion != null && v_creditos.comison)
                {
                    v_creditos.fec_negacion = null;
                    if (v_creditos.plan_id != null)
                    {
                        icb_plan_financiero buscarPlan = db.icb_plan_financiero.FirstOrDefault(x => x.plan_id == v_creditos.plan_id);
                        double porcentajeEncontrado = buscarPlan != null ? buscarPlan.plan_porcentaje_comision ?? 0 : 0;
                        v_creditos.valor_comision =
                            v_creditos.vaprobado * Convert.ToDecimal(porcentajeEncontrado, new CultureInfo("is-IS")) / 100;
                    }
                    else
                    {
                        string tipoComision = db.icb_unidad_financiera
                            .FirstOrDefault(x => x.financiera_id == v_creditos.financiera_id).tipocomision;
                        if (tipoComision == "1")
                        {
                            decimal? porcentaje = db.icb_unidad_financiera
                                .FirstOrDefault(x => x.financiera_id == v_creditos.financiera_id).valor_comision;
                            v_creditos.valor_comision = v_creditos.vaprobado * Convert.ToDecimal(porcentaje, new CultureInfo("is-IS")) / 100;
                        }
                        else if (tipoComision == "0")
                        {
                            decimal? fcomision = db.icb_unidad_financiera
                                .FirstOrDefault(x => x.financiera_id == v_creditos.financiera_id).valor_comision_monto;
                            fcomision = fcomision / 1000000;
                            v_creditos.valor_comision = v_creditos.vaprobado * fcomision;
                        }
                    }
                }

                db.Entry(v_creditos).State = EntityState.Modified;

                #region Seguimiento

                vinfcredito infoCredito = db.vinfcredito.FirstOrDefault(x => x.id == credito.infocredito_id);
                string financiera = db.icb_unidad_financiera
                    .FirstOrDefault(x => x.financiera_id == v_creditos.financiera_id).financiera_nombre;
                db.seguimientotercero.Add(new seguimientotercero
                {
                    idtercero = infoCredito.tercero,
                    tipo = 13,
                    nota = "Se genero solicitud de credito con la financiera: " + financiera,
                    fecha = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                });

                if (v_creditos.pedido != null)
                {
                    int? cotizacion = db.vpedido.FirstOrDefault(x => x.id == v_creditos.pedido).idcotizacion;
                    if (cotizacion != null)
                    {
                        vcotseguimiento seguimiento = new vcotseguimiento
                        {
                            cot_id = Convert.ToInt32(cotizacion),
                            fecha = DateTime.Now,
                            responsable = Convert.ToInt32(Session["user_usuarioid"]),
                            Notas = "Se genero solicitud de credito con la finciera " + financiera,
                            Motivo = null,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            tipo_seguimiento = 13
                        };
                        db.vcotseguimiento.Add(seguimiento);
                    }
                }

                #endregion

                db.SaveChanges();
                TempData["mensaje"] = "Registro Actualizado Correctamente";
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                TempData["mensaje_error"] = "Errores en la actualizacion del registro";
            }

            ViewBag.estadoc = new SelectList(db.estados_credito, "codigo", "descripcion", credito.estadoc);
            ViewBag.financiera_id = new SelectList(db.icb_unidad_financiera, "financiera_id", "financiera_nombre",
                credito.financiera_id);
            ViewBag.plan_id = new SelectList(db.icb_plan_financiero, "plan_id", "plan_nombre", credito.plan_id);
            listas(credito);
            //var pedidos = (from ped in db.vpedido
            //               join ped_pg in db.vpedpago
            //               on ped.id equals ped_pg.idpedido
            //               where ped_pg.condicion == 1 && ped.nit == wnit
            //               select new
            //               {
            //                   id = ped.id,
            //                   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
            //                   valor = ped_pg.valor,
            //               }).ToList();
            var pedidos = (from ped in db.vpedido
                           join ped_pg in db.vpedpago
                               on ped.id equals ped_pg.idpedido
                           where ped.nit == credito.tercero
                                 && ped.modelo == credito.vehiculo
                                 && ped_pg.condicion == 1
                           select new
                           {
                               ped.id,
                               numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null
                                            ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " +
                                              ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " +
                                              ped.icb_terceros.segapellido_tercero
                                            : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                               ped_pg.valor
                           }).ToList();
            ViewBag.numeropedidos = pedidos.Count();
            pedidos.OrderByDescending(d => d.numero);
            ViewBag.pedido = new SelectList(pedidos, "id", "numero", credito.pedido);
            ConsultaDatosCreacion(credito);
            credito.vaprobado =
                credito.vaprobado != null ? Convert.ToString(Convert.ToDecimal(credito.vaprobado, new CultureInfo("is-IS"))) : "0";
            BuscarFavoritos(menu);
            return View(credito);
        }

        public JsonResult BuscarMotivosNegacion(int? id)
        {
            v_creditos credito = db.v_creditos.FirstOrDefault(x => x.Id == id);
            if (credito.estadoc == "N" && credito.idmotnegacion != null)
            {
                var info = (from e in db.motivosNegacion
                            select new
                            {
                                e.id,
                                e.descripcion
                            }).ToList();


                return Json(new { info, guardado = credito.idmotnegacion }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var info = (from e in db.motivosNegacion
                            select new
                            {
                                e.id,
                                e.descripcion
                            }).ToList();

                return Json(new { info, guardado = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public void ConsultaDatosCreacion(v_creditosform v_creditos)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(v_creditos.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(v_creditos.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public ActionResult Seguimiento(int id, int? menu)
        {
            int credito_id = db.v_creditos.Find(id).Id;
            vcredseguimiento seguimiento = new vcredseguimiento
            {
                credito_id = credito_id
            };
            ViewBag.tipo = new SelectList(db.vtiposeguimientocot, "id_tipo_seguimiento", "nombre_seguimiento");
            BuscarFavoritos(menu);
            return View(seguimiento);
        }

        [HttpPost]
        public ActionResult Seguimiento(vcredseguimiento seguimiento, int? menu)
        {
            if (seguimiento.observacion != null)
            {
                seguimiento.fecha = DateTime.Now;
                seguimiento.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                seguimiento.responsable = Convert.ToInt32(Session["user_usuarioid"]);
                db.vcredseguimiento.Add(seguimiento);
                int result = db.SaveChanges();
                if (result > 0)
                {
                    TempData["mensaje"] = "Nota agregada correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error al agregar la nota, por favor intente nuevamente";
                }
            }
            else
            {
                TempData["mensaje_error"] = "No se agrego  ninguna nota, por favor ingrese nota nuevamente";
            }

            ViewBag.tipo = new SelectList(db.vtiposeguimientocot, "id_tipo_seguimiento", "nombre_seguimiento");
            BuscarFavoritos(menu);
            return View(seguimiento);
        }

        public JsonResult ActualizarV_pedido(int? idPed, int? idfinan)
        {
            bool result = false;
            if (idPed > 0)
            {
                vpedido buscarPedido = db.vpedido.FirstOrDefault(c => c.id == idPed);
                if (buscarPedido != null)
                {
                    // var pedidosActualizar = db.v_creditos.FirstOrDefault(p => p.pedido == buscarPedido.id);
                    // pedidosActualizar.estadoc = "F";
                    // db.Entry(pedidosActualizar).State = EntityState.Modified;
                    // db.SaveChanges();

                    buscarPedido.nit_prenda = idfinan;
                    //buscarPedido.

                    db.Entry(buscarPedido).State = EntityState.Modified;
                    int actualizar = db.SaveChanges();
                    TempData["mensaje"] = "Registro de Pedido Actualizado Correctamente";
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult fueConfirmado(int ped, int no_mod)
        {
            bool result = true;
            //var Ped = 0

            //var buscarPedidos = db.vpedido.FirstOrDefault(c => c.id == idPed);
            //if (buscarPedido != null)
            //{

            //    buscarPedido.nit_prenda = idfinan;

            //    db.Entry(buscarPedido).State = EntityState.Modified;
            //    var actualizar = db.SaveChanges();
            //    TempData["mensaje"] = "Registro de Pedido Actualizado Correctamente";
            //    if (actualizar > 0)
            //    {
            //        result = true;
            //        return Json(result, JsonRequestBehavior.AllowGet);
            //    }
            //}
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult BuscarPedidosNit(id)
        //{
        //    wwid = id;
        //    var pedidos = (from ped in db.vpedido
        //                   where ped.nit == wwid
        //                   select new
        //                   {
        //                       id = ped.id,
        //                       numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
        //                   }).ToList();
        //}

        public ActionResult Indicadores(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatos(DateTime? fecha_inicio, DateTime? fecha_fin)
        {
            if (fecha_inicio == null)
            {
                fecha_inicio = DateTime.Now.AddMonths(-1);
            }

            if (fecha_fin == null)
            {
                fecha_fin = DateTime.Now;
            }
            else
            {
                fecha_fin = fecha_fin.Value.AddDays(1);
            }

            List<int?> pedidosotro = (from cc in db.v_creditos
                                      join ec2 in db.estados_credito
                                          on cc.estadoc equals ec2.codigo //into cppcp
                                                                          //  from ec2 in cppcp.DefaultIfEmpty()
                                      where cc.estadoc == "C" && cc.pedido != null
                                      select cc.pedido).ToList();

            var data2 = (from c in db.v_creditos
                         join f in db.icb_unidad_financiera
                             on c.financiera_id equals f.financiera_id
                         join info in db.vinfcredito
                             on c.infocredito_id equals info.id
                         join t in db.icb_terceros
                             on info.tercero equals t.tercero_id
                         join ec in db.estados_credito
                             on c.estadoc equals ec.codigo
                         join vp in db.vpedido
                             on c.pedido equals vp.id into cvp
                         from vp in cvp.DefaultIfEmpty()
                         join m in db.vmotivocomision
                             on c.idmotcomision equals m.id into mot
                         from m in mot.DefaultIfEmpty()
                         join n in db.motivosNegacion
                             on c.idmotnegacion equals n.id into mot2
                         from n in mot2.DefaultIfEmpty()
                         where !pedidosotro.Contains(c.pedido) && c.fec_creacion >= fecha_inicio &&
                               c.fec_creacion <=
                               fecha_fin /// agregado para validar que no aparezcan los creditos que ya estan en estado de Confirmado
                         select new
                         {
                             id = c.Id,
                             nombre = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + ") " +
                                      t.razon_social,
                             financiera = f.financiera_nombre,
                             c.fec_solicitud, //!= null ? c.fec_solicitud.ToString() : "", //.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                                              //  fec_actualizacion = v_creditos.fec_actualizacion!= null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                             c.fec_desembolso, //!= null ? c.fec_desembolso.ToString() : "",
                             estadoc = ec.descripcion,
                             pedido = c.vpedido.numero.Value.ToString(),
                             iestadoc = c.estadoc,
                             c.fec_aprobacion,
                             c.fec_negacion,
                             //c.fec_desembolso,
                             c.fec_confirmacion,
                             c.fec_envdocumentos,
                             c.fec_desistimiento,
                             m.motivo,
                             n.descripcion
                             //fecha_estado = c.estadoc == "A" ? c.fec_aprobacion.ToString():
                             // c.estadoc == "N" ? c.fec_negacion.ToString() :
                             // c.estadoc == "D" ? c.fec_desembolso.ToString() :
                             // c.estadoc == "C" ? c.fec_confirmacion.ToString() :
                             // c.estadoc == "T" ? c.fec_envdocumentos.ToString() :
                             // c.estadoc == "O" ? c.fec_desistimiento.ToString():""
                         }).ToList();
            List<bDatos> data = data2.Select(c => new bDatos
            {
                id = c.id,
                nombre = c.nombre,
                financiera = c.financiera,
                fec_solicitud = c.fec_solicitud != null
                    ? c.fec_solicitud.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                fec_desembolso = c.fec_desembolso != null
                    ? c.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                estadoc = c.estadoc,
                pedido = c.pedido,
                fecha_estado = c.iestadoc == "A" ? c.fec_aprobacion != null
                        ?
                        c.fec_aprobacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" :
                    c.iestadoc == "N" ? c.fec_negacion != null
                        ?
                        c.fec_negacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" :
                    c.iestadoc == "D" ? c.fec_desembolso != null
                        ?
                        c.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" :
                    c.iestadoc == "C" ? c.fec_confirmacion != null
                        ?
                        c.fec_confirmacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" :
                    c.iestadoc == "T" ? c.fec_envdocumentos != null
                        ?
                        c.fec_envdocumentos.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" :
                    c.iestadoc == "O" ? c.fec_desistimiento != null
                        ?
                        c.fec_desistimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "" : "",
                motivo = c.motivo != null ? c.motivo : "",
                descripcion = c.descripcion != null ? c.descripcion : ""
                //fecha_estado = c.fecha_estado
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarCreditosFiltro(int? financiera, DateTime? desde, DateTime? hasta, int? tercero,
            int? pedido)
        {
            if (desde != null && hasta != null)
            {
                if (desde > hasta)
                {
                    return Json(new { error = true }, JsonRequestBehavior.AllowGet);
                }
            }

            System.Linq.Expressions.Expression<Func<vw_creditosBackOffice, bool>> fechas = PredicateBuilder.True<vw_creditosBackOffice>();
            System.Linq.Expressions.Expression<Func<vw_creditosBackOffice, bool>> financieraP = PredicateBuilder.False<vw_creditosBackOffice>();
            System.Linq.Expressions.Expression<Func<vw_creditosBackOffice, bool>> tercercoP = PredicateBuilder.False<vw_creditosBackOffice>();
            System.Linq.Expressions.Expression<Func<vw_creditosBackOffice, bool>> pedidoP = PredicateBuilder.False<vw_creditosBackOffice>();

            #region validacion de fechas

            if (desde == null)
            {
                desde = db.v_creditos.OrderBy(x => x.fec_solicitud).Select(x => x.fec_solicitud).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = db.v_creditos.OrderByDescending(x => x.fec_solicitud).Select(x => x.fec_solicitud)
                    .FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault();
            }

            #endregion

            fechas = fechas.And(x => x.fec_solicitud >= desde);
            fechas = fechas.And(x => x.fec_solicitud <= hasta);

            #region Predicate pedido

            if (pedido != null)
            {
                pedidoP = pedidoP.Or(d => d.pedido == pedido);
                fechas = fechas.And(pedidoP);
            }

            #endregion

            #region Predicate tercero

            if (tercero != null)
            {
                tercercoP = tercercoP.Or(d => d.tercero_id == tercero);
                fechas = fechas.And(tercercoP);
            }

            #endregion

            #region Predicate financiera

            if (financiera != null)
            {
                financieraP = financieraP.Or(d => d.financiera_id == financiera);
                fechas = fechas.And(financieraP);
            }

            #endregion

            List<vw_creditosBackOffice> consulta = db.vw_creditosBackOffice.Where(fechas).ToList();

            var data = consulta.Select(x => new
            {
                estadoc = x.estadoc != null ? x.estadoc : "",
                x.facturado,
                fec_desembolso = x.fec_desembolso != null
                    ? x.fec_desembolso.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_solicitud = x.fec_solicitud != null
                    ? x.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                financiera = x.financiera != null ? x.financiera : "",
                x.financiera_id,
                x.id,
                nombre = x.nombre != null ? x.nombre : "",
                pedido = x.pedido != null ? x.pedido.ToString() : "",
                planmayor = x.planmayor != null ? x.planmayor : "",
                ref_descripcion = x.ref_descripcion != null ? x.ref_descripcion : "",
                x.tercero_id,
                x.valorP,
                poliza = x.poliza != null ? x.poliza : ""
            }).ToList();

            return Json(new { error = false, data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosCliente(int? id)
        {
            //verifico si el id no es nulo
            if (id != null)
            {
                var data = from t in db.icb_terceros
                           join c in db.nom_ciudad
                               on t.ciu_id equals c.ciu_id
                           where t.tercero_id == id
                           select new
                           {
                               nombre =
                                   t.prinom_tercero != null ? t.prinom_tercero + " " + t.apellido_tercero : t.razon_social,
                               ciudad = c.ciu_nombre,
                               telefono = t.telf_tercero,
                               email = t.email_tercero,
                               celular = t.celular_tercero
                           };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosPedido(int? pedido)
        {
            //    var pedidos = (from ped in db.vpedido
            //                   join ped_pg in db.vpedpago
            //                   on ped.id equals ped_pg.idpedido
            //                   where ped.nit == infocredito.tercero
            //                   && ped.modelo == v_creditos.vehiculo
            //                   && ped_pg.condicion == 1

            //                   select new
            //                   {
            //                       id = ped.id,
            //                       numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
            //                       valor = ped_pg.valor,
            //                   }).ToList();
            var data2 = (from p in db.vpedido
                         join ped_pg in db.vpedpago
                             on p.id equals ped_pg.idpedido
                         where p.id == pedido
                               && ped_pg.condicion == 1
                         select new
                         {
                             p.nit,
                             p.vendedor,
                             p.modelo,
                             ped_pg.valor,
                             valorveh = p.vrtotal,
                             p.id_anio_modelo
                         }).ToList();
            List<datosPed> data = data2.Select(x => new datosPed
            {
                nit = x.nit != null ? x.nit ?? 0 : 0,
                vendedor = x.vendedor != null ? x.vendedor ?? 0 : 0,
                modelo = x.modelo != null ? x.modelo : "",
                valor = x.valor != null ? x.valor ?? 0 : 0,
                anio = x.id_anio_modelo,
                valorveh = x.valorveh != null ? x.valorveh.Value.ToString("N0", new CultureInfo("is-IS")) : ""
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidoNit(int? wnit)
        {
            /*var buscarParametro = db.icb_sysparameter.Where(x => x.syspar_cod == "P55").FirstOrDefault();
			var condicionparametro = buscarParametro != null ? Convert.ToInt32(buscarParametro.syspar_value) : 1;*/

            if (wnit != null)
            {
                var pedidos = (from ped in db.vpedido
                               join ped_pg in db.vpedpago
                                   on ped.id equals ped_pg.idpedido
                               where ped_pg.condicion == 1 && ped.nit == wnit
                               select new
                               {
                                   ped.id,
                                   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null
                                                ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero +
                                                  " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero +
                                                  " " + ped.icb_terceros.segapellido_tercero
                                                : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                                   ped_pg.valor
                               }).ToList();
                pedidos.OrderByDescending(d => d.numero);
                //   ViewBag.pedido = new SelectList(pedidos, "id", "numero");

                return Json(pedidos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<int?> pedidosotro = (from cc in db.v_creditos
                                          join ec2 in db.estados_credito
                                              on cc.estadoc equals ec2.codigo //into cppcp
                                                                              //  from ec2 in cppcp.DefaultIfEmpty()
                                          where cc.estadoc == "C" && cc.pedido != null
                                          select cc.pedido).ToList();


                var pedidos = (from ped in db.vpedido
                               join ped_pg in db.vpedpago
                                   on ped.id equals ped_pg.idpedido
                               where ped_pg.condicion == 1 && !pedidosotro.Contains(ped.id)
                               select new
                               {
                                   ped.id,
                                   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null
                                                ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero +
                                                  " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero +
                                                  " " + ped.icb_terceros.segapellido_tercero
                                                : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                                   ped_pg.valor
                               }).OrderBy(x => x.id).ToList();
                pedidos.OrderByDescending(d => d.numero);
                //	ViewBag.pedido = new SelectList(pedidos, "id", "numero", v_creditos.pedido);


                //var pedidos = (from ped in db.vpedido
                //			   join ped_pg in db.vpedpago
                //			   on ped.id equals ped_pg.idpedido
                //			   where ped_pg.condicion == 1
                //			   select new
                //			   {
                //				   id = ped.id,
                //				   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
                //				   valor = ped_pg.valor,
                //			   }).ToList();
                // pedidos.OrderByDescending(d => d.numero);
                //   ViewBag.pedido = new SelectList(pedidos, "id", "numero");

                return Json(pedidos, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarSinPedido_Nit(int? wnit)
        {
            var personas = (from per in db.icb_terceros
                            join dir in db.terceros_direcciones
                                on per.tercero_id equals dir.idtercero into dirter
                            from dir in dirter.DefaultIfEmpty()
                            where per.tercero_id == wnit // per.tercero_estado == true
                            select new
                            {
                                per.tercero_id,
                                per.email_tercero,
                                per.telf_tercero,
                                per.celular_tercero,
                                per.tpdoc_id,
                                per.razon_social,
                                dir.direccion
                            }).ToList();
            var persona = personas.Select(c => new
            {
                id = c.tercero_id,
                email = c.email_tercero != null ? c.email_tercero : "",
                telefono = c.tpdoc_id == 1 ? c.telf_tercero : c.celular_tercero,
                direccion = c.direccion != null ? c.direccion : ""
                //userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion: "",
                // userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.Value.ToShortDateString() + " " + c.userfec_actualizacion.Value.ToShortTimeString() : "",
                //  userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.ToString(): "",  // ojo 
                //userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : c.userfec_creacion != null ? c.userfec_creacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "", //!= null ? c.userfec_actualizacion.Value.Year + "/" + c.userfec_actualizacion.Value.Month + "/" + c.userfec_actualizacion.Value.Day  : "",
                //estadoUsuario = c.user_estado == true ? "Activo" : "Inactivo"
            }).FirstOrDefault();
            //persona.OrderByDescending(d => d.tercero_id);
            //   ViewBag.pedido = new SelectList(pedidos, "id", "numero");
            return Json(persona, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarSeguimientos(int id)
        {
            var data = from seguimiento in db.vcredseguimiento
                       join usuario in db.users
                           on seguimiento.responsable equals usuario.user_id
                       where seguimiento.credito_id == id
                       select new
                       {
                           fec_creacion = seguimiento.fecha.ToString(),
                           responsable = usuario.user_nombre + " " + usuario.user_apellido,
                           seguimiento.observacion
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserConfirmacionPendiente()
        {
            return View();
        }

        public JsonResult buscarSinConfirmar()
        {
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            var buscar = (from a in db.v_creditos
                          join b in db.icb_unidad_financiera
                              on a.financiera_id equals b.financiera_id
                          join c in db.vinfcredito
                              on a.infocredito_id equals c.id
                          join d in db.icb_terceros
                              on c.tercero equals d.tercero_id
                          where a.estadoc == "D"
                          select new { a, b, c, d }).ToList();

            var data = buscar.Select(x => new
            {
                x.a.Id,
                fec_solicitud = x.a.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm"),
                pedido = x.a.pedido != null ? Convert.ToString(x.a.pedido) : "",
                financiera = x.b.financiera_nombre,
                valor_solicitado = x.a.vsolicitado.Value.ToString("0,0", elGR),
                valor_aprobado = x.a.vaprobado.Value.ToString("0,0", elGR),
                tercero = x.d.prinom_tercero + " " + x.d.segnom_tercero + " " + x.d.apellido_tercero + " " +
                          x.d.segapellido_tercero,
                documento_tercero = x.d.doc_tercero,
                fijo = x.d.telf_tercero != null ? x.d.telf_tercero : "Sin fijo",
                celular = x.d.celular_tercero != null ? x.d.celular_tercero : "Sin Celular",
                email = x.d.email_tercero != null ? x.d.email_tercero : "",
                x.a.plazo
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult confirmarCredito(string id, string fecha)
        {
            int guardar = 0;
            if (id != null && id != "" && fecha != null && fecha != "")
            {
                int Id = Convert.ToInt32(id);
                v_creditos find = db.v_creditos.FirstOrDefault(x => x.Id == Id);

                if (find != null)
                {
                    find.fec_confirmacion = Convert.ToDateTime(fecha);
                    find.estadoc = "C";
                    db.Entry(find).State = EntityState.Modified;
                    guardar = db.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(1, JsonRequestBehavior.AllowGet); //Guardado exitosamente
                    }

                    return Json(2, JsonRequestBehavior.AllowGet); //No se realizaron cambios
                }

                return Json(3, JsonRequestBehavior.AllowGet); //No encontrado
            }

            return Json(4, JsonRequestBehavior.AllowGet); //datos vacios o null
        }

        public JsonResult cotizacion_pedido(int id_tercero)
        {
            var pedidos2 = db.vpedido.Where(x => x.nit == id_tercero).Select(x => new
            {
                x.id,
                x.numero,
                x.nit,
                x.bodega,
                bodega_nom = x.bodega_concesionario.bodccs_nombre,
                asesor = x.users.user_nombre + " " + x.users.user_apellido,
                vehiculo = x.modelo_vehiculo.modvh_nombre,
                x.fecha
            }).ToList();

            var cotizaciones2 = db.vw_cotizacion.Where(x => x.tercero_id == id_tercero).Select(x => new
            {
                x.cot_idserial,
                x.cot_numcotizacion,
                nombre = x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero + " " +
                         x.segapellido_tercero,
                x.bodccs_nombre,
                x.asesor,
                x.vehiculo,
                x.cot_feccreacion
            }).ToList();

            var pedidos = pedidos2.Select(x => new
            {
                x.id,
                x.numero,
                x.nit,
                x.bodega,
                x.bodega_nom,
                x.asesor,
                x.vehiculo,
                fecha = x.fecha.ToString("yyyy/MM/dd")
            }).ToList();

            var cotizaciones = cotizaciones2.Select(x => new
            {
                x.cot_idserial,
                x.cot_numcotizacion,
                x.nombre,
                x.bodccs_nombre,
                x.asesor,
                x.vehiculo,
                cot_feccreacion = x.cot_feccreacion.ToString("yyyy/MM/dd")
            }).ToList();

            var data = new
            {
                pedidos,
                cotizaciones
            };


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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

        public class bDatos
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public string financiera { get; set; }
            public string fec_solicitud { get; set; }
            public string fec_desembolso { get; set; }
            public string estadoc { get; set; }
            public string pedido { get; set; }
            public string fecha_estado { get; set; }
            public string motivo { get; set; }
            public string descripcion { get; set; }
        }

        public class datosPed
        {
            public int nit { get; set; }
            public int? anio { get; set; }
            public int vendedor { get; set; }
            public string modelo { get; set; }
            public decimal valor { get; set; }
            public string valorveh { get; set; }
        }
    }
}

//var buscarPedidoVCredito = db.vpedido.FirstOrDefault(x => x.id == v_creditos.Id);
//var buscarPedidosCliente = db.vpedido.Where(x => x.nit == infocredito.tercero).ToList();
//var pedidosx = (from ped in db.vpedido
//               join ped_pg in db.vpedpago
//               on ped.id equals ped_pg.idpedido
//               where ped_pg.condicion == 1
//               select new
//               {
//                   id = ped.id,
//                   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
//                   valor = ped_pg.valor,
//               }).OrderBy(x => x.id).ToList();

//if (!string.IsNullOrWhiteSpace(credito.fec_actualizacion))
//{
//    var xfec_actualizacion = Convert.ToDateTime(credito.fec_actualizacion);
//    ViewBag.xfec_actualizacion = xfec_actualizacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
//}

//if (!string.IsNullOrWhiteSpace(credito.fec_aprobacion))
//{
//    var xfec_aprobacion = Convert.ToDateTime(credito.fec_aprobacion);
//ViewBag.xfec_aprobacion = xfec_aprobacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
//}

//if (!string.IsNullOrWhiteSpace(credito.fec_envdocumentos))
//{
//    var xfec_envdocumentos = Convert.ToDateTime(credito.fec_envdocumentos);
//    ViewBag.xfec_envdocumentos = new DateTime(xfec_envdocumentos.Year, xfec_envdocumentos.Month, xfec_envdocumentos.Day).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
//}
//var xfec_aprobacion = Convert.ToDateTime(credito.fec_aprobacion);
//ViewBag.fec_aprobacion = new DateTime(xfec_aprobacion.Year, xfec_aprobacion.Month, 1).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));


//  ViewBag.fechadesde = new DateTime(date.Year, date.Month, 1).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
// var pedi = (from pd in db.vpedido
//             join mod in db.modelo_vehiculo
//             on pd.id equals mod.modvh_nombre
//             select new
//             {
//                 id = pd.id,

//             }).Tolist();
//ViewBag.pedi = new SelectList(db.modelo_vehiculo,  "modvh_nombre");