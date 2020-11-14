using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class InspeccionPeritajeController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        public void listas(Modelo_adecuaciones modelo_adecuaciones)
        {
            //			var ref_repuestos = context.icb_referencia.ToList();
            var ref_repuestos = (from rep in context.icb_referencia
                                 where rep.modulo == "R"
                                 select new
                                 {
                                     rep.ref_codigo,
                                     ref_descripcion = "(" + rep.ref_codigo + ") " + rep.ref_descripcion
                                 }).ToList();

            ViewBag.ref_codigo = new SelectList(ref_repuestos, "ref_codigo", "ref_descripcion");

            var mano_obra = (from man in context.ttempario
                             select new
                             {
                                 man.codigo,
                                 operacion = "(" + man.codigo + ") " + man.operacion
                             }).ToList();

            ViewBag.mo_codigo = new SelectList(mano_obra, "codigo", "operacion");

            #region ocultar

            //if (infocredito != null)
            //{
            //	ViewBag.salario = infocredito.ingsalario;
            //	ViewBag.comisiones = infocredito.ingcomision;
            //	ViewBag.arriendo = infocredito.ingarriendo;
            //	ViewBag.otros = infocredito.ingotros;
            //	ViewBag.prestamo = infocredito.egrprestamo;
            //	ViewBag.tarjetas = infocredito.egrtarjeta;
            //	ViewBag.hipotecas = infocredito.egrhipoteca;
            //	ViewBag.sostenimiento = infocredito.egrsostenimiento;
            //	tercero = infocredito.tercero;
            //	tercero2 = infocredito.tercero2 ?? 0;
            //}

            //var asesores = from u in db.users
            //			   where u.rol_id == 4
            //			   select new
            //			   {
            //				   nombre = u.user_nombre + " " + u.user_apellido,
            //				   u.user_id
            //			   };

            //var listAsesores = new List<SelectListItem>();
            //foreach (var item in asesores)
            //{
            //	listAsesores.Add(new SelectListItem()
            //	{
            //		Text = item.nombre,
            //		Value = item.user_id.ToString()
            //	});
            //}

            //var cliente = (from t in db.icb_terceros
            //			   join tp in db.tercero_cliente
            //			   on t.tercero_id equals tp.tercero_id
            //			   join dir in db.vw_dirtercero
            //			   on t.tercero_id equals dir.idtercero into dtt //into cppcp
            //			   from dir in dtt.DefaultIfEmpty()
            //			   select new
            //			   {
            //				   nombre = t.prinom_tercero != null ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero : t.doc_tercero + t.razon_social,
            //				   t.tercero_id,
            //				   t.email_tercero,
            //				   t.telf_tercero,
            //				   t.celular_tercero,
            //				   dir.direccion
            //			   }).ToList();

            //var lista_cliente = new List<SelectListItem>();
            //foreach (var item in cliente)
            //{
            //	lista_cliente.Add(new SelectListItem()
            //	{
            //		Text = item.nombre,
            //		Value = item.tercero_id.ToString(),
            //		Selected = item.tercero_id == tercero ? true : false
            //	});
            //}

            //var lista_cliente2 = new List<SelectListItem>();
            //foreach (var item in cliente)
            //{
            //	lista_cliente2.Add(new SelectListItem()
            //	{
            //		Text = item.nombre,
            //		Value = item.tercero_id.ToString(),
            //		Selected = item.tercero_id == tercero2 ? true : false
            ////	});
            ////}

            //ViewBag.asesor_id = listAsesores;
            //ViewBag.tercero = lista_cliente;
            //ViewBag.tercero2 = lista_cliente2;

            //var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            // ViewBag.bodega = bodega;

            #endregion


            //var pedidos = (from ped in db.vpedido
            //			   join ped_pg in db.vpedpago
            //			   on ped.id equals ped_pg.idpedido
            //			   where ped_pg.condicion == 1 && !pedidosotro.Contains(ped.id)
            //			   select new
            //			   {
            //				   id = ped.id,
            //				   numero = ped.numero + " - " + (ped.icb_terceros.prinom_tercero != null ? " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.prinom_tercero + " " + ped.icb_terceros.segnom_tercero + " " + ped.icb_terceros.apellido_tercero + " " + ped.icb_terceros.segapellido_tercero : " (" + ped.icb_terceros.doc_tercero + ") " + ped.icb_terceros.razon_social),
            //				   valor = ped_pg.valor,
            //			   }).OrderBy(x => x.id).ToList();
            //pedidos.OrderByDescending(d => d.numero);
            //ViewBag.pedido = new SelectList(pedidos, "id", "numero", v_creditos.pedido);
            //ViewBag.motivodesiste = new SelectList(db.vcredmotdesistio, "id", "motivo", v_creditos.motivodesiste);
            //ViewBag.vehiculo = new SelectList(db.modelo_vehiculo, "modvh_codigo", "modvh_nombre", v_creditos.vehiculo2);
        }

        // GET: InspeccionPeritaje
        public ActionResult InspeccionPeritaje(string placa, int? menu)
        {
            //if (!string.IsNullOrEmpty(placa)) {
            ViewBag.placa = placa;
            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");
            //}
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult InspeccionPeritaje(HttpPostedFileBase[] imagen, int? menu)
        {
            int idsolicitud = Convert.ToInt32(Request["idsolicitud"]);
            int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
            int clasinsper = Convert.ToInt32(Request["clas_insper"]);

            context.icb_encabezado_insp_peritaje.Add(new icb_encabezado_insp_peritaje
            {
                encab_insper_idclasper = clasinsper,
                encab_insper_fecha = DateTime.Now,
                encab_insper_idusuario = idusuario,
                encab_insper_idsolicitud = idsolicitud
            });
            bool result = context.SaveChanges() > 0;
            int idencabezado = context.icb_encabezado_insp_peritaje.OrderByDescending(x => x.encab_insper_id)
                .FirstOrDefault().encab_insper_id;

            #region Seguimiento

            int tercero = Convert.ToInt32(Request["idtercero"]);
            context.seguimientotercero.Add(new seguimientotercero
            {
                idtercero = tercero,
                tipo = 14,
                nota = "Peritaje realizado",
                fecha = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            });

            string placa = Request["placaPeritaje"];
            System.Collections.Generic.List<int> idCotBuscada = context.vcotretoma.Where(x => x.placa == placa).Select(x => x.idcot).ToList();

            System.Collections.Generic.List<int> cotizaciones = (from c in context.icb_cotizacion
                                                                 where idCotBuscada.Contains(c.cot_idserial) && c.id_tercero == tercero
                                                                 select
                                                                     c.cot_idserial
                ).ToList();

            if (cotizaciones != null)
            {
                System.Collections.Generic.List<vcotseguimiento> cotSeguimiento = (from c in context.vcotseguimiento
                                                                                   where cotizaciones.Contains(c.cot_id) && c.tipo_seguimiento == 14
                                                                                   select c).ToList();

                if (cotSeguimiento.Count() == 0)
                {
                    foreach (int item in cotizaciones)
                    {
                        vcotseguimiento seguimiento = new vcotseguimiento
                        {
                            cot_id = Convert.ToInt32(item),
                            fecha = DateTime.Now,
                            responsable = Convert.ToInt32(Session["user_usuarioid"]),
                            Notas = "Peritaje realizado",
                            Motivo = null,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            tipo_seguimiento = 14
                        };
                        context.vcotseguimiento.Add(seguimiento);
                    }
                }
            }

            #endregion

            if (result)
            {
                if (imagen[0] != null && imagen.Length > 0)
                {
                    for (int i = 0; i < imagen.Length; i++)
                    {
                        icb_imagenes_peritaje img = new icb_imagenes_peritaje
                        {
                            idperitaje = idencabezado,
                            imagen = idencabezado + "_" + i + "_" + imagen[i].FileName
                        };
                        context.icb_imagenes_peritaje.Add(img);
                        string path = Server.MapPath("~/Content/img/peritajes/" + idencabezado + "_" + i + "_" +
                                                  imagen[i].FileName);
                        imagen[i].SaveAs(path);
                    }
                }

                System.Collections.Generic.List<icb_piezaperitaje> piezas = context.icb_piezaperitaje.ToList();

                bool flag = false;
                foreach (icb_piezaperitaje id in piezas)
                {
                    string comentario = Request["comentario" + id.pieza_id];
                    string idconvencion = Request["convencion" + id.pieza_id];

                    if (idconvencion != null)
                    {
                        context.icb_insp_peritaje.Add(new icb_insp_peritaje
                        {
                            insp_zona_id = id.zona_id,
                            insp_pieza_id = id.pieza_id,
                            insp_conve_id = Convert.ToInt32(idconvencion),
                            insp_comentario = comentario,
                            insp_encabezado_id = idencabezado
                        });

                        if (flag == false)
                        {
                            int lineasOperacion = Convert.ToInt32(Request["listaOperacionesTabla"]);
                            for (int i = 0; i <= lineasOperacion; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["zona" + i]))
                                {
                                    string idZona = Request["zona" + i];
                                    string idPieza = Request["pieza" + i];
                                    string idOperacion = Request["operacion" + i];
                                    string valor = Request["valorOperacion" + i];

                                    context.pperitajeoperacion.Add(new pperitajeoperacion
                                    {
                                        idZona = Convert.ToInt32(idZona),
                                        idPieza = Convert.ToInt32(idPieza),
                                        idOperacion = idOperacion,
                                        valor = Convert.ToDecimal(valor, miCultura),
                                        idEncabPeritaje = idencabezado,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    });
                                }
                            }

                            flag = true;
                        }

                        bool result2 = context.SaveChanges() > 0;
                        if (result2)
                        {
                            icb_solicitud_peritaje solicitud =
                                context.icb_solicitud_peritaje.FirstOrDefault(x => x.id_peritaje == idsolicitud);
                            solicitud.fecha_inspeccion_peritaje = DateTime.Now;
                            solicitud.atendida = true;
                            context.Entry(solicitud).State = EntityState.Modified;
                            bool result3 = context.SaveChanges() > 0;
                            if (result3)
                            {
                                TempData["mensaje"] = "La inspección de peritajes se ha guardado correctamente";
                                //return RedirectToAction("Aprobar", new { id = idencabezado, menu });
                            }
                            else
                            {
                                TempData["mensaje_error"] = "Error de conexion, intente mas tarde";
                                return View();
                            }
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion, intente mas tarde";
                            return View();
                        }
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error de conexion, intente mas tarde";
                return View();
            }

            BuscarFavoritos(menu);
            return RedirectToAction("Aprobar", new { id = idencabezado, menu });
            //return ViewBag();
        }

        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Aprobar(int? id, int? menu)
        {
            ViewBag.rol = Session["user_rolid"];

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_encabezado_insp_peritaje peritaje = context.icb_encabezado_insp_peritaje.Find(id);
            if (peritaje == null)
            {
                return HttpNotFound();
            }

            System.Collections.Generic.List<icb_imagenes_peritaje> fotos = context.icb_imagenes_peritaje.Where(x => x.idperitaje == id).ToList();
            ViewBag.fotos = fotos;
            ViewBag.total_fotos = fotos.Count();
            BuscarFavoritos(menu);
            return View(peritaje);
        }

        [HttpPost]
        public ActionResult Aprobar(icb_encabezado_insp_peritaje peritaje, int? menu)
        {
            //var fotos = context.icb_imagenes_peritaje.Where(x => x.idperitaje == id).ToList();
            System.Collections.Generic.List<icb_imagenes_peritaje> fotos = peritaje.icb_imagenes_peritaje.ToList();
            ViewBag.fotos = fotos;
            ViewBag.total_fotos = fotos.Count();
            ViewBag.rol = Session["user_rolid"];
            if (ModelState.IsValid)
            {
                peritaje.fec_aprobacion = DateTime.Now;
                peritaje.user_aprobacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(peritaje).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "Peritaje actualizado correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Error al actualizar el peritaje, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(peritaje);
        }

        public ActionResult cargarAdecuaciones(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Modelo_adecuaciones modelo_adecuaciones = new Modelo_adecuaciones
            {
                id_adecuacion = id ?? 0
            };
            //BuscarFavoritos(menu);

            listas(modelo_adecuaciones);

            return View(modelo_adecuaciones);
        }

        [HttpPost]
        public ActionResult cargarAdecuaciones(adecuaciones_peritaje adecuaciones, int? menu)
        {
            string repuestos = Request["seleccionRpt"];
            string[] listaRpt = repuestos.Split(',');
            adecuaciones.fecha = DateTime.Now;
            adecuaciones.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            foreach (string item in listaRpt)
            {
                adecuaciones.referencia_codigo = item;
                adecuaciones.precio_repuesto = Convert.ToInt32(Request["precio+" + item]);
                adecuaciones.detalle = Request["detalle+" + item];
                adecuaciones.cantidad = Convert.ToInt32(Request["cantidad+" + item]);
                context.adecuaciones_peritaje.Add(adecuaciones);
                context.SaveChanges();
            }


            TempData["mensaje"] = "Adecuaciones Agregadas Correctamente";

            int? id = adecuaciones.peritaje_id;

            return RedirectToAction("Aprobar?id=" + adecuaciones.peritaje_id, new { menu });
        }

        public ActionResult BrowserAdecuaciones(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult VerAdecuaciones(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_encabezado_insp_peritaje peritaje = context.icb_encabezado_insp_peritaje.Find(id);
            BuscarFavoritos(menu);
            return View(peritaje);
        }

        public ActionResult Comparativa(int id, string placa)
        {
            ViewBag.id = id;
            ViewBag.placa = placa;
            return View();
        }

        public JsonResult BuscarVehiculoPlacaComparativo(int id, string placa)
        {
            var buscarAnterior = (from a in context.icb_encabezado_insp_peritaje
                                  join b in context.icb_solicitud_peritaje
                                      on a.encab_insper_idsolicitud equals b.id_peritaje
                                  join c in context.icb_tercero_vhretoma
                                      on b.id_tercero_vhretoma equals c.veh_id
                                  join d in context.icb_clas_peritaje
                                      on a.encab_insper_clasificacion equals d.claper_id into ps
                                  from d in ps.DefaultIfEmpty()
                                  where c.placa == placa && a.encab_insper_id != id
                                  select new
                                  {
                                      fecha = a.encab_insper_fecha,
                                      id = a.encab_insper_id,
                                      solicitud = b.id_peritaje,
                                      a.estado,
                                      clasificacion = d.claper_nombre
                                  }).OrderByDescending(x => x.fecha).FirstOrDefault();

            var buscarNuevo = (from a in context.icb_encabezado_insp_peritaje
                               join b in context.icb_solicitud_peritaje
                                   on a.encab_insper_idsolicitud equals b.id_peritaje
                               join c in context.icb_tercero_vhretoma
                                   on b.id_tercero_vhretoma equals c.veh_id
                               join d in context.icb_clas_peritaje
                                   on a.encab_insper_clasificacion equals d.claper_id into ps
                               from d in ps.DefaultIfEmpty()
                               where c.placa == placa
                               select new
                               {
                                   fecha = a.encab_insper_fecha,
                                   id = a.encab_insper_id,
                                   solicitud = b.id_peritaje,
                                   a.estado,
                                   clasificacion = d.claper_nombre
                               }).OrderByDescending(x => x.fecha).FirstOrDefault();


            int estadoPlaca = 0;
            var vehiculo = from sl in context.icb_solicitud_peritaje
                           join vh in context.icb_tercero_vhretoma
                               on sl.id_tercero_vhretoma equals vh.veh_id
                           join tercero in context.icb_terceros
                               on vh.tercero_id equals tercero.tercero_id
                           where vh.placa == placa
                                 && sl.fecha_inspeccion_peritaje == null
                           select new
                           {
                               sl.id_peritaje,
                               vh.tercero_id,
                               prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                               segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                               apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                               segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                               telf_tercero = tercero.telf_tercero != null ? tercero.telf_tercero : "",
                               celular_tercero = tercero.celular_tercero != null ? tercero.celular_tercero : "",
                               email_tercero = tercero.email_tercero != null ? tercero.email_tercero : "",
                               vh.placa,
                               vh.modelo_codigo,
                               vh.tp_vehiculo,
                               vh.color,
                               vh.serie,
                               vh.cilindraje,
                               vh.tp_motor,
                               vh.numero_motor
                           };

            if (vehiculo != null)
            {
                estadoPlaca = 1;
            }

            System.Collections.Generic.List<icb_conveperitaje> conven = context.icb_conveperitaje.ToList();
            var listaConven = conven.Select(x => new { x.conve_id, x.conve_nombre, x.pieza_id });

            System.Collections.Generic.List<icb_clas_peritaje> clasinper = context.icb_clas_peritaje.ToList();
            var listaClasinsper = clasinper.Select(x => new
            {
                x.claper_id,
                x.claper_nombre
            });

            System.Collections.Generic.List<GetFormularioPeritaje_Result> result = context.GetFormularioPeritaje().ToList();
            int total = result.Count;
            var listaZonas = result.Select(x => new
            {
                x.zonaper_id,
                x.zonaper_nombre,
                x.pieza_id,
                x.pieza_nombre,
                x.cuantas_piezas,
                x.cuantas_conveciones
            });

            int idperitaje = id == null ? 0 : id;
            var peritaje = from i in context.icb_insp_peritaje
                           where i.insp_encabezado_id == idperitaje
                           select new
                           {
                               pieza = i.insp_pieza_id,
                               convencion = i.icb_conveperitaje.conve_nombre,
                               comentario = i.insp_comentario
                           };

            var dataActual = new
            {
                estadoPlaca,
                vehiculo,
                listaZonas,
                total,
                listaConven,
                listaClasinsper,
                peritaje
            };

            int estadoPlaca2 = 0;
            var vehiculo2 = from sl in context.icb_solicitud_peritaje
                            join vh in context.icb_tercero_vhretoma
                                on sl.id_tercero_vhretoma equals vh.veh_id
                            join tercero in context.icb_terceros
                                on vh.tercero_id equals tercero.tercero_id
                            where sl.id_peritaje == buscarAnterior.solicitud
                                  && sl.fecha_inspeccion_peritaje == null
                            select new
                            {
                                sl.id_peritaje,
                                vh.tercero_id,
                                prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                                apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                                telf_tercero = tercero.telf_tercero != null ? tercero.telf_tercero : "",
                                celular_tercero = tercero.celular_tercero != null ? tercero.celular_tercero : "",
                                email_tercero = tercero.email_tercero != null ? tercero.email_tercero : "",
                                vh.placa,
                                vh.modelo_codigo,
                                vh.tp_vehiculo,
                                vh.color,
                                vh.serie,
                                vh.cilindraje,
                                vh.tp_motor,
                                vh.numero_motor
                            };

            if (vehiculo2 != null)
            {
                estadoPlaca2 = 1;
            }

            System.Collections.Generic.List<icb_conveperitaje> conven2 = context.icb_conveperitaje.ToList();
            var listaConven2 = conven2.Select(x => new { x.conve_id, x.conve_nombre, x.pieza_id });

            System.Collections.Generic.List<icb_clas_peritaje> clasinper2 = context.icb_clas_peritaje.ToList();
            var listaClasinsper2 = clasinper.Select(x => new
            {
                x.claper_id,
                x.claper_nombre
            });

            System.Collections.Generic.List<GetFormularioPeritaje_Result> result2 = context.GetFormularioPeritaje().ToList();
            int total2 = result2.Count;
            var listaZonas2 = result2.Select(x => new
            {
                x.zonaper_id,
                x.zonaper_nombre,
                x.pieza_id,
                x.pieza_nombre,
                x.cuantas_piezas,
                x.cuantas_conveciones
            });

            int idperitaje2 = id == null ? 0 : id;
            var peritaje2 = from i in context.icb_insp_peritaje
                            where i.insp_encabezado_id == idperitaje2
                            select new
                            {
                                pieza = i.insp_pieza_id,
                                convencion = i.icb_conveperitaje.conve_nombre,
                                comentario = i.insp_comentario
                            };

            var dataAntiguo = new
            {
                estadoPlaca2,
                vehiculo2,
                listaZonas2,
                total2,
                listaConven2,
                listaClasinsper2,
                peritaje2
            };

            var data = new
            {
                dataActual,
                dataAntiguo,
                fechaAntiguo = buscarAnterior.fecha.ToString("yyyy/MM/dd HH:mm"),
                estadoAntiguo = buscarAnterior.estado != null ? buscarAnterior.estado : "Sin estado",
                clasificacionAntiguo = buscarAnterior.clasificacion != null
                    ? buscarAnterior.clasificacion
                    : "Sin clasificación",
                fechaNuevo = buscarNuevo.fecha.ToString("yyyy/MM/dd HH:mm"),
                estadoNuevo = buscarNuevo.estado != null ? buscarNuevo.estado : "Sin estado",
                clasificacionNuevo = buscarNuevo.clasificacion != null ? buscarNuevo.clasificacion : "Sin clasificación"
            };


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarVehiculoPlaca(string placa, int? id)
        {
            int estadoPlaca = 0;
            var vehiculo = from sl in context.icb_solicitud_peritaje
                           join vh in context.icb_tercero_vhretoma
                               on sl.id_tercero_vhretoma equals vh.veh_id
                           join tercero in context.icb_terceros
                               on vh.tercero_id equals tercero.tercero_id
                           where vh.placa == placa
                                 && sl.fecha_inspeccion_peritaje == null
                           select new
                           {
                               sl.id_peritaje,
                               vh.tercero_id,
                               prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                               segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                               apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                               segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                               telf_tercero = tercero.telf_tercero != null ? tercero.telf_tercero : "",
                               celular_tercero = tercero.celular_tercero != null ? tercero.celular_tercero : "",
                               email_tercero = tercero.email_tercero != null ? tercero.email_tercero : "",
                               vh.placa,
                               vh.modelo_codigo,
                               vh.tp_vehiculo,
                               vh.color,
                               vh.serie,
                               vh.cilindraje,
                               vh.tp_motor,
                               vh.numero_motor
                           };

            if (vehiculo != null)
            {
                estadoPlaca = 1;
            }

            System.Collections.Generic.List<icb_conveperitaje> conven = context.icb_conveperitaje.ToList();
            var listaConven = conven.Select(x => new { x.conve_id, x.conve_nombre, x.pieza_id });

            System.Collections.Generic.List<icb_clas_peritaje> clasinper = context.icb_clas_peritaje.ToList();
            var listaClasinsper = clasinper.Select(x => new
            {
                x.claper_id,
                x.claper_nombre
            });

            System.Collections.Generic.List<GetFormularioPeritaje_Result> result = context.GetFormularioPeritaje().ToList();
            int total = result.Count;
            var listaZonas = result.Select(x => new
            {
                x.zonaper_id,
                x.zonaper_nombre,
                x.pieza_id,
                x.pieza_nombre,
                x.cuantas_piezas,
                x.cuantas_conveciones
            });

            int? idperitaje = id == null ? 0 : id;
            var peritaje = from i in context.icb_insp_peritaje
                           where i.insp_encabezado_id == idperitaje
                           select new
                           {
                               pieza = i.insp_pieza_id,
                               convencion = i.icb_conveperitaje.conve_nombre,
                               comentario = i.insp_comentario
                           };

            var data = new
            {
                estadoPlaca,
                vehiculo,
                listaZonas,
                total,
                listaConven,
                listaClasinsper,
                peritaje
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPiezaXzona(int zona)
        {
            System.Collections.Generic.List<GetFormularioPeritaje_Result> sp = context.GetFormularioPeritaje().ToList();
            var piezas = sp.Where(x => x.zonaper_id == zona).Select(x => new { id = x.pieza_id, nombre = x.pieza_nombre })
                .ToList();

            return Json(piezas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarOperacionesPeritaje(int id)
        {
            var info = (from pp in context.pperitajeoperacion
                        join z in context.icb_zonaperitaje
                            on pp.idZona equals z.zonaper_id
                        join p in context.icb_piezaperitaje
                            on pp.idPieza equals p.pieza_id
                        join o in context.ttempario
                            on pp.idOperacion equals o.codigo
                        where pp.idEncabPeritaje == id
                        select new
                        {
                            z.zonaper_nombre,
                            p.pieza_nombre,
                            o.operacion,
                            pp.valor
                        }).ToList();

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRepuestos()
        {
            var data = from i in context.icb_referencia
                       where i.modulo == "R"
                       select new
                       {
                           codigo = i.ref_codigo,
                           nombre = i.ref_descripcion,
                           precio = i.ref_valor_unitario
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRepuestoDatos(string refe)
        {
            var data2 = (from i in context.icb_referencia
                         join p in context.rprecios
                             on i.ref_codigo equals p.codigo into al1
                         from p in al1.DefaultIfEmpty()
                         where i.modulo == "R" && i.ref_codigo == refe
                         select new
                         {
                             codigo = i.ref_codigo,
                             descripcion = i.ref_descripcion,
                             precio = p.precio1 != null ? p.precio1 : 0,
                             iva = i.por_iva,
                             dct = i.por_dscto,
                             nombre = "(" + i.ref_codigo + ") " + i.ref_descripcion
                         }).ToList();

            var data = data2.Select(c => new
            {
                c.codigo,
                c.descripcion,
                precio2 = c.precio,
                precio = c.precio != null ? c.precio.ToString("N0") : "",
                iva2 = c.iva,
                iva = c.iva != null ? c.iva.ToString("N0") : "",
                dct2 = c.dct,
                dct = c.dct != null ? c.dct.ToString("N0") : "",
                c.nombre
            }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarManoDatos(string codigo)
        {
            var data2 = (from i in context.ttempario
                         join va in context.codigo_iva
                             on i.iva equals va.id
                         where i.codigo == codigo
                         select new
                         {
                             i.codigo,
                             descripcion = i.operacion,
                             precio = i.precio != null ? i.precio : 0,
                             iva = va.porcentaje,
                             //dct = i.por_dscto != null ? i.por_dscto : 0,
                             nombre = "(" + i.codigo + ") " + i.operacion
                         }).ToList();

            var data = data2.Select(c => new
            {
                c.codigo,
                c.descripcion,
                precio2 = c.precio,
                precio = c.precio != null ? c.precio.Value.ToString("N0") : "",
                iva2 = c.iva,
                iva = c.iva != null ? c.iva.Value.ToString("N0") : "",
                //dct = i.por_dscto != null ? i.por_dscto : 0,
                nombre = "(" + c.codigo + ") " + c.descripcion
            }).FirstOrDefault();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosBrowser()
        {
            var data2 = (from i in context.icb_encabezado_insp_peritaje
                         select new
                         {
                             i.icb_solicitud_peritaje.icb_tercero_vhretoma.placa,
                             clasificacion = i.icb_clas_peritaje.claper_nombre,
                             perito = i.users.user_nombre + " " + i.users.user_apellido,
                             fecha = i.encab_insper_fecha,
                             estado = i.estado != null ? i.estado : "",
                             i.fec_aprobacion,
                             id = i.encab_insper_id
                         }).OrderByDescending(i => i.fecha).ToList();


            var agrupar = data2.GroupBy(x => x.placa).Select(grp => new
            {
                fechaMayor = grp.Max(x => x.fecha),
                id = grp.Select(x => x.id).Max(),
                contar = grp.Count(),
                placa = grp.Key
            }).Where(x => x.contar > 1).ToList();

            var data = data2.Select(x => new
            {
                x.placa,
                x.clasificacion,
                x.perito,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm"),
                x.estado,
                fec_aprobacion = x.fec_aprobacion != null ? x.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm") : "",
                x.id
            });

            return Json(new { data, agrupar }, JsonRequestBehavior.AllowGet);
        }

        //Browser adecuaciones
        public JsonResult BuscarDatosAdecuaciones()
        {
            var data = (from i in context.icb_encabezado_insp_peritaje
                        join a in context.adecuaciones_peritaje
                            on i.encab_insper_id equals a.peritaje_id into al2
                        from a in al2.DefaultIfEmpty()
                        select new
                        {
                            i.icb_solicitud_peritaje.icb_tercero_vhretoma.placa,
                            clasificacion = i.icb_clas_peritaje.claper_nombre,
                            usuario = a.users.user_nombre + " " + a.users.user_apellido,
                            fecha = a.fecha.ToString(),
                            a.id,
                            peritaje_id = i.encab_insper_id,
                            valor_total = a.valor_total_adecuacion
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //Datos adecuaciones
        public JsonResult BuscarAdecuaciones(int peritaje_id)
        {
            var data = from a in context.adecuaciones_peritaje
                       where a.peritaje_id == peritaje_id
                       select new
                       {
                           codigo = a.referencia_codigo,
                           a.precio_repuesto,
                           usuario = a.users.user_nombre + " " + a.users.user_apellido,
                           fecha = a.fecha.ToString(),
                           a.cantidad,
                           mano_obra = a.mano_de_obra_id,
                           precio_mo = a.mano_obra_precio,
                           a.detalle,
                           valor_total = a.valor_total_adecuacion
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult peritajeUsados(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult ResultadoPeritaje(int? id, int? menu)
        {
            ViewBag.rol = Session["user_rolid"];

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_encabezado_insp_peritaje peritaje = context.icb_encabezado_insp_peritaje.Find(id);
            if (peritaje == null)
            {
                return HttpNotFound();
            }

            System.Collections.Generic.List<icb_imagenes_peritaje> fotos = context.icb_imagenes_peritaje.Where(x => x.idperitaje == id).ToList();
            ViewBag.fotos = fotos;
            ViewBag.total_fotos = fotos.Count();
            BuscarFavoritos(menu);
            return View(peritaje);
        }

        public JsonResult BuscarPeritajeUsados()
        {
            string par = "P32";
            string valorParametro = context.icb_sysparameter.Where(x => x.syspar_cod == par).Select(x => x.syspar_value)
                .FirstOrDefault();

            int valorPar = Convert.ToInt32(valorParametro);

            var data = (from i in context.icb_encabezado_insp_peritaje
                        join s in context.icb_solicitud_peritaje
                            on i.encab_insper_idsolicitud equals s.id_peritaje
                        join r in context.icb_tercero_vhretoma
                            on s.id_tercero_vhretoma equals r.veh_id
                        join t in context.icb_terceros
                            on r.tercero_id equals t.tercero_id
                        join vi in context.vw_inventario_hoy
                            on r.placa equals vi.ref_codigo
                        join m in context.modelo_vehiculo
                            on r.modelo_codigo equals m.modvh_codigo
                        where /*vi.stock > 0 &&*/ DbFunctions.DiffDays(i.encab_insper_fecha, DateTime.Now) < valorPar
                        select new
                        {
                            i.icb_solicitud_peritaje.icb_tercero_vhretoma.placa,
                            perito = i.users.user_nombre + " " + i.users.user_apellido,
                            cliente = t.prinom_tercero + " " + t.segapellido_tercero + " " + t.apellido_tercero + " " +
                                      t.segapellido_tercero,
                            fecha = i.encab_insper_fecha.ToString(),
                            estado = i.estado != null ? i.estado : "",
                            vehiculo = m.modvh_nombre,
                            adecuaciones = i.adecuaciones != null ? i.adecuaciones : "",
                            id = i.encab_insper_id
                        }).OrderByDescending(i => i.fecha);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregaAdecuacionMano(int? peritajeid, string codigo, decimal precio, int cantidad,
            decimal val_total, float? poriva, decimal? mtoiva, float? pordes, decimal? mtodes)
        {
            //var repuestos = Request["seleccionRpt"];
            //var listaRpt = repuestos.Split(',');
            DateTime hoy = DateTime.Now;
            int user_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            adecuaciones_peritaje buscarCodigo =
                context.adecuaciones_peritaje.FirstOrDefault(x =>
                    x.peritaje_id == peritajeid && x.mano_de_obra_id == codigo);
            if (buscarCodigo == null)
            {
                adecuaciones_peritaje adecuacion = new adecuaciones_peritaje
                {
                    peritaje_id = peritajeid,
                    mano_de_obra_id = codigo,
                    fecha = hoy,
                    mano_obra_precio = precio,
                    cantidad = cantidad,
                    user_creacion = user_creacion,
                    valor_total_adecuacion = val_total
                };
                if (poriva != null)
                {
                    adecuacion.poriva = poriva;
                }

                if (mtoiva != null)
                {
                    adecuacion.valoriva = mtoiva;
                }

                if (pordes != null)
                {
                    adecuacion.pordescuento = pordes;
                }

                if (mtodes != null)
                {
                    adecuacion.valordescuento = mtodes;
                }

                context.adecuaciones_peritaje.Add(adecuacion);
                context.SaveChanges();
            }


            TempData["mensaje"] = "Mano de Obra Agregadas Correctamente";

            var datos2 = (from i in context.adecuaciones_peritaje
                          join tem in context.ttempario
                              on i.mano_de_obra_id equals tem.codigo into al3
                          from tem in al3.DefaultIfEmpty()
                          where i.peritaje_id == peritajeid && i.referencia_codigo == null
                          select new
                          {
                              i.id,
                              i.peritaje_id,
                              i.mano_de_obra_id,
                              i.fecha,
                              i.mano_obra_precio,
                              i.cantidad,
                              i.valor_total_adecuacion,
                              tem.operacion,
                              i.poriva,
                              i.valoriva,
                              i.pordescuento,
                              i.valordescuento
                          }).ToList();
            var data2 = datos2.Select(c => new
            {
                c.id,
                c.peritaje_id,
                c.mano_de_obra_id,
                fecha = c.fecha != null ? c.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                mano_obra_precio = c.mano_obra_precio != null ? c.mano_obra_precio.Value.ToString("N0") : "",
                cantidad = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "",
                valor_total_adecuacion =
                    c.valor_total_adecuacion != null ? c.valor_total_adecuacion.Value.ToString("N0") : "",
                c.operacion,
                nombre_mano = "(" + c.mano_de_obra_id + ") " + c.operacion,
                poriva = c.poriva != null ? c.poriva.Value.ToString("N0") : "",
                valoriva = c.valoriva != null ? c.valoriva.Value.ToString("N0") : "",
                pordescuento = c.pordescuento != null ? c.pordescuento.Value.ToString("N0") : "",
                valordescuento = c.valordescuento != null ? c.valordescuento.Value.ToString("N0") : ""
            }).ToList();

            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregaAdecuacionRepuesto(int? peritajeid, string referencia_codigo, decimal precio_repuesto,
            int cantidad, decimal val_total, float? poriva, decimal? mtoiva, float? pordes, decimal? mtodes)
        {
            //var repuestos = Request["seleccionRpt"];
            //var listaRpt = repuestos.Split(',');
            DateTime hoy = DateTime.Now;
            int user_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            adecuaciones_peritaje buscarCodigo = context.adecuaciones_peritaje.FirstOrDefault(x =>
                x.peritaje_id == peritajeid && x.referencia_codigo == referencia_codigo);
            if (buscarCodigo == null)
            {
                adecuaciones_peritaje adecuacion = new adecuaciones_peritaje
                {
                    peritaje_id = peritajeid,
                    referencia_codigo = referencia_codigo,
                    fecha = hoy,
                    precio_repuesto = precio_repuesto,
                    cantidad = cantidad,
                    user_creacion = user_creacion,
                    valor_total_adecuacion = val_total
                };
                if (poriva != null)
                {
                    adecuacion.poriva = poriva;
                }

                if (mtoiva != null)
                {
                    adecuacion.valoriva = mtoiva;
                }

                if (pordes != null)
                {
                    adecuacion.pordescuento = pordes;
                }

                if (mtodes != null)
                {
                    adecuacion.valordescuento = mtodes;
                }

                context.adecuaciones_peritaje.Add(adecuacion);
                context.SaveChanges();
            }
            else
            {
                int AcumCant = 0;
                AcumCant = buscarCodigo.cantidad ?? 0;
                AcumCant = AcumCant + cantidad;

                //decimal  Wprecio = 0;
                //Wprecio = buscarCodigo.precio_repuesto??0;

                decimal AcumTotal = 0;
                AcumTotal = buscarCodigo.valor_total_adecuacion ?? 0;
                AcumTotal = AcumTotal + val_total;

                buscarCodigo.cantidad = AcumCant;
                buscarCodigo.valor_total_adecuacion = AcumTotal;

                context.Entry(buscarCodigo).State = EntityState.Modified;
                context.SaveChanges();
            }


            TempData["mensaje"] = "Adecuaciones de Repuestos Agregadas Correctamente";

            var datos = (from i in context.adecuaciones_peritaje
                         join rep in context.icb_referencia
                             on i.referencia_codigo equals rep.ref_codigo into al4
                         from rep in al4.DefaultIfEmpty()
                         where i.peritaje_id == peritajeid && i.mano_de_obra_id == null
                         select new
                         {
                             i.id,
                             i.peritaje_id,
                             i.referencia_codigo,
                             i.fecha,
                             i.precio_repuesto,
                             i.cantidad,
                             i.valor_total_adecuacion,
                             rep.ref_descripcion,
                             i.poriva,
                             i.valoriva,
                             i.pordescuento,
                             i.valordescuento
                         }).ToList();
            var data = datos.Select(c => new
            {
                c.id,
                c.peritaje_id,
                c.referencia_codigo,
                fecha = c.fecha != null ? c.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                precio_repuesto = c.precio_repuesto != null ? c.precio_repuesto.Value.ToString("N0") : "",
                cantidad = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "",
                valor_total_adecuacion =
                    c.valor_total_adecuacion != null ? c.valor_total_adecuacion.Value.ToString("N0") : "",
                c.ref_descripcion,
                nombre_repuesto = "(" + c.referencia_codigo + ") " + c.ref_descripcion,
                poriva = c.poriva != null ? c.poriva.Value.ToString("N0") : "",
                valoriva = c.valoriva != null ? c.valoriva.Value.ToString("N0") : "",
                pordescuento = c.pordescuento != null ? c.pordescuento.Value.ToString("N0") : "",
                valordescuento = c.valordescuento != null ? c.valordescuento.Value.ToString("N0") : ""
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefrescaAdecuacionRepuesto(int? peritajeid)
        {
            if (peritajeid != null)
            {
                var datos = (from i in context.adecuaciones_peritaje
                             join rep in context.icb_referencia
                                 on i.referencia_codigo equals rep.ref_codigo into al
                             from rep in al.DefaultIfEmpty()
                             where i.peritaje_id == peritajeid && i.mano_de_obra_id == null
                             select new
                             {
                                 i.id,
                                 i.peritaje_id,
                                 i.referencia_codigo,
                                 i.fecha,
                                 i.precio_repuesto,
                                 i.cantidad,
                                 i.valor_total_adecuacion,
                                 rep.ref_descripcion,
                                 i.poriva,
                                 i.valoriva,
                                 i.pordescuento,
                                 i.valordescuento
                             }).ToList();
                var data = datos.Select(c => new
                {
                    c.id,
                    c.peritaje_id,
                    c.referencia_codigo,
                    fecha = c.fecha != null ? c.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    precio_repuesto = c.precio_repuesto != null ? c.precio_repuesto.Value.ToString("N0") : "",
                    cantidad = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "",
                    valor_total_adecuacion = c.valor_total_adecuacion != null
                        ? c.valor_total_adecuacion.Value.ToString("N0")
                        : "",
                    c.ref_descripcion,
                    nombre_repuesto = "(" + c.referencia_codigo + ") " + c.ref_descripcion,
                    poriva = c.poriva != null ? c.poriva.Value.ToString("N0") : "",
                    valoriva = c.valoriva != null ? c.valoriva.Value.ToString("N0") : "",
                    pordescuento = c.pordescuento != null ? c.pordescuento.Value.ToString("N0") : "",
                    valordescuento = c.valordescuento != null ? c.valordescuento.Value.ToString("N0") : ""
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefrescaAdecuacionMano(int? peritajeid)
        {
            if (peritajeid != null)
            {
                var datos = (from i in context.adecuaciones_peritaje
                             join tem in context.ttempario
                                 on i.mano_de_obra_id equals tem.codigo into al5
                             from tem in al5.DefaultIfEmpty()
                             where i.peritaje_id == peritajeid && i.referencia_codigo == null
                             select new
                             {
                                 i.id,
                                 i.peritaje_id,
                                 i.mano_de_obra_id,
                                 i.fecha,
                                 i.mano_obra_precio,
                                 i.cantidad,
                                 i.valor_total_adecuacion,
                                 tem.operacion,
                                 i.poriva,
                                 i.valoriva,
                                 i.pordescuento,
                                 i.valordescuento
                             }).ToList();
                var data = datos.Select(c => new
                {
                    c.id,
                    c.peritaje_id,
                    c.mano_de_obra_id,
                    fecha = c.fecha != null ? c.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    mano_obra_precio = c.mano_obra_precio != null ? c.mano_obra_precio.Value.ToString("N0") : "",
                    cantidad = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "",
                    valor_total_adecuacion = c.valor_total_adecuacion != null
                        ? c.valor_total_adecuacion.Value.ToString("N0")
                        : "",
                    c.operacion,
                    nombre_mano = "(" + c.mano_de_obra_id + ") " + c.operacion,
                    poriva = c.poriva != null ? c.poriva.Value.ToString("N0") : "",
                    valoriva = c.valoriva != null ? c.valoriva.Value.ToString("N0") : "",
                    pordescuento = c.pordescuento != null ? c.pordescuento.Value.ToString("N0") : "",
                    valordescuento = c.valordescuento != null ? c.valordescuento.Value.ToString("N0") : ""
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SumaAdecuacion(int? peritajeid)
        {
            if (peritajeid != null)
            {
                decimal? sumaTodo = (from i in context.adecuaciones_peritaje
                                     where i.peritaje_id == peritajeid
                                     select i.valor_total_adecuacion).Sum();


                return Json(sumaTodo, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarElemento(int idDetalle)
        {
            adecuaciones_peritaje buscarElemento = context.adecuaciones_peritaje.FirstOrDefault(x => x.id == idDetalle);
            if (buscarElemento != null)
            {
                context.Entry(buscarElemento).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarHistoricoVehiculo(string placa)
        {
            var info = (from v in context.vw_historialtaller
                        where v.placa == placa
                        select new
                        {
                            v.orden_taller,
                            v.placa,
                            v.ref_descripcion,
                            v.fecha,
                            v.razoz_ingreso,
                            v.kilometraje,
                            v.Cliente
                        }).ToList();

            var data = info.Select(x => new
            {
                orden = x.orden_taller,
                x.placa,
                modelo = x.ref_descripcion != null ? x.ref_descripcion : "",
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("is-IS")),
                razon = x.razoz_ingreso != null ? x.razoz_ingreso : "",
                x.kilometraje,
                cliente = x.Cliente
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
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