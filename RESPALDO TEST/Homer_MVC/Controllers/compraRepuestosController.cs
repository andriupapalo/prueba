using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers

//se verifico el funcionamiento el 27/06/2018 y estuvo ok
{
    public class compraRepuestosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        //clase para traer solicitados
        public class listadosolicitudes
        {
            public string ref_codigo { get; set; }
            public string ref_descripcion { get; set; }
            public string tipo_compra { get; set; }
            public int idtipo_compra { get; set; }
            public int pedido { get; set; }
            public string notas { get; set; }
            public string archivo_pedido { get; set; }
            public string npedido_gm { get; set; }
            public int? cantidad_total { get; set; }
            public List<int> ref_detalle_solicitudes { get; set; }
        }
        private static Expression<Func<vw_browser_pedidos_firme, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_pedidos_firme), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_pedidos_firme, string>> lambda = Expression.Lambda<Func<vw_browser_pedidos_firme, string>>(menuProperty, menu);

            return lambda;
        }
        // GET: compraRepuestos 
        public ActionResult Index(int? menu)
        {
            var tpdoc = from td in context.tp_doc_registros
                        where td.sw == 1
                        select new
                        {
                            td.tpdoc_nombre,
                            td.tpdoc_id
                        };

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in tpdoc)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.tpdoc_nombre,
                    Value = item.tpdoc_id.ToString()
                });
            }

            ViewBag.tipo = lista;
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "bodccs_cod", "bodccs_nombre");
            ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        //cargue archivo
        [HttpPost]
        public ActionResult ImportarTxt(HttpPostedFileBase txtfile, int? menu)
        {
            try
            {
                string path = Server.MapPath("~/Content/" + txtfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtfile.SaveAs(path);

                StreamReader objReader = new StreamReader(path);
                string linea = "";
                ArrayList arrText = new ArrayList();
                bool encabezado = true;

                while (linea != null)
                {
                    linea = objReader.ReadLine();
                    if (encabezado == false)
                    {
                        if (linea != null)
                        {
                            arrText.Add(linea);
                        }
                    }

                    encabezado = false;
                }

                objReader.Close();

                foreach (string item in arrText)
                {
                    string prueba = item.Substring(47, 5).Trim();
                    if (string.IsNullOrEmpty(item.Substring(47, 5).Trim()))
                    {
                        encab_documento encab = new encab_documento
                        {
                            tipo = Convert.ToInt32(Request["tipo"])
                        };
                        icb_doc_consecutivos doc_conse =
                            context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == encab.tipo);
                        long consecutivo = doc_conse.doccons_siguiente;
                        encab.numero = consecutivo + 1;
                        encab.nit = Convert.ToInt32(Request["nit"]);
                        DateTime fecha = new DateTime(Convert.ToInt32(item.Substring(26, 4)),
                            Convert.ToInt32(item.Substring(30, 2)), Convert.ToInt32(item.Substring(32, 2)));
                        encab.fecha = fecha;
                        encab.fpago_id = Convert.ToInt32(Request["fpago_id"]);
                        fpago_tercero fpago = context.fpago_tercero.FirstOrDefault(x => x.fpago_id == encab.fpago_id);
                        int? dias_vencimiento = fpago != null ? fpago.dvencimiento : 0;
                        encab.vencimiento = fecha.AddDays(dias_vencimiento ?? 0);
                        encab.valor_total = Convert.ToInt32(item.Substring(40, 6));
                        string costo = item.Substring(47, 26).Trim();
                        encab.costo = Convert.ToInt32(costo);
                        encab.documento = item.Substring(15, 10);
                        encab.bodega = Convert.ToInt32(Request["bodega"]);
                        encab.numorden = item.Substring(88, 5);
                        encab.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        encab.fec_creacion = DateTime.Now;
                        encab.estado = true;
                        encab.impoconsumo = 0;
                        context.encab_documento.Add(encab);

                        try
                        {
                            context.SaveChanges();
                            doc_conse.doccons_siguiente = consecutivo + 1;
                            context.Entry(doc_conse).State = EntityState.Modified;
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                {
                                    string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity,
                                        validationError.ErrorMessage);
                                    // raise a new exception nesting
                                    // the current instance as InnerException
                                    raise = new InvalidOperationException(message, raise);
                                }
                            }

                            throw raise;
                        }
                    }

                    Console.WriteLine(item);
                    Console.ReadLine();
                }

                TempData["mensaje"] = "El archivo se ha cargado correctamente";
                return RedirectToAction("Index", "compraRepuestos", new { menu });
            }
            catch (Exception)
            {
                TempData["mensajeError"] = "Error al leer el archivo, verifique que el archivo sea el correcto";
                return RedirectToAction("Index", "compraRepuestos", new { menu });
            }
        }

        public ActionResult preCargue(HttpPostedFileBase txtPreCargue, int? menu)
        {
            try
            {
                string path = Server.MapPath("~/Content/" + txtPreCargue.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtPreCargue.SaveAs(path);

                StreamReader objReader = new StreamReader(path);
                string linea = "";
                ArrayList arrText = new ArrayList();
                bool encabezado = true;

                while (linea != null)
                {
                    linea = objReader.ReadLine();
                    if (encabezado == false)
                    {

                        if (linea != null)
                        {
                            if (linea.Length > 0)
                            {
                                arrText.Add(linea);
                            }
                        }
                    }

                    encabezado = false;
                }

                objReader.Close();

                int bodega_actual = Convert.ToInt32(Session["user_bodega"]);
                string codigo_conse = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P28").syspar_value;
                int valor = Convert.ToInt32(codigo_conse);

                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == valor && x.bodega_id == bodega_actual);
                if (grupo == null)
                {
                    TempData["mensaje_error"] = "No hay consecutivos parametrizados para este documento";
                    return RedirectToAction("Index", "compraRepuestos", new { menu });
                }

                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                if (consecutivo < 0)
                {
                    TempData["mensaje_error"] = "No hay consecutivos parametrizados para este documento";
                    return RedirectToAction("Index", "compraRepuestos", new { menu });
                }

                DateTime fecha_creacion = DateTime.Now;
                string pedido_interno = "";
                string pedidoGM = "";

                foreach (string item in arrText)
                {
                    int longitud = item.Length;
                    if (longitud == 93 || longitud == 147)
                    {
                        if (string.IsNullOrEmpty(item.Substring(47, 5).Trim()))
                        {
                            rprecarga precarga = new rprecarga
                            {
                                numero = consecutivo
                            };
                            DateTime fecha = new DateTime(Convert.ToInt32(item.Substring(26, 4)),
                                Convert.ToInt32(item.Substring(30, 2)), Convert.ToInt32(item.Substring(32, 2)));
                            precarga.fecha = fecha;
                            precarga.valor_totalenca = Convert.ToInt32(item.Substring(39, 7));
                            string costo = item.Substring(47, 26).Trim();
                            precarga.documento = item.Substring(15, 10);
                            precarga.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            precarga.fec_creacion = fecha_creacion;
                            precarga.estado = true;
                            pedido_interno = item.Substring(88, 5);
                            precarga.pedidoint = pedido_interno;
                            context.rprecarga.Add(precarga);
                        }
                        else
                        {
                            rprecarga precarga = new rprecarga
                            {
                                codigo = item.Substring(53, 18)
                            };
                            int i = 0;
                            if (item.Substring(53, 5) == "00000")
                            {
                                while (item.Substring(53 + i, 1) == "0")
                                {
                                    i++;
                                    precarga.codigo = item.Substring(53 + i, 18 - i);
                                }
                            }

                            precarga.numero = consecutivo;
                            int seq = Convert.ToInt32(item.Substring(52, 1));
                            precarga.seq = seq;
                            precarga.fec_creacion = fecha_creacion;
                            //var refe = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == item.Substring(52, 18));
                            //var iva = refe != null ? refe.por_iva : null;
                            //precarga.poriva = iva;
                            string cantpe = item.Substring(91, 4).Trim();
                            precarga.cant_ped = Convert.ToInt32(item.Substring(91, 4).Trim());
                            precarga.cant_fact = Convert.ToInt32(item.Substring(121, 4).Trim());
                            string valor_unitario = item.Substring(item.Length - 10, 10).Trim();
                            precarga.valor_unitario = Convert.ToInt32(valor_unitario);
                            precarga.pedidoint = pedido_interno;
                            pedidoGM = item.Substring(21, 10);
                            precarga.pedidogm = pedidoGM;
                            precarga.valor_total = Convert.ToInt32(item.Substring(item.Length - 22, 12).Trim());
                            context.rprecarga.Add(precarga);
                        }
                    }
                    else if (longitud == 0)
                    {

                    }
                    else
                    {
                        TempData["mensajeError"] = "Error al leer el archivo, verifique que el archivo sea el correcto.";
                        TempData["lineaError"] = "Línea [" + longitud + "]\" " + item + " \" ";
                        return RedirectToAction("Index", "compraRepuestos", new { menu });
                    }
                }

                try
                {
                    context.SaveChanges();
                    TempData["mensaje"] = "El archivo se ha cargado correctamente";
                }
                catch (DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity,
                                validationError.ErrorMessage);
                            // raise a new exception nesting
                            // the current instance as InnerException
                            raise = new InvalidOperationException(message, raise);
                        }
                    }

                    throw raise;
                }
                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                long numero = context.rprecarga.OrderByDescending(x => x.id).FirstOrDefault().numero;
                return RedirectToAction("GuardarPreCargue", "compraRepuestos", new { numero, menu });
            }
            catch (DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity,
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }

                TempData["mensajeError"] = raise;
                return RedirectToAction("Index", "compraRepuestos", new { menu });
            }
        }

        public ActionResult GuardarPreCargue(int? numero, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                if (numero != null)
                {
                    //se cargan los datos del mismo numero que aun no se han comprado
                    //var datos = context.rprecarga.Where(x => x.numero == numero && x.comprado != true && x.seleccion == true);
                    ViewBag.numero = numero;
                    //decimal valor_total = 0;
                    //foreach (var item in datos)
                    //{
                    //    valor_total += item.valor_total;
                    //    ViewBag.valor_total = valor_total;
                    //    ViewBag.fecha = item.fec_creacion;
                    //}

                    //se cargan los datos del mismo numero que aun no se han comprado
                    List<rprecarga> precarga = context.rprecarga.Where(x => x.numero == numero && x.comprado != true).ToList();
                    BuscarFavoritos(menu);
                    return View(precarga);
                }
                else
                {
                    return RedirectToAction("BrowserPreCargue", "compraRepuestos");
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public ActionResult BrowserBackorder(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        /*[HttpPost]
        public ActionResult BrowserBackorder()
        {
            return View();
        }*/

        public ActionResult solicitudes(int? menu)
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre");
            ViewBag.ftipo_compra = new SelectList(context.rtipocompra.Where(x => x.estado && x.para_pfirme), "id", "Descripcion");
            ViewBag.estadosolicitudr = new SelectList(context.restado_solicitud_Repuestos, "id_estado_solicitud", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserPedidoEnFirme(int? menu)
        {
            ViewBag.seleccionados = new string[0];
            //ViewBag.bacs = new SelectList(context.bodega_concesionario.Where(x => x.codigobac != null), "codigobac", "bodccs_nombre");
            ViewBag.sedes = new SelectList(context.icb_sedes.Where(x => x.sede_estado), "sede_id", "sede_nombre");
            ViewBag.tipo_compra_buscar = new SelectList(context.rtipocompra.Where(x => x.estado), "id", "descripcion");
            ViewBag.sedes_buscar = new SelectList(context.icb_sedes.Where(x => x.sede_estado), "sede_id", "sede_nombre");

            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult BrowserPedidoEnFirme(int? menu, string[] select_seleccion)
        {
            ViewBag.seleccionados = select_seleccion?? new string[0];
            //ViewBag.bacs = new SelectList(context.bodega_concesionario.Where(x => x.codigobac != null), "codigobac", "bodccs_nombre");
            ViewBag.tipo_compra_buscar = new SelectList(context.rtipocompra.Where(x => x.estado), "id", "descripcion");
            ViewBag.sedes_buscar = new SelectList(context.icb_sedes.Where(x => x.sede_estado), "sede_id", "sede_nombre");
            ViewBag.sedes = new SelectList(context.icb_sedes.Where(x => x.sede_estado), "sede_id", "sede_nombre");

            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult GetDatosParaPFirme(string[] seleccionados)
        {
            seleccionados = seleccionados?? new string[0];

            var buscarDatos = (from a in context.rsolicitudesrepuestos
                join b in context.bodega_concesionario
                    on a.bodega equals b.id
                /*join d in context.users
                    on a.usuario equals d.user_id*/
                join f in context.rdetallesolicitud
                    on a.id equals f.id_solicitud
                join g in context.icb_referencia
                    on f.referencia equals g.ref_codigo
                join tc in context.rtipocompra
                    on a.tipo_compra equals tc.id into tcp
                    from tc in tcp.DefaultIfEmpty()
                where (f.esta_pedido==1) && (seleccionados.Contains(f.id.ToString()) || seleccionados.Count() == 0)
                group new {g, f, tc} by new {g.ref_codigo, tc.id} 
                into grp 
                select new 
                {
                    ref_codigo = grp.Select(i => i.g.ref_codigo).FirstOrDefault(),
                    ref_descripcion = grp.Select(i => i.g.ref_descripcion).FirstOrDefault(),
                    tipo_compra = grp.Select(i => i.tc.descripcion).FirstOrDefault(),
                    idtipo_compra = grp.Select(i => i.tc.id).FirstOrDefault(),
                    pedido = grp.OrderByDescending(o => o.f.esta_pedido).Select(i => i.f.esta_pedido).FirstOrDefault(),
                    notas = grp.OrderByDescending(o => o.f.nota_pedido).Select(i => i.f.nota_pedido).FirstOrDefault(),
                    archivo_pedido = grp.OrderByDescending(o => o.f.archivo_pedido).Select(i => i.f.archivo_pedido).FirstOrDefault(),
                    npedido_gm = grp.OrderByDescending(o => o.f.npedido_gm).Select(i => i.f.npedido_gm).FirstOrDefault(),
                    cantidad_total = grp.Select(i => i.f.cantidad).Sum(),
                    ref_detalle_solicitudes=grp.Select(k=>k.f.id).ToList(),
                }).ToList();

            var data = buscarDatos.Select(x => new
            {
                CodeReferencia  = x.ref_codigo ?? "",
                DesReferencia = x.ref_descripcion != null ? x.ref_descripcion.ToUpper() : "",
                cantidad_pedida = x.cantidad_total != null ? x.cantidad_total : 0,
                tipo_compra = x.tipo_compra != null ? x.tipo_compra.ToUpper() : "",
                //idtp_compra = x.idtipo_compra != null ? x.idtipo_compra.ToString() : "",
                idtp_compra = x.idtipo_compra != 0 ? x.idtipo_compra.ToString() : "",
                // fue_pedido = x.pedido != null? (x.pedido ? 1 : 0) : 0,
                archivo_generado = x.archivo_pedido?? "",
                notas_pedido = x.notas?? "",
                npedido_gm = x.npedido_gm ?? "",
                idsolicitudes=string.Join(",",x.ref_detalle_solicitudes),
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDatosDesdePFirme(int? numeropedido)
        {
            var buscarDatos = (from a in context.rsolicitudesrepuestos
                               join b in context.bodega_concesionario
                                   on a.bodega equals b.id
                               /*join d in context.users
                                   on a.usuario equals d.user_id*/
                               join f in context.rdetallesolicitud
                                   on a.id equals f.id_solicitud
                               join g in context.icb_referencia
                                   on f.referencia equals g.ref_codigo
                               join p in context.archivo_compra
                              on f.consecutivo_interno equals p.numero_compra
                               join tc in context.rtipocompra
                                   on a.tipo_compra equals tc.id into tcp
                               from tc in tcp.DefaultIfEmpty()
                               where p.id_archivo_compra==numeropedido
                               group new { g, f, tc } by new { g.ref_codigo, tc.id }
                into grp
                               select new
                               {
                                   ref_codigo = grp.Select(i => i.g.ref_codigo).FirstOrDefault(),
                                   ref_descripcion = grp.Select(i => i.g.ref_descripcion).FirstOrDefault(),
                                   tipo_compra = grp.Select(i => i.tc.descripcion).FirstOrDefault(),
                                   idtipo_compra = grp.Select(i => i.tc.id).FirstOrDefault(),
                                   pedido = grp.OrderByDescending(o => o.f.esta_pedido).Select(i => i.f.esta_pedido).FirstOrDefault(),
                                   notas = grp.OrderByDescending(o => o.f.nota_pedido).Select(i => i.f.nota_pedido).FirstOrDefault(),
                                   archivo_pedido = grp.OrderByDescending(o => o.f.archivo_pedido).Select(i => i.f.archivo_pedido).FirstOrDefault(),
                                   npedido_gm = grp.OrderByDescending(o => o.f.npedido_gm).Select(i => i.f.npedido_gm).FirstOrDefault(),
                                   cantidad_total = grp.Select(i => i.f.cantidad).Sum(),
                                   ref_detalle_solicitudes = grp.Select(k => k.f.id).ToList(),
                               }).ToList();

            var data = buscarDatos.Select(x => new
            {
                CodeReferencia = x.ref_codigo ?? "",
                DesReferencia = x.ref_descripcion != null ? x.ref_descripcion.ToUpper() : "",
                cantidad_pedida = x.cantidad_total != null ? x.cantidad_total : 0,
                tipo_compra = x.tipo_compra != null ? x.tipo_compra.ToUpper() : "",
                idtp_compra = x.idtipo_compra != null ? x.idtipo_compra.ToString() : "",
                // fue_pedido = x.pedido != null? (x.pedido ? 1 : 0) : 0,
                archivo_generado = x.archivo_pedido ?? "",
                notas_pedido = x.notas ?? "",
                npedido_gm = x.npedido_gm ?? "",
                idsolicitudes = string.Join(",", x.ref_detalle_solicitudes),
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getFiltroPedidosEnFirme(int? tipo_compra,int? sede,string fecha_inicio,string fecha_fin,string pedidogm)
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
            //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
            string sortColumn = Request.Form
                .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                .FirstOrDefault();
            string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            search = search.Replace(" ", "");
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

            var predicado = PredicateBuilder.True<vw_browser_pedidos_firme>();
            if (tipo_compra != null)
            {
                predicado = predicado.And(d => d.tipo_compra == tipo_compra);
            }
            if(sede != null)
            {
                predicado = predicado.And(d => d.bodega == sede);
            }
            if (!string.IsNullOrWhiteSpace(fecha_inicio))
            {
                var fechax = DateTime.Now;
                var convertir = DateTime.TryParse(fecha_inicio, out fechax);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fecha >= fechax);
                }
            }
            if (!string.IsNullOrWhiteSpace(fecha_fin))
            {
                var fechax = DateTime.Now;
                var convertir = DateTime.TryParse(fecha_fin, out fechax);
                if (convertir == true)
                {
                    fechax = fechax.AddDays(1);
                    predicado = predicado.And(d => d.fecha < fechax);
                }
            }
            if (!string.IsNullOrWhiteSpace(pedidogm))
            {
                predicado = predicado.And(d => d.numero_gm.Contains(pedidogm));
            }

            int registrostotales = context.vw_browser_pedidos_firme.Where(predicado).Count();

            List<vw_browser_pedidos_firme> query2 = new List<vw_browser_pedidos_firme>();
            if (sortColumnDir == "asc")
            {
                 query2 = context.vw_browser_pedidos_firme.Where(predicado)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                query2 = context.vw_browser_pedidos_firme.Where(predicado)
                   .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
            }
            var query = query2.Select(d => new
            {
                d.id_archivo_compra,
                d.descripcion,
                d.sede_nombre,
                d.fecha2,
                tienearchivo=!string.IsNullOrWhiteSpace(d.archivo)?1:0,
                d.archivo,
                d.numero_compra2,
                d.numero_gm,
                d.cantidad_pedido,
                d.pedidas,
                d.recibiendo,
                d.recibidos,
                d.cancelados
            }).ToList();
            return Json(
                   new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                   JsonRequestBehavior.AllowGet);
        }


        public JsonResult GenerarDocumento(int? tipo_compra, string fpago, int? sede, Agrupado[] listRef, string[] seleccionados)
        {
            using (DbContextTransaction dbTran = context.Database.BeginTransaction()) {
                try
                {
                    const string ext_arc = "txt";
                    var valido = false;
                    var generado = false;
                    var bac = (from s in context.icb_sedes where s.sede_id == sede select new { s.sede_codigo_bac, canalDistri = s.rtiposcanalesdistribucion.codigo }).FirstOrDefault();
                    // Nomenclatura Archivo (OK)
                    const int totalsize_arc = 10;
                    //cuento las entradas que hay en la tabla archivos de compra
                    var numero = context.archivo_compra.Count();
                    if (numero == 0)
                    {
                        numero = 1;
                    }
                    else
                    {
                        numero = context.archivo_compra.Max(d => d.numero_compra);
                    }
                    var siguiente = (numero + 1).ToString();
                    var clave_pedido_arc = (from tc in context.rtipocompra where tc.id == tipo_compra select new { tc.clave, tc.condExp }).FirstOrDefault(); //4
                    /*var siguiente = (from p in context.icb_sysparameter where p.syspar_cod == "P144" select p.syspar_value).FirstOrDefault();*/
                    var next_consecutivo_arc = RellenarCon(6, siguiente, 'L', '0'); //6

                    string concat_arc = clave_pedido_arc.clave + next_consecutivo_arc;
                    bool valido_arc = totalsize_arc == concat_arc.Length;
                    
                    // Nomenclatura Encabezado (OK)
                    const int totalsize_enc = 54;
                    var indicativo_enc = "00"; //2
                    var fecha_enc = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture); //14
                    var organizVentas_enc = "7180"; //4
                    //var canalDistrib_enc = (from p in context.icb_sysparameter where p.syspar_cod == "P145" select p.syspar_value).FirstOrDefault(); //2
                    var canalDistrib_enc = bac.canalDistri; //2
                    var sector_enc = fpago; //2
                    var solicitante_enc = bac.sede_codigo_bac; //10
                    var numPedidoCli_enc = RellenarCon(20, siguiente, 'L', '0'); //20

                    string concat_enc = indicativo_enc + fecha_enc + organizVentas_enc + canalDistrib_enc + sector_enc + solicitante_enc + numPedidoCli_enc;
                    bool valido_enc = totalsize_enc == concat_enc.Length;

                    // Nomenclatura Pedido (OK)
                    const int totalsize_ped = 66;
                    var indicativo_ped = "01"; //2
                    var organizVentas_ped = organizVentas_enc; //4
                    var canalDistrib_ped = canalDistrib_enc; //2
                    var sector_ped = sector_enc; //2
                    var solicitante_ped = solicitante_enc; //10
                    var destinatario_ped = solicitante_enc; //10 //??
                    var numPedidoCli_ped = numPedidoCli_enc; //20
                    var fechaPedida_ped = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture); //8
                    var condicionesExp_ped = clave_pedido_arc.condExp; //2
                    var moneda_ped = RellenarCon(5, "COP", 'R', ' '); //5
                    var fill_or_kill_ped = RellenarCon(1, "", 'R', ' '); //1

                    string concat_ped = indicativo_ped + organizVentas_ped + canalDistrib_ped + sector_ped + solicitante_ped + destinatario_ped + numPedidoCli_ped + fechaPedida_ped + condicionesExp_ped + moneda_ped + fill_or_kill_ped;
                    bool valido_ped = totalsize_ped == concat_ped.Length;

                    // Nomenclatura Posicion
                    bool todos_validos_pos = true;
                    List<string> lineas_concat_pos = new List<string>();
                    int sumaCantidadListRef = 0;
                    var solcicitudes = context.rdetallesolicitud.Where(x => seleccionados.Contains(x.id.ToString())).ToList();
                    foreach (var row in listRef)
                    {
                        const int totalsize_pos = 66;
                        var indicativo_pos = "02"; //2
                        var material_pos = EsNumero(row.coderef) ? RellenarCon(18, row.coderef, 'L', '0') : RellenarCon(18, row.coderef, 'R', ' '); //18
                        var codMaterialCli_pos = RellenarCon(22, "", 'R', ' '); //22
                        var cantPedido_pos = RellenarCon(13, row.cantiTotal, 'R', ' '); //13
                        var BO_flag_pos = "X"; //1
                        var destinatario_pos = destinatario_ped; //10

                        string concat_pos = indicativo_pos + material_pos + codMaterialCli_pos + cantPedido_pos + BO_flag_pos + destinatario_pos;
                        lineas_concat_pos.Add(concat_pos);
                        sumaCantidadListRef += EsNumero(row.cantiTotal) ? Int32.Parse(row.cantiTotal) : 0;

                        var solicitudes2 = row.solicitudes.Split(',');
                        List<int> idsolicitudes = new List<int>();
                        foreach(var item2 in solicitudes2)
                        {
                            var idsoli = 0;
                            var convertir3 = Int32.TryParse(item2, out idsoli);
                            if (convertir3 == true)
                            {
                                idsolicitudes.Add(idsoli);
                            }
                        }
                        //
                        var intsiguiente = Convert.ToInt32(siguiente);
                        //Actualizando aquellas solcitudes con la referencia instanciada
                        var agrupados = solcicitudes.Where(x => x.referencia == row.coderef && idsolicitudes.Contains(x.id)).ToList();
                        foreach (var rf in agrupados)
                        {
                            //rf.npedido_gm = row.numpedido_gm;
                            rf.nota_pedido = row.notas;
                            rf.archivo_pedido = concat_arc + "." + ext_arc;
                            //cambio de estado de pedido (crear tabla de estados de referencia_pedido y asignarle el segundo (pedido)
                            rf.esta_pedido = 2;
                            rf.consecutivo_interno = intsiguiente;
                            
                            context.Entry(rf).State = EntityState.Modified;
                            //me traigo el pedido
                            var pedi = context.rsolicitudesrepuestos.Where(d => d.id == rf.id_solicitud).FirstOrDefault();
                            //veo si el pedido al cual pertenece la solicitud tiene mas referencias en estado 1 
                            var ped = context.rdetallesolicitud.Where(d => d.id_solicitud == rf.id_solicitud && d.id != rf.id && rf.esta_pedido==1).Count();
                            //si no hay
                            if (ped == 0)
                            {
                                //verifico el estado del pedido. Si está en otro estado que no sea pedida
                                if (pedi.estado_solicitud < 3)
                                {
                                    pedi.estado_solicitud = 3;
                                    context.Entry(pedi).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                if (pedi.estado_solicitud < 3)
                                {
                                    pedi.estado_solicitud = 2;
                                    context.Entry(pedi).State = EntityState.Modified;
                                }
                            }
                        }
                        context.SaveChanges();
                        bool valido_pos = totalsize_pos == concat_pos.Length;
                        if (!valido_pos)
                        {
                            todos_validos_pos = false;
                            break;
                        }
                    }
                    // Nomenclatura Cifras
                    const int totalsize_cif = 12;
                    var indicativo_cif = "99"; //2
                    var cifcontrol_cif = RellenarCon(10, sumaCantidadListRef.ToString(), 'L', '0'); //10

                    string concat_cif = indicativo_cif + cifcontrol_cif;
                    bool valido_cif = totalsize_cif == concat_cif.Length;

                    var complet_path = "~/Content/PedidosFirmes/" + concat_arc + "." + ext_arc;

                    if (valido_arc && valido_enc && valido_ped && todos_validos_pos && valido_cif)
                    {
                        valido = true;
                        List<string> lineas = new List<string>();
                        lineas.Add(concat_enc);
                        lineas.Add(concat_ped);
                        foreach (var pos in lineas_concat_pos)
                        {
                            lineas.Add(pos);
                        }
                        lineas.Add(concat_cif);

                        /* WriteAllLines creates a file, writes a collection of strings to the file,
                         and then closes the file.  You do NOT need to call Flush() or Close(). */
                        var path = Server.MapPath(complet_path);
                        System.IO.File.WriteAllLines(path, lineas);
                        generado = System.IO.File.Exists(path);
                        if (generado)
                        {
                            //creo el registro en bd del archivo
                            var pedido = new archivo_compra
                            {
                                numero_compra=numero+1,
                                bodega=sede.Value,
                                tipo_compra=tipo_compra.Value,
                                archivo= concat_arc + "." + ext_arc,
                                fecha=DateTime.Now,
                                vigente=true,
                        };
                            context.archivo_compra.Add(pedido);
                            //Aumento de Consecutivo YA NO VA
                            /*var row = context.icb_sysparameter.Where(x => x.syspar_cod == "P144").FirstOrDefault();
                            string sgtString = row.syspar_value;
                            bool convirtio = decimal.TryParse(sgtString, out decimal sgte);
                            if (sgte == 0 || sgte == null)
                            {
                                row.syspar_value = null;
                            }
                            else
                            {
                                row.syspar_value = (sgte + 1) + "";
                            }
                            context.Entry(row).State = EntityState.Modified;*/
                            context.SaveChanges();
                            dbTran.Commit();
                        }
                        else
                        {
                            dbTran.Rollback();
                        }
                    }
                    else
                    {
                        dbTran.Rollback();
                    }
                    return Json(new { valido, generado, nbr_archivo = concat_arc,numero_archivo=siguiente, path = complet_path.Replace("~", "..") }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException)
                {
                    dbTran.Rollback();
                    throw;
                }
            }
        }

        public JsonResult GuardarNumero(string numeropedido,int? numerointerno)
        {         
            var valor = 0;
            var respuesta = "";

            if(!string.IsNullOrWhiteSpace(numeropedido) && numerointerno != null)
            {
                //valido que existe el pedido interno en la tabla rdetallesolicitud
                var solici = context.rdetallesolicitud.Where(d => d.consecutivo_interno == numerointerno).ToList();
                if (solici.Count > 0)
                {
                    //busco el archivo de compra
                    var archivompra = context.archivo_compra.Where(d => d.numero_compra == numerointerno).FirstOrDefault();
                    archivompra.numero_gm = numeropedido;
                    context.Entry(archivompra).State = EntityState.Modified;
                    foreach (var item in solici)
                    {
                        item.npedido_gm = numeropedido;
                        context.Entry(item).State = EntityState.Modified;
                    }
                    var guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        valor = 1;
                        respuesta = "Pedido Actualizado Satisfactoriamente";
                    }
                    else
                    {
                        respuesta = "error en guardado de Base de Datos";
                    }
                }
                else
                {
                    respuesta = "Debe ingresar un número de pedido, y tener seleccionado un Pedido en firme";
                }
            }
            else
            {
                respuesta = "Debe ingresar un número de pedido, y tener seleccionado un Pedido en firme";
            }

            var data = new
            {
                valor,
                respuesta
            };
            return Json(data);
        }

        public string RellenarCon(int longitud, string str, char dir, char comodin)
        {
            string comodines;
            if (dir == 'L')
            {
                comodines = str.PadLeft(longitud, comodin);
            }
            else 
            { //Derecha por default
                comodines = str.PadRight(longitud, comodin);
            }
            return comodines;
        }

        public bool EsNumero(string s)
        {
            return decimal.TryParse(s, out decimal t);
        }

        public JsonResult cargarDatosPrecargue(int numero)
        {
            var info = (from rp in context.rprecarga
                        join r in context.icb_referencia
                            on rp.codigo equals r.ref_codigo into a
                        from r in a.DefaultIfEmpty()
                        join cp in context.vw_promedio
                            on new { Key1 = rp.codigo, Key2 = rp.fec_creacion.Year, Key3 = rp.fec_creacion.Month } equals new { Key1 = cp.codigo, Key2 = (int)cp.ano, Key3 = (int)cp.mes } into b
                        from cp in b.DefaultIfEmpty()
                        join p in context.rprecios
                            on rp.codigo equals p.codigo into c
                        from p in c.DefaultIfEmpty()
                        where rp.numero == numero && rp.seleccion == false
                        orderby rp.id
                        select new
                        {
                            rp.id,
                            rp.codigo,
                            r.ref_descripcion,
                            rp.fecha,
                            rp.poriva,
                            rp.documento,
                            rp.pedidoint,
                            rp.pedidogm,
                            rp.valor_totalenca,
                            rp.valor_unitario,
                            rp.valor_total,
                            rp.cant_ped,
                            rp.cant_fact,
                            rp.cant_real,
                            cp.Promedio,
                            //precio1 = p.precio1 != null ? p.precio1 : 0,
                            precio1 = p.precio1,
                            rp.difcosto,
                            rp.difcantidad,
                            r.perfil,
                        }).ToList();

            //var asd = info.GroupBy(x => x.id).Select(g => new
            //{
            //    g.Key,
            //    id = g.Select(s => s.id).FirstOrDefault(),
            //    codigo = g.Select(s => s.codigo).FirstOrDefault(),
            //    ref_descripcion = g.Select(s => s.ref_descripcion).FirstOrDefault(),
            //    fecha = g.Select(s => s.fecha).FirstOrDefault(),
            //    poriva = g.Select(s => s.poriva).FirstOrDefault(),
            //    documento = g.Select(s => s.documento).FirstOrDefault(),
            //    pedidoint = g.Select(s => s.pedidoint).FirstOrDefault(),
            //    pedidogm = g.Select(s => s.pedidogm).FirstOrDefault(),
            //    valor_totalenca = g.Select(s => s.valor_totalenca).FirstOrDefault(),
            //    valor_unitario = g.Select(s => s.valor_unitario).FirstOrDefault(),
            //    valor_total = g.Select(s => s.valor_total).FirstOrDefault(),
            //    cant_ped = g.Select(s => s.cant_ped).FirstOrDefault(),
            //    cant_fact = g.Select(s => s.cant_fact).FirstOrDefault(),
            //    cant_real = g.Select(s => s.cant_real).FirstOrDefault(),
            //    Promedio = g.Select(s => s.Promedio).FirstOrDefault(),
            //    precio1 = g.Select(s => s.precio1).FirstOrDefault(),
            //    difcantidad = g.Select(s => s.difcantidad).FirstOrDefault(),
            //    difcosto = g.Select(s => s.difcosto).FirstOrDefault()
            //}).ToList();


            var data = info.Select(x => new
            {
                x.id,
                codigo = x.codigo != null ? x.codigo : "",
                descripcion = !string.IsNullOrWhiteSpace(x.ref_descripcion) ? x.ref_descripcion : "",
                existe = !string.IsNullOrWhiteSpace(x.ref_descripcion) ? 1 : 0,
                fecha = x.fecha != null ? x.fecha.Value.ToShortDateString() : "",
                iva = x.poriva != null ? x.poriva : 0,
                documento = !string.IsNullOrWhiteSpace(x.documento) ? x.documento : "",
                pedidoint = !string.IsNullOrWhiteSpace(x.pedidoint) ? x.pedidoint : "",
                pedidogm = !string.IsNullOrWhiteSpace(x.pedidogm) ? x.pedidogm : "",
                x.valor_totalenca,
                x.valor_unitario,
                x.valor_total,
                x.cant_ped,
                x.cant_fact,
                x.cant_real,
                /*valor_totalenca = x.valor_totalenca != null ? x.valor_totalenca : 0,
                valor_unitario = x.valor_unitario != null ? x.valor_unitario : 0,
                valor_total = x.valor_total != null ? x.valor_total : 0,
                cant_ped = x.cant_ped != null ? x.cant_ped : 0,
                cant_fact = x.cant_fact != null ? x.cant_fact : 0,
                cant_real = x.cant_real != null ? x.cant_real : 0,*/
                Promedio = x.Promedio != null ? x.Promedio : 0,
                precio = x.precio1,
                //precio = x.precio1 != null ? x.precio1 : 0,
                x.difcantidad,
                x.difcosto,
                perfilcontable = x.perfil != null ? 1 : 0,
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerReferencias(string q /*cadena de la referencia a buscar*/)
        {
            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            var referencias = (from r in context.icb_referencia
                               where r.ref_descripcion.Contains(q) || r.ref_codigo.Contains(q)
                               select new
                               {
                                   id = r.ref_codigo,
                                   text = "(" + r.ref_codigo + ") " + r.ref_descripcion
                               }).ToList();
            return Json(referencias);
        }

        [HttpPost]
        public ActionResult GuardarPreCargue(IEnumerable<rprecarga> lista, int? menu)
        {
            int contador = 0;
            string ids = Request["id"];
            string[] listaId = ids.Split(',');
            decimal valor_totalencab = 0;
            DateTime fecha_actualizacion = DateTime.Now;

            foreach (string item in listaId)
            {
                int id = Convert.ToInt32(item);
                rprecarga precarga = context.rprecarga.FirstOrDefault(x => x.id == id);
                string numdecimal = Request["cant_real" + item];
                if (!string.IsNullOrEmpty(numdecimal))
                {
                    precarga.cant_real = Convert.ToDecimal(numdecimal, miCultura);
                }

                string boolean = Request["Aseleccion" + item];
                string difCantidad = Request["AdifCantidad" + item];
                string difCosto = Request["AdifCosto" + contador];
                contador++;

                if (difCantidad == null || difCantidad == "0")
                {
                    precarga.difcantidad = Convert.ToInt32(difCantidad);
                }
                else
                {
                    precarga.difcantidad = Convert.ToInt32(difCantidad);
                }

                if (difCosto == null || difCosto == "0")
                {
                    precarga.difcosto = Convert.ToInt32(difCosto);
                }
                else
                {
                    precarga.difcosto = Convert.ToInt32(difCosto);
                }

                if (boolean == null || boolean == "0")
                {
                    precarga.seleccion = false;
                    precarga.valor_totalenca = 0;
                }
                else
                {
                    precarga.seleccion = true;
                    precarga.valor_total = precarga.cant_real * precarga.valor_unitario;
                    valor_totalencab += precarga.valor_total;
                }

                precarga.fec_actualizacion = fecha_actualizacion;
                context.Entry(precarga).State = EntityState.Modified;
            }

            context.SaveChanges();

            foreach (string item in listaId)
            {
                int id = Convert.ToInt32(item);
                rprecarga precarga = context.rprecarga.FirstOrDefault(x => x.id == id && x.seleccion);
                if (precarga != null)
                {
                    precarga.valor_totalenca = valor_totalencab;
                    context.Entry(precarga).State = EntityState.Modified;
                }
            }

            context.SaveChanges();
            TempData["mensaje"] = "PreCargue guardado correctamente";

            //enviar al browser donde se ve la información por consecutivo
            //el browser tiene un boton ver donde lo envia a la misma vista de guardarPrecargue

            long numero = context.rprecarga.OrderByDescending(x => x.numero).FirstOrDefault().numero;
            return RedirectToAction("BrowserPreCargue", "compraRepuestos", new { menu });
        }

        public ActionResult BrowserPreCargue(int? menu)
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.sw == 1 && x.tipo == 3);
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");
            ViewBag.usuarios = context.users.Where(x => x.rol_id == 6 || x.rol_id == 2027).ToList();

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = items;

            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        //----------------comprar
        [HttpPost]
        public ActionResult BrowserPreCargue(NotasContablesModel modelo, int? menu)
        {
            listas();

            var listadosoc = new List<ListadoSolicitudesEnCompra>();
            #region variables

            int numero = Convert.ToInt32(Request["numero"]);
            int perfilContable = Convert.ToInt32(Request["perfil"]);
            IQueryable<rprecarga> lista = context.rprecarga.Where(x => x.numero == numero && x.seleccion);
            string no_existe = "";
            int funciono = 0;
            int tipo_doc = Convert.ToInt32(Request["selectTipoDocumento"]);
            int nit = Convert.ToInt32(Request["selectProveedor"]);
            int bodega = Convert.ToInt32(Request["selectBodegas"]);
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
            icb_terceros buscarProveedor = context.icb_terceros.FirstOrDefault(x => x.tercero_id == nit);
            int? regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
            //var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
            decimal costoModelo = Convert.ToDecimal(modelo.costo, miCultura);

            decimal totalCreditos = 0;
            decimal totalDebitos = 0;
            decimal retenciones = 0;
            decimal valor_totalencaAntesImpuestos = 0;
            decimal total_iva = 0;
            decimal valor_totalenca = 0;
            decimal rete = 0;
            decimal rIVA = 0;
            decimal rICA = 0;

            float porcentaje_rete = 0;
            float porcentaje_reteIVA = 0;
            float porcentaje_reteICA = 0;


            int documentointerno = 0;
            grupoconsecutivos grupo2 = new grupoconsecutivos();
            long consecutivo2 = 0;

            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                              join nombreParametro in context.paramcontablenombres
                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                              join cuenta in context.cuenta_puc
                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                              where perfil.id_perfil == perfilContable
                                              select new
                                              {
                                                  perfil.id,
                                                  perfil.id_nombre_parametro,
                                                  perfil.cuenta,
                                                  perfil.centro,
                                                  perfil.id_perfil,
                                                  nombreParametro.descripcion_parametro,
                                                  cuenta.cntpuc_numero
                                              }).ToList();

            int secuencia = 1;

            List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
            centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
            int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
            List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

            #endregion

            #region validar si la referencia existe

            foreach (rprecarga item in lista)
            {
                icb_referencia existe = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == item.codigo);
                if (existe == null)
                {
                    no_existe += item.codigo + ", ";
                }
            }

            if (no_existe.Length > 0)
            {
                TempData["mensaje_error"] =
                    "No se puede realizar una compra por que las siguientes referencias no existen " + no_existe;
                return RedirectToAction("GuardarPreCargue", new { numero, menu });
            }

            decimal descuento = !string.IsNullOrEmpty(Request["descuento"]) ? Convert.ToDecimal(Request["descuento"], miCultura) : 0;

            List<rprecarga> datos2 = context.rprecarga.Where(x => x.numero == numero && x.seleccion && x.comprado != true && x.cant_real
            == 0).ToList();
            //devuelvo a edicion aquellos que me vinieron con cantidad recibida 0
            foreach (var item in datos2)
            {
                item.seleccion = false;
                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();
            }

            List<rprecarga> datos = context.rprecarga.Where(x => x.numero == numero && x.seleccion && x.comprado != true && x.cant_real>0).ToList();
            if (datos.Count() == 0)
            {
                TempData["mensaje_error"] = "No hay referencias seleccionadas para comprar, por favor valide";
                return View(modelo);
            }

            #endregion

            //consecutivo
            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                x.documento_id == tipo_doc && x.bodega_id == bodega);
            if (grupo != null)
            {
                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                foreach (rprecarga item in datos)
                {
                    valor_totalencaAntesImpuestos = valor_totalencaAntesImpuestos+ item.valor_unitario;
                    /*icb_referencia iva = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == item.codigo);

                    float por_iva = 0;
                    if (iva != null)
                    {
                        por_iva = iva.por_iva_compra;
                    }*/

                    var cantidadiva = item.valor_total - item.valor_unitario;
                    var porcen = cantidadiva > 0 ? (cantidadiva * 100 / item.valor_unitario) : 0;
                    var por_iva = float.Parse(porcen.ToString());
                    //total_iva += item.valor_unitario * item.cant_fact * Convert.ToDecimal(por_iva, miCultura) / 100;
                    total_iva += item.valor_unitario * Convert.ToDecimal(por_iva, miCultura) / 100;

                }

                #region Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado (retenciones)

                if (buscarProveedor.retfuente == null)
                {
                    perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                        x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                        x.tipo_regimenid == regimen_proveedor);
                    if (buscarPerfilTributario != null)
                    {
                        if (buscarPerfilTributario.retfuente == "A" &&
                            valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseretencion)
                        {
                            porcentaje_rete = buscarTipoDocRegistro.retencion;
                            rete = Math.Round(valor_totalencaAntesImpuestos *
                                              Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100));
                        }

                        if (buscarPerfilTributario.retiva == "A" && total_iva >= buscarTipoDocRegistro.baseiva)
                        {
                            porcentaje_reteIVA = buscarTipoDocRegistro.retiva;
                            rIVA = Convert.ToDecimal(
                                Math.Round(total_iva * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100)));
                        }

                        if (buscarPerfilTributario.retica == "A" &&
                            valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseica)
                        {
                            terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                x.idcodica == buscarProveedor.id_acteco && x.bodega == bodega);
                            decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                            if (bodega_acteco != null)
                            {
                                porcentaje_reteICA = (float)bodega_acteco.porcentaje;
                                rICA = Math.Round(valor_totalencaAntesImpuestos *
                                                  Convert.ToDecimal(bodega_acteco.porcentaje / 1000));
                            }

                            if (tercero_acteco != 0)
                            {
                                porcentaje_reteICA = (float)buscarProveedor.acteco_tercero.tarifa;
                                rICA = Math.Round(valor_totalencaAntesImpuestos *
                                                  Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000));
                            }
                            else
                            {
                                porcentaje_reteICA = buscarTipoDocRegistro.retica;
                                rICA = Math.Round(valor_totalencaAntesImpuestos *
                                                  Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000));
                            }
                        }
                    }
                }

                if (buscarProveedor.retfuente != null)
                //if (buscarPerfilTributario != null)

                {
                    if (buscarProveedor.retfuente == "A" &&
                        valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseretencion)
                    {
                        porcentaje_rete = buscarTipoDocRegistro.retencion;
                        rete = Math.Round(valor_totalencaAntesImpuestos *
                                          Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100));
                    }

                    if (buscarProveedor.retiva == "A" && total_iva >= buscarTipoDocRegistro.baseiva)
                    {
                        porcentaje_reteIVA = buscarTipoDocRegistro.retiva;
                        rIVA = Convert.ToDecimal(
                            Math.Round(total_iva * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100)));
                    }

                    if (buscarProveedor.retica == "A" && valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseica)
                    {
                        terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                            x.idcodica == buscarProveedor.id_acteco && x.bodega == bodega);
                        decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                        if (bodega_acteco != null)
                        {
                            porcentaje_reteICA = (float)bodega_acteco.porcentaje;
                            rICA = Math.Round(valor_totalencaAntesImpuestos *
                                              Convert.ToDecimal(bodega_acteco.porcentaje / 1000));
                        }

                        if (tercero_acteco != 0)
                        {
                            porcentaje_reteICA = (float)buscarProveedor.acteco_tercero.tarifa;
                            rICA = Math.Round(valor_totalencaAntesImpuestos *
                                              Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000));
                        }
                        else
                        {
                            porcentaje_reteICA = buscarTipoDocRegistro.retica;
                            rICA = Math.Round(valor_totalencaAntesImpuestos *
                                              Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000));
                        }
                    }
                }

                #endregion

                valor_totalenca = valor_totalencaAntesImpuestos + total_iva - rete - rIVA - rICA +
                                  Convert.ToDecimal(modelo.fletes, miCultura) + Convert.ToDecimal(modelo.iva_fletes, miCultura);
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        //Encabezado documento

                        #region encabezado

                        encab_documento encabezado = new encab_documento
                        {
                            tipo = tipo_doc,
                            numero = consecutivo,
                            nit = nit,
                            fecha = DateTime.Now
                        };
                        int? condicion = modelo.fpago_id;
                        encabezado.fpago_id = condicion;
                        int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                        DateTime vencimiento = DateTime.Now.AddDays(dias);
                        encabezado.vencimiento = vencimiento;
                        encabezado.valor_total = valor_totalenca;
                        encabezado.iva = total_iva;

                        // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
                        if (buscarProveedor.retfuente == null)
                        {
                            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                x.tipo_regimenid == regimen_proveedor);
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.retfuente == "A" && valor_totalencaAntesImpuestos >=
                                    buscarTipoDocRegistro.baseretencion)
                                {
                                    encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                    encabezado.retencion =
                                        Math.Round(valor_totalencaAntesImpuestos *
                                                   Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura));
                                    retenciones += encabezado.retencion;
                                }

                                if (buscarPerfilTributario.retiva == "A" && total_iva >= buscarTipoDocRegistro.baseiva)
                                {
                                    encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                    encabezado.retencion_iva =
                                        Math.Round(encabezado.iva *
                                                   Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                    retenciones += encabezado.retencion_iva;
                                }

                                if (buscarPerfilTributario.retica == "A" &&
                                    valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseica)
                                {
                                    terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                        x.idcodica == buscarProveedor.id_acteco && x.bodega == bodega);
                                    decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                    if (bodega_acteco != null)
                                    {
                                        encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                        encabezado.retencion_ica =
                                            Math.Round(valor_totalencaAntesImpuestos *
                                                       Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                        retenciones += encabezado.retencion_ica;
                                    }

                                    if (tercero_acteco != 0)
                                    {
                                        encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                        encabezado.retencion_ica =
                                            Math.Round(valor_totalencaAntesImpuestos *
                                                       Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                                        retenciones += encabezado.retencion_ica;
                                    }
                                    else
                                    {
                                        encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                        encabezado.retencion_ica =
                                            Math.Round(valor_totalencaAntesImpuestos *
                                                       Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                                        retenciones += encabezado.retencion_ica;
                                    }
                                }
                            }
                        }

                        if (buscarProveedor.retfuente != null)
                        {
                            if (buscarProveedor.retfuente == "A" &&
                                valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseretencion)
                            {
                                encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                encabezado.retencion =
                                    Math.Round(valor_totalencaAntesImpuestos *
                                               Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura));
                                retenciones += encabezado.retencion;
                            }

                            if (buscarProveedor.retiva == "A" && total_iva >= buscarTipoDocRegistro.baseiva)
                            {
                                encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                encabezado.retencion_iva =
                                    Math.Round(encabezado.iva * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                retenciones += encabezado.retencion_iva;
                            }

                            if (buscarProveedor.retica == "A" &&
                                valor_totalencaAntesImpuestos >= buscarTipoDocRegistro.baseica)
                            {
                                terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                    x.idcodica == buscarProveedor.id_acteco && x.bodega == bodega);
                                decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                if (bodega_acteco != null)
                                {
                                    encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                    encabezado.retencion_ica =
                                        Math.Round(valor_totalencaAntesImpuestos *
                                                   Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                    retenciones += encabezado.retencion_ica;
                                }

                                if (tercero_acteco != 0)
                                {
                                    encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                    encabezado.retencion_ica =
                                        Math.Round(valor_totalencaAntesImpuestos *
                                                   Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                                    retenciones += encabezado.retencion_ica;
                                }
                                else
                                {
                                    encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                    encabezado.retencion_ica =
                                        Math.Round(valor_totalencaAntesImpuestos *
                                                   Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                                    retenciones += encabezado.retencion_ica;
                                }
                            }
                        }

                        if (modelo.fletes != null)
                        {
                            encabezado.fletes = Convert.ToDecimal(modelo.fletes, miCultura);
                            encabezado.iva_fletes = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                        }

                        encabezado.costo =
                            valor_totalencaAntesImpuestos; // tenia el total pero la jefe liliana el dia 16/06 lo cambio
                        encabezado.vendedor = Convert.ToInt32(Request["solicitadopor"]);
                        encabezado.documento = Convert.ToString(modelo.pedido);
                        encabezado.bodega = bodega;
                        encabezado.concepto = modelo.concepto;
                        encabezado.moneda = Convert.ToInt32(Request["moneda"]);
                        if (Request["tasa"] != "")
                        {
                            encabezado.tasa = Convert.ToInt32(Request["tasa"]);
                        }

                        encabezado.valor_mercancia = valor_totalencaAntesImpuestos;
                        encabezado.fec_creacion = DateTime.Now;
                        encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        encabezado.estado = true;
                        encabezado.perfilcontable = Convert.ToInt32(Request["perfil"]);
                        encabezado.concepto2 = modelo.concepto2;

                        context.encab_documento.Add(encabezado);
                        context.SaveChanges();

                        #endregion

                        /*int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                            .FirstOrDefault().idencabezado;*/
                        int id_encabezado = encabezado.idencabezado;
                       //veo si el documento externo tiene documento interno asociado
                       tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == tipo_doc).FirstOrDefault();
                        if (doc_interno.doc_interno_asociado != null)
                        {
                            grupo2 = context.grupoconsecutivos.FirstOrDefault(x =>
                              x.documento_id == doc_interno.doc_interno_asociado && x.bodega_id == bodega);
                            if (grupo2 != null)
                            {
                                consecutivo2 = doc.BuscarConsecutivo(grupo2.grupo);
                                //calculo y guardo el encabezado del movimiento interno
                                encab_documento encabezado2 = new encab_documento
                                {
                                    tipo = doc_interno.doc_interno_asociado.Value,
                                    numero = consecutivo2,
                                    nit = encabezado.nit,
                                    fecha = DateTime.Now,
                                    fpago_id = encabezado.fpago_id,
                                    vencimiento = encabezado.vencimiento,
                                    valor_total = encabezado.valor_total,
                                    iva = encabezado.iva,
                                    porcen_retencion = encabezado.porcen_retencion,
                                    retencion = encabezado.retencion,
                                    porcen_reteiva = encabezado.porcen_reteiva,
                                    retencion_iva = encabezado.retencion_iva,
                                    porcen_retica = encabezado.porcen_retica,
                                    retencion_ica = encabezado.retencion_ica,
                                    fletes = encabezado.fletes,
                                    iva_fletes = encabezado.iva_fletes,
                                    costo = encabezado.costo,
                                    vendedor = encabezado.vendedor,
                                    documento = encabezado.documento,
                                    remision = encabezado.remision,
                                    bodega = encabezado.bodega,
                                    concepto = encabezado.concepto,
                                    moneda = encabezado.moneda,
                                    perfilcontable = encabezado.perfilcontable,
                                    valor_mercancia = encabezado.valor_mercancia,
                                    fec_creacion = encabezado.fec_creacion,
                                    userid_creacion = encabezado.userid_creacion,
                                    estado = true,
                                    concepto2 = encabezado.concepto2,
                                    id_movimiento_interno = encabezado.idencabezado,
                                };
                                context.encab_documento.Add(encabezado2);
                                context.SaveChanges();
                                documentointerno = encabezado2.idencabezado;
                            }
                        }
                            //Mov Contable

                            #region movimientos contables

                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                            foreach (var parametro in parametrosCuentasVerificar)
                        {
                            string descripcionParametro = context.paramcontablenombres
                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 1 && Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                    || parametro.id_nombre_parametro == 3 &&
                                    Convert.ToDecimal(encabezado.retencion, miCultura) != 0
                                    || parametro.id_nombre_parametro == 4 &&
                                    Convert.ToDecimal(encabezado.retencion_iva, miCultura) != 0
                                    || parametro.id_nombre_parametro == 5 &&
                                    Convert.ToDecimal(encabezado.retencion_ica, miCultura) != 0
                                    || parametro.id_nombre_parametro == 6 && Convert.ToDecimal(encabezado.fletes, miCultura) != 0
                                    || parametro.id_nombre_parametro == 14 &&
                                    Convert.ToDecimal(encabezado.iva_fletes, miCultura) != 0)
                                {
                                    mov_contable movNuevo = new mov_contable
                                    {
                                        id_encab = encabezado.idencabezado,
                                        seq = secuencia,
                                        idparametronombre = parametro.id_nombre_parametro,
                                        cuenta = parametro.cuenta,
                                        centro = parametro.centro,
                                        fec = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        documento = Convert.ToString(0),
                                        detalle = "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                       encabezado.pedido
                                    };

                                    cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();

                                    if (info.tercero)
                                    {
                                        movNuevo.nit = nit;
                                    }
                                    else
                                    {
                                        icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                            .FirstOrDefault();
                                        movNuevo.nit = tercero.tercero_id;
                                    }

                                    // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                    #region Cuentas X Pagar

                                    if (parametro.id_nombre_parametro == 1)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = Convert.ToString(modelo.pedido);
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = Convert.ToDecimal(valor_totalenca, miCultura);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = Convert.ToDecimal(valor_totalenca, miCultura);
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = Convert.ToDecimal(valor_totalenca, miCultura);
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = Convert.ToDecimal(valor_totalenca, miCultura);
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region Retencion

                                    if (parametro.id_nombre_parametro == 3)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalencaAntesImpuestos, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = modelo.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = rete;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = rete;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = rete;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = rete;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region ReteIVA

                                    if (parametro.id_nombre_parametro == 4)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(total_iva, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = modelo.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = rIVA;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = rIVA;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = rIVA;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = rIVA;
                                        }
                                    }

                                    #endregion

                                    #region ReteICA

                                    if (parametro.id_nombre_parametro == 5)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalencaAntesImpuestos, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = modelo.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = rICA;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = rICA;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = rICA;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = rICA;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region Fletes

                                    if (parametro.id_nombre_parametro == 6)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = modelo.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(modelo.fletes, miCultura);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(modelo.fletes, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(modelo.fletes, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(modelo.fletes, miCultura);
                                        }
                                    }

                                    #endregion

                                    #region IVA fletes

                                    if (parametro.id_nombre_parametro == 14)
                                    {
                                        /*if (info.aplicaniff==true)
                                        {

                                        }*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = modelo.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(modelo.iva_fletes, miCultura);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                        }
                                    }

                                    #endregion

                                    context.mov_contable.Add(movNuevo);
                                    //context.SaveChanges();

                                    secuencia++;
                                    //Cuentas valores

                                    #region Cuentas valores

                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                        x.nit == movNuevo.nit);
                                    if (buscar_cuentas_valores != null)
                                    {
                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                        context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        DateTime fechaHoy = DateTime.Now;
                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                            ano = fechaHoy.Year,
                                            mes = fechaHoy.Month,
                                            cuenta = movNuevo.cuenta,
                                            centro = movNuevo.centro,
                                            nit = movNuevo.nit,
                                            debito = movNuevo.debito,
                                            credito = movNuevo.credito,
                                            debitoniff = movNuevo.debitoniif,
                                            creditoniff = movNuevo.creditoniif
                                        };
                                        context.cuentas_valores.Add(crearCuentaValor);
                                        //context.SaveChanges();
                                    }

                                    #endregion

                                    totalCreditos += movNuevo.credito;
                                    totalDebitos += movNuevo.debito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta =
                                            "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                        DescripcionParametro = descripcionParametro,
                                        ValorDebito = movNuevo.debito,
                                        ValorCredito = movNuevo.credito
                                    });
                                }
                            }
                        }

                        #endregion

                        //Lineas documento y referencias inventario

                        #region lineasDocumento-y-referenciasInventario

                        int i = 0;
                        foreach (rprecarga item in datos)
                        {
                            if (item.codigo != null)
                            {
                                icb_referencia unidadCodigo =
                                    context.icb_referencia.FirstOrDefault(x => x.ref_codigo == item.codigo);
                                string und = unidadCodigo.unidad_medida;
                                i++;
                                lineas_documento linea = new lineas_documento
                                {
                                    id_encabezado = id_encabezado,
                                    codigo = item.codigo,
                                    seq = i,
                                    fec = DateTime.Now,
                                    nit = Convert.ToInt32(Request["selectProveedor"]),
                                    und = Convert.ToString(und),
                                    cantidad = item.cant_fact,
                                    valor_unitario = (item.valor_unitario / item.cant_fact),
                                    costo_unitario = (item.valor_unitario / item.cant_fact),
                                    bodega = bodega,
                                    porcentaje_descuento = 0,
                                    cantidad_pedida = item.cant_ped,
                                    fec_creacion = DateTime.Now,
                                    estado = true,
                                    pedido = Convert.ToInt32(item.pedidoint),
                                    moneda = Convert.ToInt32(Request["moneda"])
                                };
                                if (Request["tasa"] != "")
                                {
                                    linea.tasa = Convert.ToInt32(Request["tasa"]);
                                }

                                linea.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                icb_referencia por_iva = context.icb_referencia.Find(item.codigo);
                                if (por_iva != null)
                                {
                                    linea.porcentaje_iva = por_iva.por_iva_compra;
                                }
                                linea.id_precarga = item.id;
                                context.lineas_documento.Add(linea);

                                //modificar en rprecarga para indicar que ya se compro
                                item.comprado = true;
                                context.Entry(item).State = EntityState.Modified;
                                context.SaveChanges();

                                string codigoReferencia = item.codigo;
                                icb_referencia buscarReferencia =
                                    context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigoReferencia);
                                if (buscarReferencia != null)
                                {
                                    buscarReferencia.fec_ultima_entrada = DateTime.Now;
                                    context.Entry(buscarReferencia).State = EntityState.Modified;
                                }
                            }

                            //Referencias Inven
                            CsCalcularCostoPromedio calcular = new CsCalcularCostoPromedio();

                            #region referencias inven
                            //Calculo de costo promedio
                            decimal costo_promedio = calcular.calcularCostoPromedio(Convert.ToDecimal(Request["cantidad" + i], miCultura), item.valor_unitario,
                                               item.codigo, bodega);

                            referencias_inven referencia = context.referencias_inven.FirstOrDefault(x =>
                                x.codigo == item.codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month &&
                                x.bodega == bodega);

                            var entrada = true;
                            if (doc_interno.doc_interno_asociado != null)
                            {//calculo el comportamiento del documento interno asociado

                                var docinternoaso = context.tp_doc_registros.Where(d => d.tpdoc_id == doc_interno.doc_interno_asociado.Value).FirstOrDefault();
                                if (docinternoaso.entrada_salida != null)
                                {
                                    entrada = docinternoaso.entrada_salida.Value;
                                }
                            }


                            if (referencia != null)
                            {
                                referencia.costo_prom = costo_promedio;
                                referencia.codigo = item.codigo;
                                if (entrada == true)
                                {
                                    referencia.can_com += item.cant_real;
                                    referencia.cos_com += item.valor_unitario * item.cant_real;
                                    referencia.can_ent += item.cant_real;
                                    referencia.cos_ent += item.valor_unitario * item.cant_real;
                                }
                                else
                                {
                                    referencia.can_sal += item.cant_real;
                                    referencia.cos_sal += item.valor_unitario * item.cant_real;
                                }                               
                                context.Entry(referencia).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                            else
                            {
                                referencias_inven refe = new referencias_inven
                                {
                                    codigo = item.codigo,
                                    bodega = bodega,
                                    ano = Convert.ToInt16(DateTime.Now.Year),
                                    mes = Convert.ToInt16(DateTime.Now.Month),
                                    modulo = "R",
                                };
                                if (entrada == true)
                                {
                                    refe.can_com = item.cant_real;
                                    refe.cos_com = item.valor_unitario * item.cant_real;
                                    refe.can_ent = item.cant_real;
                                    refe.cos_ent = item.valor_unitario * item.cant_real;
                                   
                                }
                                else
                                {
                                    refe.can_sal = item.cant_real;
                                    refe.cos_sal = item.valor_unitario * item.cant_real;
                                }

                                refe.costo_prom = item.valor_unitario;
                                refe.modulo = "R";
                                context.referencias_inven.Add(refe);
                                context.SaveChanges();

                            }
                            //Referencias Inven
                            //registro de referencias del documento interno (si existe)
                            if (doc_interno.doc_interno_asociado != null && documentointerno != 0)
                            {

                                lineas_documento lineas2 = new lineas_documento
                                {
                                    id_encabezado = documentointerno,
                                    codigo = item.codigo,
                                    seq = i,
                                    fec = DateTime.Now,
                                    nit = Convert.ToInt32(Request["selectProveedor"]),
                                    cantidad = item.cant_fact,
                                    valor_unitario = (item.valor_unitario/item.cant_real),
                                    porcentaje_descuento = 0,
                                    costo_unitario = (item.valor_unitario / item.cant_real),
                                    bodega = bodega,
                                    fec_creacion = DateTime.Now,
                                    pedido = Convert.ToInt32(item.pedidoint),
                                    moneda = Convert.ToInt32(Request["moneda"]),

                                    estado = true,
                                };
                                lineas2.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                var cantidadiva = item.valor_total - item.valor_unitario;
                                var porcen = cantidadiva>0?(cantidadiva * 100 / item.valor_unitario):0;
                                var por_iva2 = float.Parse(porcen.ToString());
                                //icb_referencia por_iva = context.icb_referencia.Find(item.codigo);

                                /*if (por_iva2 != null)
                                {*/
                                    //lineas2.porcentaje_iva = por_iva2.por_iva_compra;
                                    lineas2.porcentaje_iva = por_iva2;
                                //}
                                if (Request["tasa"] != "")
                                {
                                    lineas2.tasa = Convert.ToInt32(Request["tasa"]);
                                }

                                context.lineas_documento.Add(lineas2);
                                //aca va la parte de adjudicacion del inventario a solicitudes de repuestos que tengan el pedido GM
                                var cantidadtot = item.cant_real;
                                var archivocom = context.archivo_compra.OrderBy(d=>d.fecha).Where(d => d.numero_gm == item.pedidogm).ToList();
                                foreach (var item3 in archivocom)
                                {
                                    bool breakLoops = false;
                                    //veo si hay solicitudes que tengan esta referencia sin recibir completa
                                    var solicit = context.rdetallesolicitud.OrderBy(d => d.fecha_creacion).Where(d => d.consecutivo_interno == item3.numero_compra && d.referencia==item.codigo && (d.cantidad_recibida == null || d.cantidad_recibida < d.cantidad) && (d.esta_pedido>=2 && d.esta_pedido<4)).ToList();
                                    foreach (var item4 in solicit)
                                    {
                                        var insertar = 0;
                                        //verifico si la solicitud no se encuentra en el arreglo de solicitudes ya despachadas
                                        var existe = listadosoc.Where(d => d.pedidogm == item.pedidogm && d.referencia == item.codigo && d.solicitud == item4.id).FirstOrDefault();
                                        if (existe!=null)
                                        {
                                            if (existe.todo == false)
                                            {
                                                insertar = 1;
                                            }
                                        }
                                        else
                                        {
                                            insertar = 1;
                                        }
                                        //si se puede guardar
                                        if (insertar == 1)
                                        {
                                            //determino cuando insertar
                                            var cant_reci = item4.cantidad_recibida != null ? item4.cantidad_recibida.Value : 0;
                                            var diferencia = item4.cantidad.Value- cant_reci;
                                            var cant_despachar = diferencia <= Convert.ToInt32(cantidadtot) ? diferencia : Convert.ToInt32(cantidadtot);

                                            cantidadtot = cantidadtot - cant_despachar;
                                            item4.cantidad_recibida = cant_reci + cant_despachar;
                                            var todo2 = false;
                                            if ((cant_reci + cant_despachar) == item4.cantidad)
                                            {
                                                item4.esta_pedido = 4;
                                                todo2 = true;
                                            }
                                            else
                                            {
                                                item4.esta_pedido = 3;
                                            }
                                            context.Entry(item4).State = EntityState.Modified;
                                            //veo si esa solicitud tiene una separacion
                                            if (item4.IDRSEPARACION != null)
                                            {
                                                //busco esa separacion
                                                var sepa = context.rseparacionmercancia.Where(d => d.id == item4.IDRSEPARACION).FirstOrDefault();
                                                if (sepa != null)
                                                {
                                                    sepa.cantidad_recibida =sepa.cantidad_recibida!=null?(sepa.cantidad_recibida + cant_despachar):cant_despachar;
                                                    context.Entry(item4).State = EntityState.Modified;
                                                    //veo si esa separacion viene de un pedido de repuestos
                                                    if (sepa.idpedido != null)
                                                    {
                                                        //veo si existe el pedido
                                                        var vped = context.icb_referencia_movdetalle.Where(d => d.icb_referencia_mov.refmov_id == sepa.idpedido && d.ref_codigo == item.codigo).FirstOrDefault();
                                                        if (vped != null)
                                                        {
                                                            var recibido_pedido = vped.cantidad_recibida;
                                                            vped.cantidad_recibida = vped.cantidad_recibida + cant_despachar;
                                                            if (recibido_pedido + cant_despachar >= vped.refdet_cantidad)
                                                            {
                                                                vped.tiene_stock = true;
                                                                context.Entry(vped).State = EntityState.Modified;
                                                                //enviar_notificacion al asesor
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            //veo si existe la OT (en desarrollo)

                                            //agrego la ref a la pila de ref despachadas.
                                            existe = listadosoc.Where(d => d.pedidogm == item.pedidogm && d.referencia == item.codigo && d.solicitud == item4.id).FirstOrDefault();
                                            if (existe != null)
                                            {
                                                
                                                existe.todo = todo2;
                                                existe.cantidad = existe.cantidad + cant_despachar;
                                            }
                                            else
                                            {
                                                listadosoc.Add(new ListadoSolicitudesEnCompra {cantidad=cant_despachar,referencia=item.codigo,pedidogm=item.pedidogm,solicitud=item4.id,todo=todo2 });
                                            }
                                        }
                                        if (cantidadtot <= 0)
                                        {
                                            breakLoops = true;
                                            break;
                                        }
                                    }
                                    if (breakLoops == true)
                                    {
                                        
                                        break;
                                    }
                                    var guardarn = context.SaveChanges();
                                }

                            }



                            #endregion
                        }

                        #endregion


                        //Mov Contable (IVA e Inventario)

                        #region Mov Contable (IVA e Inventario)

                        int secuenciaI = secuencia;
                        foreach (rprecarga item in datos)
                        {
                            if (item.codigo != null)
                            {
                                foreach (var parametro in parametrosCuentasVerificar)
                                {
                                    string descripcionParametro = context.paramcontablenombres
                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                        .descripcion_parametro;
                                    cuenta_puc buscarCuenta =
                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                    if (buscarCuenta != null)
                                    {
                                        if (parametro.id_nombre_parametro == 2 &&
                                            Convert.ToDecimal(item.valor_unitario, miCultura) != 0
                                            || parametro.id_nombre_parametro == 9 &&
                                            Convert.ToDecimal(valor_totalenca, miCultura) != 0)
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = encabezado.idencabezado,
                                                seq = secuenciaI,
                                                idparametronombre = parametro.id_nombre_parametro,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                fec = DateTime.Now,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                documento = Convert.ToString(0),
                                                detalle =
                                                "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                encabezado.pedido
                                            };

                                            cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == parametro.cuenta)
                                                .FirstOrDefault();

                                            if (info.tercero)
                                            {
                                                movNuevo.nit = nit;
                                            }
                                            else
                                            {
                                                icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                    .FirstOrDefault();
                                                movNuevo.nit = tercero.tercero_id;
                                            }

                                            #region IVA

                                            if (parametro.id_nombre_parametro == 2)
                                            {
                                                icb_referencia iva = context.icb_referencia.FirstOrDefault(x =>
                                                    x.ref_codigo == item.codigo);
                                                float por_iva = 0;
                                                if (iva != null)
                                                {
                                                    por_iva = iva.por_iva_compra;
                                                }

                                                var cantidadiva = item.valor_total - item.valor_unitario;
                                                var porcen = cantidadiva > 0 ? (cantidadiva * 100 / item.valor_unitario) : 0;
                                                /*decimal valorIva =
                                                    item.valor_unitario * item.cant_real * Convert.ToDecimal(por_iva, miCultura) /
                                                    100;*/
                                                decimal valorIva = cantidadiva;
                                                icb_referencia perfilReferencia =
                                                    context.icb_referencia.FirstOrDefault(x =>
                                                        x.ref_codigo == item.codigo);
                                                int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                    r.id == perfilBuscar);

                                                #region tiene perfil la referencia

                                                if (pcr != null)
                                                {
                                                    int? cuentaIva = pcr.cuenta_iva_compras;

                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                    movNuevo.seq = secuenciaI;
                                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                    if (cuentaIva != null)
                                                    {
                                                        movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                        movNuevo.centro = parametro.centro;
                                                        movNuevo.fec = DateTime.Now;
                                                        movNuevo.fec_creacion = DateTime.Now;
                                                        movNuevo.userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                        movNuevo.documento = Convert.ToString(modelo.pedido);


                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == cuentaIva).FirstOrDefault();
                                                        if (infoReferencia.manejabase)
                                                        {
                                                            movNuevo.basecontable =
                                                                Convert.ToDecimal(item.valor_unitario, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (infoReferencia.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (infoReferencia.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = Convert.ToDecimal(valorIva, miCultura);

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = Convert.ToDecimal(valorIva, miCultura);
                                                        }

                                                        if (infoReferencia.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = Convert.ToDecimal(valorIva, miCultura);
                                                        }

                                                        if (infoReferencia.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = Convert.ToDecimal(valorIva, miCultura);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        movNuevo.cuenta = parametro.cuenta;
                                                        movNuevo.centro = parametro.centro;
                                                        movNuevo.fec = DateTime.Now;
                                                        movNuevo.fec_creacion = DateTime.Now;
                                                        movNuevo.userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                        movNuevo.documento = modelo.documento;

                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();
                                                        if (infoReferencia.manejabase)
                                                        {
                                                            movNuevo.basecontable =
                                                               Convert.ToDecimal(item.valor_unitario * item.cant_real, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (infoReferencia.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (infoReferencia.concepniff == 1)
                                                        {
                                                            movNuevo.credito = Convert.ToDecimal(valorIva, miCultura);
                                                            movNuevo.debito = 0;

                                                            movNuevo.creditoniif =
                                                                Convert.ToDecimal(valorIva, miCultura);
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (infoReferencia.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif =
                                                                Convert.ToDecimal(valorIva, miCultura);
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (infoReferencia.concepniff == 5)
                                                        {
                                                            movNuevo.credito = Convert.ToDecimal(valorIva, miCultura);
                                                            movNuevo.debito = 0;
                                                        }
                                                    }


                                                    //var buscarIVA = context.mov_contable.FirstOrDefault(x => x.id_encab == encabezado.idencabezado && x.cuenta == cuentaIva);
                                                    mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                        x.id_encab == encabezado.idencabezado &&
                                                        x.cuenta == parametro.cuenta &&
                                                        x.idparametronombre == parametro.id_nombre_parametro);
                                                    if (buscarIVA != null)
                                                    {
                                                        buscarIVA.debito += movNuevo.debito;
                                                        buscarIVA.debitoniif += movNuevo.debitoniif;
                                                        buscarIVA.credito += movNuevo.credito;
                                                        buscarIVA.creditoniif += movNuevo.creditoniif;
                                                        context.Entry(buscarIVA).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        mov_contable crearMovContable = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuenciaI,
                                                            idparametronombre =
                                                            parametro.id_nombre_parametro,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = parametro.centro,
                                                            nit = encabezado.nit,
                                                            fec = DateTime.Now,
                                                            debito = movNuevo.debito,
                                                            debitoniif = movNuevo.debitoniif,
                                                            basecontable = movNuevo.basecontable,
                                                            credito = movNuevo.credito,
                                                            creditoniif = movNuevo.creditoniif,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle =
                                                            "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                            encabezado.pedido
                                                        };
                                                        context.mov_contable.Add(crearMovContable);
                                                    }

                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    DateTime fechaActual = DateTime.Now;
                                                    cuentas_valores buscar_cuentas_valores =
                                                        context.cuentas_valores.FirstOrDefault(x =>
                                                            x.centro == parametro.centro && x.cuenta == movNuevo.cuenta &&
                                                            x.nit == movNuevo.nit && x.ano == fechaActual.Year &&
                                                            x.mes == fechaActual.Month);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    secuenciaI++;

                                                    #endregion
                                                }

                                                #endregion

                                                #region no tiene perfil la referencia  

                                                else
                                                {
                                                    /*if (info.aplicaniff==true)
                                                    {

                                                    }*/

                                                    if (info.manejabase)
                                                    {
                                                        movNuevo.basecontable =
                                                            Convert.ToDecimal(item.valor_unitario * item.cant_real, miCultura);
                                                    }
                                                    else
                                                    {
                                                        movNuevo.basecontable = 0;
                                                    }

                                                    if (info.documeto)
                                                    {
                                                        movNuevo.documento = modelo.documento;
                                                    }

                                                    if (buscarCuenta.concepniff == 1)
                                                    {
                                                        movNuevo.credito = 0;
                                                        movNuevo.debito = Convert.ToDecimal(valorIva, miCultura);

                                                        movNuevo.creditoniif = 0;
                                                        movNuevo.debitoniif = Convert.ToDecimal(valorIva, miCultura);
                                                    }

                                                    if (buscarCuenta.concepniff == 4)
                                                    {
                                                        movNuevo.creditoniif = 0;
                                                        movNuevo.debitoniif = Convert.ToDecimal(valorIva, miCultura);
                                                    }

                                                    if (buscarCuenta.concepniff == 5)
                                                    {
                                                        movNuevo.credito = 0;
                                                        movNuevo.debito = Convert.ToDecimal(valorIva, miCultura);
                                                    }

                                                    mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                        x.id_encab == encabezado.idencabezado &&
                                                        x.cuenta == parametro.cuenta &&
                                                        x.idparametronombre == parametro.id_nombre_parametro);
                                                    if (buscarIVA != null)
                                                    {
                                                        buscarIVA.basecontable += movNuevo.basecontable;
                                                        buscarIVA.debito += movNuevo.debito;
                                                        buscarIVA.debitoniif += movNuevo.debitoniif;
                                                        buscarIVA.credito += movNuevo.credito;
                                                        buscarIVA.creditoniif += movNuevo.creditoniif;
                                                        context.Entry(buscarIVA).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        mov_contable crearMovContable = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuenciaI,
                                                            idparametronombre =
                                                            parametro.id_nombre_parametro,
                                                            cuenta = Convert.ToInt32(parametro.cuenta),
                                                            centro = parametro.centro,
                                                            nit = encabezado.nit,
                                                            fec = DateTime.Now,
                                                            debito = movNuevo.debito,
                                                            debitoniif = movNuevo.debitoniif,
                                                            basecontable = movNuevo.basecontable,
                                                            credito = movNuevo.credito,
                                                            creditoniif = movNuevo.creditoniif,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle =
                                                            "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                            encabezado.pedido
                                                        };
                                                        context.mov_contable.Add(crearMovContable);
                                                    }

                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    DateTime fechaActual = DateTime.Now;
                                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(
                                                        x => x.centro == parametro.centro &&
                                                             x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                                                             x.ano == fechaActual.Year && x.mes == fechaActual.Month);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = parametro.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    secuenciaI++;
                                                }

                                                #endregion
                                            }

                                            #endregion

                                            #region Inventario				

                                            if (parametro.id_nombre_parametro == 9)
                                            {
                                                icb_referencia perfilReferencia =
                                                    context.icb_referencia.FirstOrDefault(x =>
                                                        x.ref_codigo == item.codigo);
                                                int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                    r.id == perfilBuscar);

                                                #region tienePerfilReferencia

                                                if (pcr != null)
                                                {
                                                    int? cuentaInven = pcr.cuenta_inv_compra;

                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                    movNuevo.seq = secuenciaI;
                                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                    if (cuentaInven != null)
                                                    {
                                                        movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                        movNuevo.centro = parametro.centro;
                                                        movNuevo.fec = DateTime.Now;
                                                        movNuevo.fec_creacion = DateTime.Now;
                                                        movNuevo.userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                        movNuevo.documento = Convert.ToString(modelo.pedido);

                                                        cuenta_puc infoReferencia = context.cuenta_puc.Where(a => a.cntpuc_id == cuentaInven).FirstOrDefault();
                                                        if (infoReferencia.manejabase)
                                                        {
                                                            movNuevo.basecontable =
                                                                Convert.ToDecimal(item.valor_unitario * item.cant_real, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (infoReferencia.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }




                                                        if (infoReferencia.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito =
                                                                Convert.ToDecimal(item.valor_unitario, miCultura);

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif =
                                                                Convert.ToDecimal(item.valor_unitario, miCultura);
                                                        }

                                                        if (infoReferencia.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif =
                                                                Convert.ToDecimal(item.valor_unitario, miCultura);
                                                        }

                                                        if (infoReferencia.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito =
                                                                Convert.ToDecimal(item.valor_unitario, miCultura);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        movNuevo.cuenta = parametro.cuenta;
                                                        movNuevo.centro = parametro.centro;
                                                        movNuevo.fec = DateTime.Now;
                                                        movNuevo.fec_creacion = DateTime.Now;
                                                        movNuevo.userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                        movNuevo.documento = Convert.ToString(modelo.pedido);

                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();
                                                        if (infoReferencia.manejabase)
                                                        {
                                                            movNuevo.basecontable =
                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (infoReferencia.documeto)
                                                        {
                                                            movNuevo.documento = Convert.ToString(modelo.pedido);
                                                        }

                                                        if (infoReferencia.concepniff == 1)
                                                        {
                                                            movNuevo.credito = Convert.ToDecimal(item.valor_unitario, miCultura);
                                                            movNuevo.debito = 0;

                                                            movNuevo.creditoniif = Convert.ToDecimal(item.valor_unitario, miCultura);
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (infoReferencia.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = Convert.ToDecimal(item.valor_unitario, miCultura);
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (infoReferencia.concepniff == 5)
                                                        {
                                                            movNuevo.credito = Convert.ToDecimal(item.valor_unitario, miCultura);
                                                            movNuevo.debito = 0;
                                                        }
                                                    }

                                                    mov_contable buscarInventario = context.mov_contable.FirstOrDefault(x =>
                                                        x.id_encab == encabezado.idencabezado &&
                                                        x.cuenta == cuentaInven);
                                                    if (buscarInventario != null)
                                                    {
                                                        buscarInventario.debito += movNuevo.debito;
                                                        buscarInventario.debitoniif += movNuevo.debitoniif;
                                                        buscarInventario.credito += movNuevo.credito;
                                                        buscarInventario.creditoniif += movNuevo.creditoniif;
                                                        context.Entry(buscarInventario).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        mov_contable crearMovContable = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuenciaI,
                                                            idparametronombre =
                                                            parametro.id_nombre_parametro,
                                                            cuenta = Convert.ToInt32(cuentaInven),
                                                            centro = parametro.centro,
                                                            nit = encabezado.nit,
                                                            fec = DateTime.Now,
                                                            debito = movNuevo.debito,
                                                            debitoniif = movNuevo.debitoniif,
                                                            basecontable = movNuevo.basecontable,
                                                            credito = movNuevo.credito,
                                                            creditoniif = movNuevo.creditoniif,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle =
                                                            "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                            encabezado.pedido
                                                        };
                                                        context.mov_contable.Add(crearMovContable);
                                                    }

                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    DateTime fechaActual = DateTime.Now;
                                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(
                                                        x => x.centro == parametro.centro &&
                                                             x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit &&
                                                             x.ano == fechaActual.Year && x.mes == fechaActual.Month);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                        //context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    secuenciaI++;
                                                }

                                                #endregion

                                                #region no tiene perfil la referencia

                                                else
                                                {
                                                    /*if (info.aplicaniff==true)
                                                    {

                                                    }*/

                                                    if (info.manejabase)
                                                    {
                                                        movNuevo.basecontable =
                                                            Convert.ToDecimal(item.valor_unitario, miCultura);
                                                    }
                                                    else
                                                    {
                                                        movNuevo.basecontable = 0;
                                                    }

                                                    if (info.documeto)
                                                    {
                                                        movNuevo.documento = modelo.documento;
                                                    }

                                                    if (buscarCuenta.concepniff == 1)
                                                    {
                                                        movNuevo.credito = 0;
                                                        movNuevo.debito =
                                                            Convert.ToDecimal(item.valor_unitario, miCultura);

                                                        movNuevo.creditoniif = 0;
                                                        movNuevo.debitoniif =
                                                            Convert.ToDecimal(item.valor_unitario, miCultura);
                                                    }

                                                    if (buscarCuenta.concepniff == 4)
                                                    {
                                                        movNuevo.creditoniif = 0;
                                                        movNuevo.debitoniif =
                                                            Convert.ToDecimal(item.valor_unitario, miCultura);
                                                    }

                                                    if (buscarCuenta.concepniff == 5)
                                                    {
                                                        movNuevo.credito = 0;
                                                        movNuevo.debito =
                                                            Convert.ToDecimal(item.valor_unitario, miCultura);
                                                    }

                                                    mov_contable buscarInventario = context.mov_contable.FirstOrDefault(x =>
                                                        x.id_encab == encabezado.idencabezado &&
                                                        x.cuenta == parametro.cuenta);
                                                    if (buscarInventario != null)
                                                    {
                                                        buscarInventario.basecontable += movNuevo.basecontable;
                                                        buscarInventario.debito += movNuevo.debito;
                                                        buscarInventario.debitoniif += movNuevo.debitoniif;
                                                        buscarInventario.credito += movNuevo.credito;
                                                        buscarInventario.creditoniif += movNuevo.creditoniif;
                                                        context.Entry(buscarInventario).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        mov_contable crearMovContable = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuenciaI,
                                                            idparametronombre =
                                                            parametro.id_nombre_parametro,
                                                            cuenta = Convert.ToInt32(parametro.cuenta),
                                                            centro = parametro.centro,
                                                            nit = encabezado.nit,
                                                            fec = DateTime.Now,
                                                            debito = movNuevo.debito,
                                                            debitoniif = movNuevo.debitoniif,
                                                            basecontable = movNuevo.basecontable,
                                                            credito = movNuevo.credito,
                                                            creditoniif = movNuevo.creditoniif,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle =
                                                            "Compra de tipo " + tipo_doc + " del pedido numero " +
                                                            encabezado.pedido
                                                        };
                                                        context.mov_contable.Add(crearMovContable);
                                                    }

                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    DateTime fechaActual = DateTime.Now;
                                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(
                                                        x => x.centro == parametro.centro &&
                                                             x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                                                             x.ano == fechaActual.Year && x.mes == fechaActual.Month);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    secuenciaI++;
                                                }

                                                #endregion
                                            }

                                            #endregion

                                            totalCreditos += movNuevo.credito;
                                            totalDebitos += movNuevo.debito;
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta =
                                                    "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                                DescripcionParametro = descripcionParametro,
                                                ValorDebito = movNuevo.debito,
                                                ValorCredito = movNuevo.credito
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        #endregion


                        #region validaciones para guardar
                        var diferenciax = totalDebitos - totalCreditos;
                        var valorabsoluto = Math.Abs(diferenciax);
                        if (valorabsoluto >= 1)
                        {
                            TempData["mensaje_error"] =
                                "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                            ViewBag.documentoSeleccionado = encabezado.tipo;
                            ViewBag.bodegaSeleccionado = encabezado.bodega;
                            ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                            ViewBag.documentoDescuadrado = listaDescuadrados;
                            ViewBag.calculoDebito = totalDebitos;
                            ViewBag.calculoCredito = totalCreditos;
                            dbTran.Rollback();
                            listas();
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        funciono = 1;

                        #endregion

                        if (funciono > 0)
                        {
                            context.SaveChanges();
                            dbTran.Commit();
                            TempData["mensaje"] = "registro creado correctamente";
                            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                            doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                            if (documentointerno != 0)
                            {
                                doc.ActualizarConsecutivo(grupo2.grupo, consecutivo2);

                            }
                            listas();
                            return RedirectToAction("DetalleCompras", new { id = id_encabezado, menu });
                        }
                    }

                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        Exception raise = ex;
                        foreach (DbEntityValidationResult validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity,
                                    validationError.ErrorMessage);
                                // raise a new exception nesting
                                // the current instance as InnerException
                                raise = new InvalidOperationException(message, raise);
                            }
                        }

                        throw raise;
                        //throw;
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "no hay consecutivo";
            }
            //cierre
            //else
            //{
            //    TempData["mensaje_error"] = "Lista vacia";
            //}
            //else
            //{
            //    TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
            //    var errors = ModelState.Select(x => x.Value.Errors)
            //                           .Where(y => y.Count > 0)
            //                           .ToList();
            //}

            listas();

            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.sw == 1 && x.tipo == 3);
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");
            ViewBag.usuarios = context.users.ToList();

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = items;

            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");

            BuscarFavoritos(menu);
            return RedirectToAction("compraManual", "compraRepuestos", new { menu });
        }

        public void listas()
        {
            int monedapeso = context.monedas.FirstOrDefault(x => x.moneda == 1).moneda;

            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.sw == 1 && x.tipo == 3);
            int user_id = Convert.ToInt32(Session["user_usuarioid"]);
            ViewBag.usuario = context.users.Where(x => x.user_id == user_id).Select(x=>x.user_id).FirstOrDefault();
            ViewBag.nombreusuario =  context.users.Where(x => x.user_id == user_id).Select( x => x.user_nombre + " " + x.user_apellido ).FirstOrDefault();
            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion", monedapeso);
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social,
                                 ter.doc_tercero
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.doc_tercero + " - " + item.nombreTErcero + " " + item.apellidosTercero + " " +
                             item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }



            ViewBag.proveedor = items;

            //ViewBag.codigo = new SelectList(context.icb_referencia.Where(x=> x.modulo == "R"), "ref_codigo", "ref_descripcion");
            ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            ViewBag.usuarios = context.users.Where(x => x.rol_id == 6 || x.rol_id == 2027).ToList();

            ViewBag.condicion_pago = context.fpago_tercero;

            encab_documento buscarUltimaCompra = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numCompraCreado = buscarUltimaCompra != null ? buscarUltimaCompra.numero : 0;



        }

        [HttpGet]
        public ActionResult compraManual(int? menu)
        {
            listas();

            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult compraManual(NotasContablesModel modelo, int? menu)
        {
            decimal baseiva = 0;
            decimal pretiva = 0;
            decimal baseica = 0;
            decimal pretica = 0;
            decimal baseretencion = 0;
            decimal pretretencion = 0;
            int documentointerno = 0;
            grupoconsecutivos grupo2 = new grupoconsecutivos();
            long consecutivo2 = 0;
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        int funciono = 0;
                        decimal totalCreditos = 0;
                        decimal totalDebitos = 0;

                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == modelo.perfilcontable
                                                          select new
                                                          {
                                                              perfil.id,
                                                              perfil.id_nombre_parametro,
                                                              perfil.cuenta,
                                                              perfil.centro,
                                                              perfil.id_perfil,
                                                              nombreParametro.descripcion_parametro,
                                                              cuenta.cntpuc_numero
                                                          }).ToList();

                        int secuencia = 1;
                        List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                        string lista = Request["lista"];
                        if (!string.IsNullOrEmpty(lista))
                        {
                            int datos = Convert.ToInt32(lista);
                            decimal costoTotal =
                                Convert.ToDecimal(Request["valor_proveedor"], miCultura); //costo con retenciones y fletes
                            decimal ivaEncabezado = Convert.ToDecimal(Request["valorIVA"], miCultura); //valor total del iva
                            decimal descuentoEncabezado =
                                Convert.ToDecimal(Request["valorDes"], miCultura); //valor total del descuento
                            decimal costoEncabezado = Convert.ToDecimal(Request["valorSub"], miCultura); //valor antes de impuestos

                            decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                            //consecutivo
                            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == modelo.tipo && x.bodega_id == modelo.bodega);
                            if (grupo != null)
                            {
                                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                //Encabezado documento

                                #region encabezado
                                DateTime fechafactura = Convert.ToDateTime(Request["Fechafactura"]);
                                encab_documento encabezado = new encab_documento
                                {
                                    tipo = modelo.tipo,
                                    numero = consecutivo,
                                    nit = modelo.nit,
                                    fecha = fechafactura
                                    };
                                int? condicion = modelo.fpago_id;
                                encabezado.fpago_id = condicion;
                                int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                DateTime vencimiento = DateTime.Now.AddDays(dias);
                                encabezado.vencimiento = vencimiento;
                                encabezado.valor_total = costoTotal;
                                encabezado.iva = ivaEncabezado;

                                #region Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                                tp_doc_registros buscarTipoDocRegistro =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.tipo);
                                tercero_proveedor buscarProveedor =
                                    context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == modelo.nit);
                                icb_terceros buscarRetenciones = context.icb_terceros.FirstOrDefault(x => x.tercero_id == modelo.nit);

                                decimal costoModelo = Convert.ToDecimal(modelo.costo, miCultura);
                                decimal retenciones = 0;
                                if (buscarRetenciones.retfuente == null)
                                {
                                    perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                        x.bodega == modelo.bodega && x.sw == buscarTipoDocRegistro.sw &&
                                        x.tipo_regimenid == buscarRetenciones.tpregimen_id);
                                    if (buscarPerfilTributario != null)
                                    {
                                        baseretencion = buscarPerfilTributario.baseretfuente != null ? buscarPerfilTributario.baseretfuente.Value : 0;
                                        pretretencion = buscarPerfilTributario.pretfuente != null ? buscarPerfilTributario.pretfuente.Value : 0;
                                        baseiva = buscarPerfilTributario.baseretiva != null ? buscarPerfilTributario.baseretiva.Value : 0;
                                        pretiva = buscarPerfilTributario.pretiva != null ? buscarPerfilTributario.pretiva.Value : 0;
                                        baseica = buscarPerfilTributario.baseretica != null ? buscarPerfilTributario.baseretica.Value : 0;
                                        pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;

                                        if (buscarPerfilTributario.retfuente == "A" &&
                                            valor_totalenca >= baseretencion)
                                        {
                                            encabezado.porcen_retencion = float.Parse(pretretencion.ToString());
                                            encabezado.retencion =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(pretretencion / 100, miCultura));
                                            retenciones += encabezado.retencion;
                                        }

                                        if (buscarPerfilTributario.retiva == "A" &&
                                            ivaEncabezado >= baseiva)
                                        {
                                            encabezado.porcen_reteiva = float.Parse(pretiva.ToString());
                                            encabezado.retencion_iva =
                                                Math.Round(encabezado.iva *
                                                           Convert.ToDecimal(pretiva / 100, miCultura));
                                            retenciones += encabezado.retencion_iva;
                                        }

                                        if (buscarPerfilTributario.retica == "A" &&
                                            valor_totalenca >= baseica)
                                        {
                                            terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                                x.idcodica == buscarProveedor.acteco_id && x.bodega == modelo.bodega);
                                            decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                            if (bodega_acteco != null)
                                            {
                                                encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                                encabezado.retencion_ica =
                                                    Math.Round(valor_totalenca *
                                                               Convert.ToDecimal(bodega_acteco.porcentaje / 100, miCultura));
                                                retenciones += encabezado.retencion_ica;
                                            }

                                            if (tercero_acteco != 0)
                                            {
                                                encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                                encabezado.retencion_ica = Math.Round(valor_totalenca *
                                                               Convert.ToDecimal(
                                                                   buscarProveedor.acteco_tercero.tarifa / 100, miCultura));
                                                retenciones += encabezado.retencion_ica;
                                            }
                                            else
                                            {
                                                encabezado.porcen_retica = (float)pretica;
                                                encabezado.retencion_ica =
                                                    Math.Round(valor_totalenca *
                                                               Convert.ToDecimal(pretica / 100, miCultura));
                                                retenciones += encabezado.retencion_ica;
                                            }
                                        }
                                    }
                                }
                                //var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == modelo.bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);


                                if (buscarRetenciones.retfuente != null)
                                {
                                    perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                                                   x.bodega == modelo.bodega && x.sw == buscarTipoDocRegistro.sw &&
                                                                   x.tipo_regimenid == buscarRetenciones.tpregimen_id);
                                    if (buscarPerfilTributario != null)
                                    {
                                        baseretencion = buscarPerfilTributario.baseretfuente != null ? buscarPerfilTributario.baseretfuente.Value : 0;
                                        pretretencion = buscarPerfilTributario.pretfuente != null ? buscarPerfilTributario.pretfuente.Value : 0;
                                        baseiva = buscarPerfilTributario.baseretiva != null ? buscarPerfilTributario.baseretiva.Value : 0;
                                        pretiva = buscarPerfilTributario.pretiva != null ? buscarPerfilTributario.pretiva.Value : 0;
                                        baseica = buscarPerfilTributario.baseretica != null ? buscarPerfilTributario.baseretica.Value : 0;
                                        pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;
                                    }
                                    else
                                    {
                                        baseretencion = buscarTipoDocRegistro.baseretencion;
                                        pretretencion = Convert.ToDecimal(buscarTipoDocRegistro.retencion.ToString(), miCultura);
                                        baseiva = Convert.ToDecimal(buscarTipoDocRegistro.baseiva, miCultura);
                                        pretiva = Convert.ToDecimal(buscarTipoDocRegistro.retiva.ToString(), miCultura);
                                        baseica = Convert.ToDecimal(buscarTipoDocRegistro.baseica, miCultura);
                                        //pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;
                                    }

                                    if (buscarRetenciones.retfuente == "A" &&
                                        valor_totalenca >= baseretencion)
                                    {
                                        encabezado.porcen_retencion = (float)pretretencion;
                                        encabezado.retencion =
                                            Math.Round(valor_totalenca *
                                                       Convert.ToDecimal(pretretencion / 100, miCultura));
                                        retenciones += encabezado.retencion;
                                    }

                                    if (buscarRetenciones.retiva == "A" && ivaEncabezado >= baseiva)
                                    {
                                        encabezado.porcen_reteiva = (float)pretiva;
                                        encabezado.retencion_iva =
                                            Math.Round(encabezado.iva *
                                                       Convert.ToDecimal(pretiva / 100, miCultura));
                                        retenciones += encabezado.retencion_iva;
                                    }

                                    if (buscarRetenciones.retica == "A" &&
                                        valor_totalenca >= baseica)
                                    {
                                        terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                            x.idcodica == buscarProveedor.acteco_id && x.bodega == modelo.bodega);
                                        decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                        if (bodega_acteco != null)
                                        {
                                            encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }

                                        if (tercero_acteco != 0)
                                        {
                                            encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(
                                                               buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                        else
                                        {
                                            encabezado.porcen_retica = (float)pretica;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(pretica / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                    }
                                }

                                #endregion

                                if (modelo.fletes != null)
                                {
                                    encabezado.fletes = Convert.ToDecimal(modelo.fletes, miCultura);
                                    encabezado.iva_fletes = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                }

                                encabezado.costo = valor_totalenca;
                                encabezado.vendedor = modelo.solicitadopor;
                                encabezado.documento = Convert.ToString(modelo.pedido);
                                encabezado.remision = modelo.remision;
                                encabezado.bodega = modelo.bodega;
                                encabezado.concepto = modelo.concepto;
                                encabezado.moneda = Convert.ToInt32(Request["moneda"]);
                                encabezado.perfilcontable = Convert.ToInt32(Request["perfilcontable"]);
                                if (Request["tasa"] != "")
                                {
                                    encabezado.tasa = Convert.ToInt32(Request["tasa"]);
                                }

                                encabezado.valor_mercancia = valor_totalenca;
                                encabezado.fec_creacion = DateTime.Now;
                                encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                encabezado.estado = true;
                                encabezado.concepto2 = modelo.concepto2;

                                context.encab_documento.Add(encabezado);
                                context.SaveChanges();

                                #endregion

                                int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado).Where(d => d.idencabezado == encabezado.idencabezado)
                                    .FirstOrDefault().idencabezado;

                                //veo si el documento externo tiene documento interno asociado
                                tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == modelo.tipo).FirstOrDefault();
                                if (doc_interno.doc_interno_asociado != null)
                                {
                                    //se consulta consecutivo de documento interno
                                    grupo2 = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == doc_interno.doc_interno_asociado && x.bodega_id == modelo.bodega);
                                    if (grupo2 != null)
                                    {
                                        consecutivo2 = doc.BuscarConsecutivo(grupo2.grupo);
                                        //calculo y guardo el encabezado del movimiento interno
                                        encab_documento encabezado2 = new encab_documento
                                        {
                                            tipo = doc_interno.doc_interno_asociado.Value,
                                            numero = consecutivo2,
                                            nit = modelo.nit,
                                            fecha = DateTime.Now,
                                            fpago_id = encabezado.fpago_id,
                                            vencimiento = encabezado.vencimiento,
                                            valor_total = encabezado.valor_total,
                                            iva = encabezado.iva,
                                            porcen_retencion = encabezado.porcen_retencion,
                                            retencion = encabezado.retencion,
                                            porcen_reteiva = encabezado.porcen_reteiva,
                                            retencion_iva = encabezado.retencion_iva,
                                            porcen_retica = encabezado.porcen_retica,
                                            retencion_ica = encabezado.retencion_ica,
                                            fletes = encabezado.fletes,
                                            iva_fletes = encabezado.iva_fletes,
                                            costo = encabezado.costo,
                                            vendedor = encabezado.vendedor,
                                            documento = encabezado.documento,
                                            remision = encabezado.remision,
                                            bodega = encabezado.bodega,
                                            concepto = encabezado.concepto,
                                            moneda = encabezado.moneda,
                                            perfilcontable = encabezado.perfilcontable,
                                            valor_mercancia = encabezado.valor_mercancia,
                                            fec_creacion = encabezado.fec_creacion,
                                            userid_creacion = encabezado.userid_creacion,
                                            estado = true,
                                            concepto2 = encabezado.concepto2,
                                            id_movimiento_interno = encabezado.idencabezado,
                                        };
                                        context.encab_documento.Add(encabezado2);
                                        context.SaveChanges();
                                        documentointerno = encabezado2.idencabezado;
                                    }
                                }
                                //Mov Contable

                                #region movimientos contables

                                //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                                foreach (var parametro in parametrosCuentasVerificar)
                                {
                                    string descripcionParametro = context.paramcontablenombres
                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                        .descripcion_parametro;
                                    cuenta_puc buscarCuenta =
                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                    if (buscarCuenta != null)
                                    {
                                        if (parametro.id_nombre_parametro == 1 &&
                                            Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                            || parametro.id_nombre_parametro == 3 &&
                                            Convert.ToDecimal(encabezado.retencion, miCultura) != 0
                                            || parametro.id_nombre_parametro == 4 &&
                                            Convert.ToDecimal(encabezado.retencion_iva, miCultura) != 0
                                            || parametro.id_nombre_parametro == 5 &&
                                            Convert.ToDecimal(encabezado.retencion_ica, miCultura) != 0
                                            || parametro.id_nombre_parametro == 6 &&
                                            Convert.ToDecimal(encabezado.fletes, miCultura) != 0
                                            || parametro.id_nombre_parametro == 14 &&
                                            Convert.ToDecimal(encabezado.iva_fletes, miCultura) != 0)
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = encabezado.idencabezado,
                                                seq = secuencia,
                                                idparametronombre = parametro.id_nombre_parametro,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                fec = DateTime.Now,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                documento = Convert.ToString(modelo.pedido),
                                                detalle = "Compra de repuestos con consecutivo " + consecutivo,
                                                estado = true
                                            };

                                            cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == parametro.cuenta)
                                                .FirstOrDefault();

                                            if (info.tercero)
                                            {
                                                movNuevo.nit = modelo.nit;
                                            }
                                            else
                                            {
                                                icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                    .FirstOrDefault();
                                                movNuevo.nit = tercero.tercero_id;
                                            }

                                            // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                            #region Cuentas X Pagar

                                            if (parametro.id_nombre_parametro == 1)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(modelo.pedido);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(costoTotal, miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(costoTotal, miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region Retencion

                                            if (parametro.id_nombre_parametro == 3)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       pretretencion / 100, miCultura)), miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            valor_totalenca *
                                                            Convert.ToDecimal(pretretencion / 100, miCultura)), miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            valor_totalenca *
                                                            Convert.ToDecimal(pretretencion / 100, miCultura)), miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       pretretencion / 100, miCultura)), miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region ReteIVA

                                            if (parametro.id_nombre_parametro == 4)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(
                                                                       pretiva / 100, miCultura)), miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = Convert.ToDecimal(
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(
                                                                       pretiva / 100, miCultura)), miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            encabezado.iva *
                                                            Convert.ToDecimal(pretiva / 100, miCultura)), miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(
                                                                       pretiva / 100, miCultura)), miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region ReteICA

                                            if (parametro.id_nombre_parametro == 5)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region Fletes

                                            if (parametro.id_nombre_parametro == 6)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(modelo.fletes, miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(modelo.fletes, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(modelo.fletes, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(modelo.fletes, miCultura);
                                                }
                                            }

                                            #endregion

                                            #region IVA fletes

                                            if (parametro.id_nombre_parametro == 14)
                                            {
                                                /*if (info.aplicaniff==true)
                                                {

                                                }*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(modelo.iva_fletes, miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                                }
                                            }

                                            #endregion

                                            context.mov_contable.Add(movNuevo);
                                            //context.SaveChanges();

                                            secuencia++;
                                            //Cuentas valores

                                            #region Cuentas valores

                                            DateTime fechaHoy = DateTime.Now;
                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                                x.nit == movNuevo.nit && x.ano == fechaHoy.Year &&
                                                x.mes == fechaHoy.Month);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.debito += movNuevo.debito;
                                                buscar_cuentas_valores.credito += movNuevo.credito;
                                                buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                context.SaveChanges();
                                            }
                                            else
                                            {
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = movNuevo.cuenta,
                                                    centro = movNuevo.centro,
                                                    nit = movNuevo.nit,
                                                    debito = movNuevo.debito,
                                                    credito = movNuevo.credito,
                                                    debitoniff = movNuevo.debitoniif,
                                                    creditoniff = movNuevo.creditoniif
                                                };
                                                context.cuentas_valores.Add(crearCuentaValor);
                                                context.SaveChanges();
                                            }

                                            #endregion

                                            totalCreditos += movNuevo.credito;
                                            totalDebitos += movNuevo.debito;
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta =
                                                    "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                                DescripcionParametro = descripcionParametro,
                                                ValorDebito = movNuevo.debito,
                                                ValorCredito = movNuevo.credito
                                            });
                                        }
                                    }
                                }

                                #endregion

                                //Lineas documento

                                #region lineasDocumento

                                List<mov_contable> listaMov = new List<mov_contable>();
                                int listaLineas = Convert.ToInt32(Request["lista"]);
                                for (int i = 1; i <= listaLineas; i++)
                                    if (!string.IsNullOrEmpty(Request["codigo" + i]))
                                    {
                                        decimal porDescuento = !string.IsNullOrEmpty(Request["porcentaje_descuento" + i])
                                            ? Convert.ToDecimal(Request["porcentaje_descuento" + i], miCultura)
                                            : 0;
                                        decimal final = 0;
                                        string referencia = Request["codigo" + i];
                                        int llevaiva = Request["porcentaje_iva" + i] != null
                                            ? Convert.ToInt32(Request["porcentaje_iva" + i])
                                            : 0;
                                        decimal valorReferencia = Convert.ToDecimal(Request["valor_unitario" + i], miCultura);
                                        decimal descontar = porDescuento / 100;
                                        decimal porIVAReferencia = Convert.ToDecimal(Request["porcentaje_iva" + i], miCultura) / 100;
                                        decimal qwerty = valorReferencia * descontar;
                                        decimal qwe = qwerty % 1;
                                        if (qwe < Convert.ToDecimal(0.5, miCultura))
                                        {
                                            final = valorReferencia - Math.Round(valorReferencia * descontar);
                                        }
                                        else if (qwe >= Convert.ToDecimal(0.5, miCultura))
                                        {
                                            final = valorReferencia - Math.Ceiling(valorReferencia * descontar);
                                        }

                                        decimal baseUnitario = final * Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                        decimal ivaReferencia = Math.Round(
                                            final * porIVAReferencia * Convert.ToDecimal(Request["cantidad" + i], miCultura));

                                        int Bodega = modelo.bodega;

                                        CsCalcularCostoPromedio calcular = new CsCalcularCostoPromedio();



                                        lineas_documento lineas = new lineas_documento
                                        {
                                            id_encabezado = id_encabezado,
                                            codigo = Request["codigo" + i],
                                            seq = i,
                                            fec = DateTime.Now,
                                            nit = modelo.nit,
                                            cantidad = Convert.ToDecimal(Request["cantidad" + i], miCultura)
                                        };
                                        decimal ivaLista = Convert.ToDecimal(Request["porcentaje_iva" + i], miCultura);
                                        lineas.porcentaje_iva = (float)ivaLista;
                                        lineas.valor_unitario = final;
                                        decimal descuento = porDescuento;
                                        lineas.porcentaje_descuento = (float)descuento;
                                        lineas.costo_unitario = final;
                                        lineas.bodega = modelo.bodega;
                                        lineas.fec_creacion = DateTime.Now;
                                        lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        lineas.estado = true;
                                        encabezado.moneda = Convert.ToInt32(Request["moneda"]);
                                        if (Request["tasa"] != "")
                                        {
                                            lineas.tasa = Convert.ToInt32(Request["tasa"]);
                                        }

                                        context.lineas_documento.Add(lineas);

                                        string codigoReferencia = Request["codigo" + i];
                                        icb_referencia buscarReferencia =
                                            context.icb_referencia.FirstOrDefault(x =>
                                                x.ref_codigo == codigoReferencia);
                                        if (buscarReferencia != null)
                                        {
                                            buscarReferencia.fec_ultima_entrada = DateTime.Now;
                                            context.Entry(buscarReferencia).State = EntityState.Modified;
                                        }

                                        #endregion

                                        if (!string.IsNullOrEmpty(Request["ordenCompra" + i]))
                                        {
                                            string asd = Request["ordenCompra" + i];
                                            int orden = Convert.ToInt32(asd);
                                            //actualizamos las lineas de la compra, ponemos la cantidad devuelta (todas)
                                            rdetalleordencompra cantidadRecibida = context.rdetalleordencompra.FirstOrDefault(x =>
                                                x.idencaborden == orden && x.codigo_referencia == lineas.codigo);

                                            #region actualizar las lineas de la compra devuelta

                                            cantidadRecibida.despachada += lineas.cantidad;
                                            context.Entry(cantidadRecibida).State = EntityState.Modified;

                                            #endregion
                                        }


                                        #region referencias inven y detalle de movimiento interno

                                        int anio = DateTime.Now.Year;
                                        int mes = DateTime.Now.Month;

                                        //Calculo de costo promedio
                                        decimal costo_promedio = calcular.calcularCostoPromedio(Convert.ToDecimal(Request["cantidad" + i], miCultura), final,
                                                referencia, Bodega);

                                        referencias_inven refin = new referencias_inven();
                                        referencias_inven existencia = context.referencias_inven.FirstOrDefault(x =>
                                            x.ano == anio && x.mes == mes && x.codigo == referencia &&
                                            x.bodega == modelo.bodega);

                                        //verifico la naturaleza del documento interno asociado
                                        var entrada = true;
                                        if (doc_interno.doc_interno_asociado != null)
                                        {//calculo el comportamiento del documento interno asociado

                                            var docinternoaso = context.tp_doc_registros.Where(d => d.tpdoc_id == doc_interno.doc_interno_asociado.Value).FirstOrDefault();
                                            if (docinternoaso.entrada_salida != null)
                                            {
                                                entrada = docinternoaso.entrada_salida.Value;
                                            }
                                        }
                                        if (existencia != null)
                                        {
                                            existencia.costo_prom = costo_promedio;
                                            existencia.codigo = referencia;
                                            if (entrada == true)
                                            {
                                                existencia.can_ent += Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                existencia.cos_ent += final * Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                existencia.can_com += Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                existencia.cos_com += final * Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                            }
                                            else
                                            {
                                                existencia.can_sal += Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                existencia.cos_sal += final * Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                            }

                                            context.Entry(existencia).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            refin.bodega = modelo.bodega;
                                            refin.codigo = referencia;
                                            refin.ano = Convert.ToInt16(DateTime.Now.Year);
                                            refin.mes = Convert.ToInt16(DateTime.Now.Month);
                                            if (entrada == true)
                                            {
                                                refin.can_ent = Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                refin.cos_ent = baseUnitario;
                                                refin.can_com = Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                refin.cos_com = baseUnitario;
                                            }
                                            else
                                            {
                                                refin.can_sal = Convert.ToDecimal(Request["cantidad" + i], miCultura);
                                                refin.cos_sal = baseUnitario;
                                            }

                                            refin.costo_prom = final;
                                            refin.modulo = "R";
                                            context.referencias_inven.Add(refin);
                                            context.SaveChanges();
                                        }

                                        //Referencias Inven
                                        //registro de referencias del documento interno (si existe)
                                        if (doc_interno.doc_interno_asociado != null && documentointerno != 0)
                                        {
                                            lineas_documento lineas2 = new lineas_documento
                                            {
                                                id_encabezado = documentointerno,
                                                codigo = lineas.codigo,
                                                seq = i,
                                                fec = DateTime.Now,
                                                nit = lineas.nit,
                                                cantidad = lineas.cantidad,
                                                porcentaje_iva = lineas.porcentaje_iva,
                                                valor_unitario = lineas.valor_unitario,
                                                porcentaje_descuento = lineas.porcentaje_descuento,
                                                costo_unitario = lineas.costo_unitario,
                                                bodega = lineas.bodega,
                                                fec_creacion = lineas.fec_creacion,
                                                userid_creacion = lineas.userid_creacion,
                                                tasa = lineas.tasa,
                                            };

                                            context.lineas_documento.Add(lineas2);
                                        }
                                        #endregion

                                        //Mov Contable (IVA e Inventario)

                                        #region Mov Contable (IVA e Inventario)

                                        foreach (var parametro in parametrosCuentasVerificar)
                                        {
                                            string descripcionParametro = context.paramcontablenombres
                                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                .descripcion_parametro;
                                            cuenta_puc buscarCuenta =
                                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                            if (buscarCuenta != null)
                                            {
                                                if (parametro.id_nombre_parametro == 2 &&
                                                    Convert.ToDecimal(ivaReferencia, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 9 &&
                                                    Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 25 &&
                                                    Convert.ToDecimal(valor_totalenca, miCultura) != 0)
                                                {
                                                    mov_contable movNuevo = new mov_contable
                                                    {
                                                        id_encab = encabezado.idencabezado,
                                                        seq = secuencia,
                                                        idparametronombre = parametro.id_nombre_parametro,
                                                        cuenta = parametro.cuenta,
                                                        centro = parametro.centro,
                                                        fec = DateTime.Now,
                                                        fec_creacion = DateTime.Now,
                                                        userid_creacion =
                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                        documento = Convert.ToString(modelo.pedido)
                                                    };

                                                    cuenta_puc info = context.cuenta_puc
                                                        .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                    if (info.tercero)
                                                    {
                                                        movNuevo.nit = modelo.nit;
                                                    }
                                                    else
                                                    {
                                                        icb_terceros tercero = context.icb_terceros
                                                            .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                        movNuevo.nit = tercero.tercero_id;
                                                    }

                                                    #region IVA

                                                    if (parametro.id_nombre_parametro == 2)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaIva = pcr.cuenta_iva_compras;
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region si tiene perfil y cuenta asignada a ese perfil

                                                            if (cuentaIva != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(modelo.pedido);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaIva)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = modelo.documento;
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region si tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(modelo.pedido);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = modelo.documento;
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region No tiene perfil la referencia

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(baseUnitario, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(ivaReferencia, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(ivaReferencia, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        #region Buscamos si ya esta o si creamos el registo en mov contable

                                                        mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarIVA != null)
                                                        {
                                                            buscarIVA.debito += movNuevo.debito;
                                                            buscarIVA.debitoniif += movNuevo.debitoniif;
                                                            buscarIVA.credito += movNuevo.credito;
                                                            buscarIVA.creditoniif += movNuevo.creditoniif;
                                                            buscarIVA.basecontable += movNuevo.basecontable;
                                                            context.Entry(buscarIVA).State = EntityState.Modified;
                                                            context.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Compra de repuestos con consecutivo " + consecutivo,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                            context.SaveChanges();
                                                        }

                                                        #endregion
                                                    }

                                                    #endregion

                                                    if (llevaiva != 0)
                                                    {
                                                        #region Inventario con iva		

                                                        if (parametro.id_nombre_parametro == 9)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == lineas.codigo);
                                                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaInven = pcr.cuenta_inv_compra;
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region Si tiene perfil y cuenta asignada a ese perfil

                                                                if (cuentaInven != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento =
                                                                        Convert.ToString(modelo.pedido);

                                                                    cuenta_puc infoReferencias = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaInven)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencias.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencias.documeto)
                                                                    {
                                                                        movNuevo.documento = modelo.documento;
                                                                    }

                                                                    if (infoReferencias.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencias.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencias.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region Si tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento =
                                                                        Convert.ToString(modelo.pedido);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento = modelo.documento;
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);

                                                                    #endregion
                                                                }
                                                            }

                                                            #endregion

                                                            #region No tiene prefil la referencia

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
                                                                {

                                                                }*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = modelo.documento;
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(baseUnitario, miCultura);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region Buscamos si ya esta o si creamos el registro en mov contable

                                                            mov_contable buscarInventario =
                                                                context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre ==
                                                                    parametro.id_nombre_parametro);
                                                            if (buscarInventario != null)
                                                            {
                                                                buscarInventario.basecontable += movNuevo.basecontable;
                                                                buscarInventario.debito += movNuevo.debito;
                                                                buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                buscarInventario.credito += movNuevo.credito;
                                                                buscarInventario.creditoniif += movNuevo.creditoniif;
                                                                context.Entry(buscarInventario).State =
                                                                    EntityState.Modified;
                                                                context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    centro = parametro.centro,
                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Compra de repuestos con consecutivo " +
                                                                    consecutivo,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        #region Inventario	sin iva			

                                                        if (parametro.id_nombre_parametro == 25)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == lineas.codigo);
                                                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaInven = pcr.cuenta_inv_compra;
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region Si tiene perfil y cuenta asignada a ese perfil

                                                                if (cuentaInven != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento =
                                                                        Convert.ToString(modelo.pedido);

                                                                    cuenta_puc infoReferencias = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaInven)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencias.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencias.documeto)
                                                                    {
                                                                        movNuevo.documento = modelo.documento;
                                                                    }

                                                                    if (infoReferencias.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencias.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencias.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region Si tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento =
                                                                        Convert.ToString(modelo.pedido);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento = modelo.documento;
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);

                                                                    #endregion
                                                                }
                                                            }

                                                            #endregion

                                                            #region No tiene prefil la referencia

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = modelo.documento;
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(baseUnitario, miCultura);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region Buscamos si ya esta o si creamos el registro en mov contable

                                                            mov_contable buscarInventario =
                                                                context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre ==
                                                                    parametro.id_nombre_parametro);
                                                            if (buscarInventario != null)
                                                            {
                                                                buscarInventario.basecontable += movNuevo.basecontable;
                                                                buscarInventario.debito += movNuevo.debito;
                                                                buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                buscarInventario.credito += movNuevo.credito;
                                                                buscarInventario.creditoniif += movNuevo.creditoniif;
                                                                context.Entry(buscarInventario).State =
                                                                    EntityState.Modified;
                                                                context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    centro = parametro.centro,
                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Compra de repuestos con consecutivo " +
                                                                    consecutivo,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion
                                                    }

                                                    secuencia++;
                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    DateTime fechaHoy = DateTime.Now;
                                                    cuentas_valores buscar_cuentas_valores =
                                                        context.cuentas_valores.FirstOrDefault(x =>
                                                            x.centro == parametro.centro &&
                                                            x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit &&
                                                            x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    totalCreditos += movNuevo.credito;
                                                    totalDebitos += movNuevo.debito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta =
                                                            "(" + buscarCuenta.cntpuc_numero + ")" +
                                                            buscarCuenta.cntpuc_descp,
                                                        DescripcionParametro = descripcionParametro,
                                                        ValorDebito = movNuevo.debito,
                                                        ValorCredito = movNuevo.credito
                                                    });
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                #region validaciones para guardar
                                decimal totaldebitos22 = Math.Round(totalDebitos);
                                decimal totalcreditos22 = Math.Round(totalCreditos);
                                if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                {
                                    TempData["mensaje_error"] =
                                        "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                    ViewBag.documentoSeleccionado = encabezado.tipo;
                                    ViewBag.bodegaSeleccionado = encabezado.bodega;
                                    ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                    ViewBag.documentoDescuadrado = listaDescuadrados;
                                    ViewBag.calculoDebito = totalDebitos;
                                    ViewBag.calculoCredito = totalCreditos;
                                    dbTran.Rollback();
                                    listas();
                                    BuscarFavoritos(menu);
                                    return View(modelo);
                                }

                                funciono = 1;
                                if (funciono > 0)
                                {
                                    context.SaveChanges();
                                    dbTran.Commit();
                                    TempData["mensaje"] = "registro creado correctamente";
                                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                    if (documentointerno != 0)
                                    {
                                        doc.ActualizarConsecutivo(grupo2.grupo, consecutivo2);

                                    }
                                    return RedirectToAction("compraManual");
                                }

                                #endregion
                            }
                            else
                            {
                                TempData["mensaje_error"] = "no hay consecutivo";
                            }
                        }
                        //cierre
                        else
                        {
                            TempData["mensaje_error"] = "Lista vacia";
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listas();
            BuscarFavoritos(menu);
            return RedirectToAction("compraManual", "compraRepuestos", new { menu });
        }

        public JsonResult validarNumFactura(int? proveedor, int? tp_doc, string numFactura)
        {
            int bexiste = 0;
            var existe = (from x in context.encab_documento
                          where x.nit == proveedor && x.tipo == tp_doc && x.documento == numFactura
                          select new { x.idencabezado }).FirstOrDefault();
            if (existe != null)
            {
                bexiste = 1;
            }

            return Json(bexiste, JsonRequestBehavior.AllowGet);
        }

        public bool calcularCostoPromedio(decimal cantidad_recibir, decimal valor_unitario, string codigo, int bodega)
        {


            decimal nuevo_costo_promedio = 0;
            var kardex2 = (from x in context.vw_kardex2
                           where x.codigo == codigo && x.bodega == bodega
                           select new
                           {
                               x.stock,
                               x.costoProm,
                               x.ano,
                               x.mes
                           }).OrderByDescending(m => new { m.ano, m.mes }).FirstOrDefault();





            if (kardex2 != null)
            {
                if (kardex2.stock != 0)
                {
                    nuevo_costo_promedio = Math.Round(Convert.ToDecimal(
                        (kardex2.stock * kardex2.costoProm + cantidad_recibir * valor_unitario) /
                        (kardex2.stock + cantidad_recibir), miCultura));
                }
            }
            else
            {
                nuevo_costo_promedio = Math.Round(valor_unitario);
            }

            //icb_referencia referencia = context.icb_referencia.Find(codigo);
            //referencia.costo_promedio = Convert.ToDecimal(nuevo_costo_promedio, miCultura);
            //context.Entry(referencia).State = EntityState.Modified;

            //Se guarda el costo promedio en la referencias_inven

            referencias_inven referencia_ = context.referencias_inven.OrderByDescending(x => new { x.ano, x.mes }).Where(x => x.codigo == codigo && x.bodega == bodega).FirstOrDefault();
            referencia_.costo_prom = Convert.ToDecimal(nuevo_costo_promedio, miCultura);
            context.Entry(referencia_).State = EntityState.Modified;


            int actualiza_costo = context.SaveChanges();
            if (actualiza_costo > 0)
            {
                return true;
            }

            return false;
        }

        public ActionResult ComprasRealizadas(int? menu)
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 14);
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult DetalleCompras(int id, int? menu)
        {
            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;
            ViewBag.facid = lineas.encab_documento.idencabezado;
            ViewBag.idEncabezado = id;
            ViewBag.factura = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.numero)
                .FirstOrDefault();
            ViewBag.fecha = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.fec_creacion)
                .FirstOrDefault();
            ViewBag.fechaVencimiento = context.encab_documento.Where(x => x.idencabezado == id)
                .Select(x => x.vencimiento).FirstOrDefault();
            ViewBag.tipoDocumento = (from a in context.encab_documento
                                     join b in context.tp_doc_registros
                                         on a.tipo equals b.tpdoc_id
                                     where a.idencabezado == id
                                     select b.tpdoc_nombre).FirstOrDefault();

         var fecha = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.fecha).FirstOrDefault();


            ViewBag.fechafactura = fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"));

            ViewBag.bodega = (from a in context.encab_documento
                              join b in context.bodega_concesionario
                                  on a.bodega equals b.id
                              where a.idencabezado == id
                              select b.bodccs_nombre).FirstOrDefault();

            var proveedor = (from a in context.encab_documento
                             join b in context.icb_terceros
                                 on a.nit equals b.tercero_id
                             where a.idencabezado == id
                             select new
                             {
                                 b.prinom_tercero,
                                 b.segnom_tercero,
                                 b.apellido_tercero,
                                 b.segapellido_tercero,
                                 b.razon_social
                             }).FirstOrDefault();
            ViewBag.proveedor = proveedor.razon_social + " " + proveedor.prinom_tercero + " " +
                                proveedor.segnom_tercero + " " + proveedor.apellido_tercero + " " +
                                proveedor.segapellido_tercero;

            ViewBag.tipoPago = (from c in context.encab_documento
                                join f in context.fpago_tercero
                                    on c.fpago_id equals f.fpago_id
                                where c.idencabezado == id
                                select f.fpago_nombre).FirstOrDefault();

            ViewBag.moneda = (from c in context.encab_documento
                              join f in context.monedas
                                  on c.moneda equals f.moneda
                              where c.idencabezado == id
                              select f.descripcion).FirstOrDefault();

            ViewBag.perfil = (from a in context.encab_documento
                              join b in context.perfil_contable_documento
                                  on a.perfilcontable equals b.id
                              where a.idencabezado == id
                              select b.descripcion).FirstOrDefault();

            ViewBag.fletes = (from a in context.encab_documento
                              where a.idencabezado == id
                              select a.fletes).FirstOrDefault();

            ViewBag.ivafletes = (from a in context.encab_documento
                                 where a.idencabezado == id
                                 select a.iva_fletes).FirstOrDefault();

            var asesor = (from a in context.encab_documento
                          join b in context.users
                              on a.solicitadopor equals b.user_id
                          where a.idencabezado == id
                          select new
                          {
                              b.user_nombre,
                              b.user_apellido,
                              a.solicitadopor
                          }).FirstOrDefault();

            ViewBag.asesor = asesor != null ? asesor.user_nombre + " " + asesor.user_apellido : "";

            ViewBag.concepto1 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.concepto2 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos2
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.observaciones = (from a in context.encab_documento
                                     where a.idencabezado == id
                                     select a.notas != null ? a.notas : "").FirstOrDefault();

            ViewBag.pedido = (from a in context.encab_documento
                              join b in context.icb_referencia_mov
                                  on a.pedido equals b.refmov_id
                              where a.idencabezado == id
                              select b.refmov_numero).FirstOrDefault();

            ViewBag.facturaProveedor = (from a in context.encab_documento
                                        where a.idencabezado == id
                                        select a.documento).FirstOrDefault();

            ViewBag.remision = context.encab_documento.FirstOrDefault(x => x.idencabezado == id).remision;

            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult DetalleCompras(int? menu)
        {
            int idEncabezado = Convert.ToInt32(Request["idEncabezado"]);

            encab_documento buscar = context.encab_documento.Find(idEncabezado);

            if (buscar != null)
            {
                buscar.documento = Request["facturaProveedor"];
                context.Entry(buscar).State = EntityState.Modified;
                context.SaveChanges();
            }

            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == idEncabezado);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;
            ViewBag.facid = lineas.encab_documento.idencabezado;
            ViewBag.idEncabezado = idEncabezado;
            ViewBag.remision = context.encab_documento.FirstOrDefault(x => x.idencabezado == idEncabezado).remision;
            ViewBag.factura = context.encab_documento.Where(x => x.idencabezado == idEncabezado).Select(x => x.numero)
                .FirstOrDefault();
            ViewBag.fecha = context.encab_documento.Where(x => x.idencabezado == idEncabezado)
                .Select(x => x.fec_creacion).FirstOrDefault();
            ViewBag.fechaVencimiento = context.encab_documento.Where(x => x.idencabezado == idEncabezado)
                .Select(x => x.vencimiento).FirstOrDefault();
            ViewBag.tipoDocumento = (from a in context.encab_documento
                                     join b in context.tp_doc_registros
                                         on a.tipo equals b.tpdoc_id
                                     where a.idencabezado == idEncabezado
                                     select b.tpdoc_nombre).FirstOrDefault();

            ViewBag.bodega = (from a in context.encab_documento
                              join b in context.bodega_concesionario
                                  on a.bodega equals b.id
                              where a.idencabezado == idEncabezado
                              select b.bodccs_nombre).FirstOrDefault();

            var proveedor = (from a in context.encab_documento
                             join b in context.icb_terceros
                                 on a.nit equals b.tercero_id
                             where a.idencabezado == idEncabezado
                             select new
                             {
                                 b.prinom_tercero,
                                 b.segnom_tercero,
                                 b.apellido_tercero,
                                 b.segapellido_tercero,
                                 b.razon_social
                             }).FirstOrDefault();
            ViewBag.proveedor = proveedor.razon_social + " " + proveedor.prinom_tercero + " " +
                                proveedor.segnom_tercero + " " + proveedor.apellido_tercero + " " +
                                proveedor.segapellido_tercero;

            ViewBag.tipoPago = (from c in context.encab_documento
                                join f in context.fpago_tercero
                                    on c.fpago_id equals f.fpago_id
                                where c.idencabezado == idEncabezado
                                select f.fpago_nombre).FirstOrDefault();

            ViewBag.moneda = (from c in context.encab_documento
                              join f in context.monedas
                                  on c.moneda equals f.moneda
                              where c.idencabezado == idEncabezado
                              select f.descripcion).FirstOrDefault();

            ViewBag.perfil = (from a in context.encab_documento
                              join b in context.perfil_contable_documento
                                  on a.perfilcontable equals b.id
                              where a.idencabezado == idEncabezado
                              select b.descripcion).FirstOrDefault();

            ViewBag.fletes = (from a in context.encab_documento
                              where a.idencabezado == idEncabezado
                              select a.fletes).FirstOrDefault();

            ViewBag.ivafletes = (from a in context.encab_documento
                                 where a.idencabezado == idEncabezado
                                 select a.iva_fletes).FirstOrDefault();

            var asesor = (from a in context.encab_documento
                          join b in context.users
                              on a.solicitadopor equals b.user_id
                          where a.idencabezado == idEncabezado
                          select new
                          {
                              b.user_nombre,
                              b.user_apellido,
                              a.solicitadopor
                          }).FirstOrDefault();

            ViewBag.asesor = asesor != null ? asesor.user_nombre + " " + asesor.user_apellido : "";

            ViewBag.concepto1 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos
                                     on a.concepto equals b.id
                                 where a.idencabezado == idEncabezado
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.concepto2 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos2
                                     on a.concepto equals b.id
                                 where a.idencabezado == idEncabezado
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.observaciones = (from a in context.encab_documento
                                     where a.idencabezado == idEncabezado
                                     select a.notas != null ? a.notas : "").FirstOrDefault();

            ViewBag.pedido = (from a in context.encab_documento
                              join b in context.icb_referencia_mov
                                  on a.pedido equals b.refmov_id
                              where a.idencabezado == idEncabezado
                              select b.refmov_numero).FirstOrDefault();

            ViewBag.facturaProveedor = (from a in context.encab_documento
                                        where a.idencabezado == idEncabezado
                                        select a.documento).FirstOrDefault();

            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == idEncabezado).ToList();
            BuscarFavoritos(menu);

            return View();
        }

        public ActionResult DetalleDevoluciones(int id, int? menu)
        {
            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;

            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
            BuscarFavoritos(menu);
            return View(datos);
        }

        //browser pre cargue
        public JsonResult BuscarDatosPrecargue()
        {
            var data = (from p in context.rprecarga
                        select new
                        {
                            p.numero,
                            fecha = p.fec_creacion.ToString()
                            //codigo = n.Select(c => c.codigo),
                            //documento = n.Select(c => c.documento),
                            //cant_fac = n.Select( c => c.cant_fact),
                            //valor_uni = n.Select(c => c.valor_unitario),
                            //cant_ped = n.Select(c => c.cant_ped),
                            //pedido_int = n.Select(c => c.pedidoint),
                            //pedido_gm = n.Select(c => c.pedidogm),
                            //valor_total = n.Select(c => c.valor_total),
                            //valor_totalencab = n.Select(c => c.valor_totalenca),
                            //cant_real = n.Select(c => c.cant_real)
                        }).ToList().Distinct();

            //var data = context.rprecarga.Where(x=> x);


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //browser compra
        public JsonResult BuscarDatosCompra()
        {
            var datos = (from e in context.encab_documento
                         join b in context.bodega_concesionario
                             on e.bodega equals b.id
                         join t in context.icb_terceros
                             on e.nit equals t.tercero_id
                         join tp in context.tp_doc_registros
                             on e.tipo equals tp.tpdoc_id
                         where tp.tipo == 3
                         select new
                         {
                             tipoDocumento = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                             e.numero,
                             nit = t.prinom_tercero != null
                                 ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                   t.apellido_tercero + " " + t.segapellido_tercero
                                 : "(" + t.doc_tercero + ") " + t.razon_social,
                             e.fecha,
                             e.valor_total,
                             id = e.idencabezado,
                             bodega = b.bodccs_nombre,
                             estado = "",
                             fecha_devolucion = "",
                             facturaProveedor = e.documento
                         }).ToList();
            var data = datos.Select(x => new
            {
                x.tipoDocumento,
                x.numero,
                x.nit,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm"),
                x.valor_total,
                x.id,
                x.bodega,
                x.estado,
                x.fecha_devolucion,
                x.facturaProveedor
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //browser backorder
        public JsonResult BuscarDatosBackorder()
        {
            var info = (from rp in context.rprecarga
                        join r in context.icb_referencia
                            on rp.codigo equals r.ref_codigo into a
                        from r in a.DefaultIfEmpty()
                        /*join cp in context.vw_promedio
                            on new { Key1 = rp.codigo, Key2 = rp.fec_creacion.Year, Key3 = rp.fec_creacion.Month } equals new { Key1 = cp.codigo, Key2 = (int)cp.ano, Key3 = (int)cp.mes } into b
                        from cp in b.DefaultIfEmpty()*/
                        join inv in context.vw_inventario_hoy
                            on r.ref_codigo equals inv.ref_codigo into b
                        from inv in b.DefaultIfEmpty()
                        join ccs in context.bodega_concesionario
                            on inv.bodega equals ccs.id into cs
                        from ccs in cs.DefaultIfEmpty()
                        where /*!rp.seleccion &&*/ rp.cant_ped - rp.cant_fact > 0
                        orderby rp.id
                        select new
                        {
                            rp.numero,
                            bodega = ccs.bodccs_nombre,
                            rp.codigo,
                            r.ref_codigo,
                            r.ref_descripcion,
                            rp.pedidogm,
                            rp.cant_ped,
                            rp.cant_fact,
                            rp.fec_creacion,
                        }).ToList().GroupBy(x => new { x.numero, x.pedidogm, x.codigo, x.cant_ped }).ToList();

            var data = info.Select(y => new
            {
                fec_creacion = y.Select(x => x.fec_creacion).FirstOrDefault().ToString("dd/MM/yyyy hh:mm tt"),
                numero = y.Select(x => x.numero).FirstOrDefault(),
                bodega = y.Select(x => !string.IsNullOrWhiteSpace(x.bodega) ? x.bodega : "").FirstOrDefault(),
                ref_codigo= y.Select(x => !string.IsNullOrWhiteSpace(x.ref_codigo) ? x.ref_codigo:"").FirstOrDefault(),
                code_ref = y.Select(x => x.codigo != null ? x.codigo : "").FirstOrDefault(),
                des_ref = y.Select(x => !string.IsNullOrWhiteSpace(x.ref_descripcion) ? x.ref_descripcion : "").FirstOrDefault(),
                existe = y.Select(x => !string.IsNullOrWhiteSpace(x.ref_descripcion) ? 1 : 0).FirstOrDefault(),
                nro_pedido_gm = y.Select(x => !string.IsNullOrWhiteSpace(x.pedidogm) ? x.pedidogm : "").FirstOrDefault(),
                cant_pedida = y.Select(x => x.cant_ped != null ? x.cant_ped : 0).FirstOrDefault(),
                cant_facturada = y.Select(x => x.cant_fact != null ? x.cant_fact : 0).Sum(),
                cant_backorder = y.Select(x => x.cant_ped != null ? x.cant_ped : 0).FirstOrDefault() - y.Select(x => x.cant_fact != null ? x.cant_fact : 0).Sum()
            }).Where(x => x.cant_backorder > 0).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosDevolucion()
        {
            var data = from e in context.encab_documento
                       join b in context.bodega_concesionario
                           on e.bodega equals b.id
                       join t in context.icb_terceros
                           on e.nit equals t.tercero_id
                       join tp in context.tp_doc_registros
                           on e.tipo equals tp.tpdoc_id
                       where tp.tipo == 14
                       select new
                       {
                           tipoDocumento = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                           e.numero,
                           nit = t.prinom_tercero != null
                               ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                 t.apellido_tercero + " " + t.segapellido_tercero
                               : "(" + t.doc_tercero + ") " + t.razon_social,
                           fecha = e.fecha.ToString(),
                           e.valor_total,
                           id = e.idencabezado,
                           bodega = b.bodccs_nombre,
                           estado = "",
                           facturaProveedor = e.documento,
                           fecha_devolucion = e.fec_actualizacion.Value.ToString()
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilPorDocumento(int tipo)
        {
            var data = (from t in context.perfil_contable_documento
                        where t.tipo == tipo
                        select new
                        {
                            t.id,
                            perfil = t.descripcion
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilPorBodega(int bodega, int tipoD)
        {
            var data = (from b in context.perfil_contable_bodega
                        join t in context.perfil_contable_documento
                            on b.idperfil equals t.id
                        where b.idbodega == bodega && t.tipo == tipoD
                        select new
                        {
                            id = b.idperfil,
                            perfil = t.descripcion
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilPorBodegaProveedor(int bodega, int tipoD, int proveedor_id)
        {
            icb_terceros proveedorIva = context.icb_terceros.Where(d => d.tercero_id == proveedor_id).FirstOrDefault();
            //(from proveedor in context.icb_terceros
            //	where proveedor.tercero_id == proveedor_id
            //	select new {proveedor.tpregimen_id}).FirstOrDefault();
            string proveedorIva2 = "";
            int tpregimen = 0;
            if (proveedorIva != null)
            {
                tpregimen = proveedorIva.tpregimen_id != null ? proveedorIva.tpregimen_id.Value : 0;
                proveedorIva2 = "A";
                // if (proveedorIva.exentoiva)
                //	proveedorIva2 = "N";
                //else if (proveedorIva.exentoiva == false) proveedorIva2 = "A";
            }

            var data = (from b in context.perfil_contable_bodega
                        join t in context.perfil_contable_documento
                            on b.idperfil equals t.id
                        where b.idbodega == bodega && t.tipo == tipoD
                        select new
                        {
                            id = b.idperfil,
                            perfil = t.descripcion,
                            t.iva,
                            t.regimen_tercero
                        }).ToList();
            var data2 = data;
            int existeiva = data.Where(d => d.iva != null).Count();
            if (existeiva > 0)
            {
                data2 = data.Where(d => d.iva == proveedorIva2 && d.regimen_tercero == tpregimen).ToList();
            }

            if (data2.Count == 0)
            {
                data2 = data;
            }

            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCondicionPorTercero(int id)
        {
            var data = (from t in context.fpago_tercero
                        join f in context.tercero_proveedor
                            on t.fpago_id equals f.fpago_id
                        where f.tercero_id == id
                        select new
                        {
                            id = t.fpago_id,
                            nombre = t.fpago_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReemplazos(string codigo, int bodega)
        {
            int existeREF = (from e in context.rremplazos
                             where e.referencia == codigo
                             select e).Count();

            int existeAL = (from e in context.rremplazos
                            where e.alterno == codigo
                            select e).Count();

            if (existeREF > 0)
            {
                rremplazos padre = (from p in context.rremplazos
                                    where p.referencia == codigo
                                    select p).FirstOrDefault();

                var data = (from r in context.rremplazos
                            join v in context.vw_inventario_hoy
                                on new { b = bodega, refAl = r.referencia } equals new { b = v.bodega, refAl = v.ref_codigo } into
                                tmp
                            from v in tmp.DefaultIfEmpty()
                            where r.idpadre == padre.idpadre
                            select new
                            {
                                id = r.alterno,
                                nombre = "(" + r.alterno + ") " + r.descripcion,
                                stock = v.stock != null ? v.stock : 0
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (existeAL > 0)
            {
                rremplazos padre = (from p in context.rremplazos
                                    where p.alterno == codigo
                                    select p).FirstOrDefault();

                var data = (from r in context.rremplazos
                            join v in context.vw_inventario_hoy
                                on new { b = bodega, refAl = r.alterno } equals new { b = v.bodega, refAl = v.ref_codigo } into tmp
                            from v in tmp.DefaultIfEmpty()
                            where r.idpadre == padre.idpadre
                            select new
                            {
                                id = r.alterno,
                                nombre = "(" + r.alterno + ") " + r.descripcion,
                                stock = v.stock != null ? v.stock : 0
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int data = 0;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DiferenciasCompraRepuestos(int numero, int? menu)
        {
            ViewBag.numeroCompra = numero;
            return View();
        }

        public JsonResult buscarDiferenciasDatos(int numero)
        {
            string parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P60").syspar_value;
            var difCantidad = (from rp in context.rprecarga
                               join r in context.icb_referencia
                                   on rp.codigo equals r.ref_codigo into temp
                               from r in temp.DefaultIfEmpty()
                               join t in context.icb_terceros
                                   on parametro equals t.doc_tercero
                               where rp.difcantidad == 1 && rp.numero == numero
                               select new
                               {
                                   referencia = r.ref_descripcion != null ? "(" + rp.codigo + ") " + r.ref_descripcion : rp.codigo,
                                   rp.numero,
                                   rp.pedidoint,
                                   rp.pedidogm,
                                   rp.documento,
                                   rp.valor_unitario,
                                   rp.cant_ped,
                                   rp.cant_fact,
                                   rp.cant_real,
                                   nit = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                         t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social,
                                   fecha = (from rp in context.rprecarga
                                            where rp.pedidoint == rp.pedidoint
                                            select new
                                            {
                                                fechas = rp.fecha.ToString()
                                            }).FirstOrDefault()
                               }).ToList();

            var difCostos = (from rp in context.rprecarga
                             join r in context.icb_referencia
                                 on rp.codigo equals r.ref_codigo into temp
                             from r in temp.DefaultIfEmpty()
                             join cp in context.vw_promedio
                                 on new { Key1 = rp.codigo, Key2 = rp.fec_creacion.Year, Key3 = rp.fec_creacion.Month } equals new { Key1 = cp.codigo, Key2 = (int)cp.ano, Key3 = (int)cp.mes } into b
                             from cp in b.DefaultIfEmpty()
                             join p in context.rprecios
                                 on rp.codigo equals p.codigo into c
                             from p in c.DefaultIfEmpty()
                             join t in context.icb_terceros
                                 on parametro equals t.doc_tercero
                             where rp.difcosto == 1 && rp.numero == numero
                             select new
                             {
                                 referencia = r.ref_descripcion != null ? "(" + rp.codigo + ") " + r.ref_descripcion : rp.codigo,
                                 rp.numero,
                                 rp.pedidoint,
                                 rp.pedidogm,
                                 rp.documento,
                                 rp.valor_unitario,
                                 Promedio = cp.Promedio != null ? cp.Promedio : 0,
                                 precio1 = p.precio1 != null ? p.precio1 : 0,
                                 nit = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                       t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social,
                                 fecha = (from rp in context.rprecarga
                                          where rp.pedidoint == rp.pedidoint
                                          select new
                                          {
                                              fechas = rp.fecha.ToString()
                                          }).FirstOrDefault()
                             }).ToList();

            return Json(new { difCantidad, difCostos }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarRetenciones(int tipo_doc, int nit, int bodega, decimal subTotal, decimal totalDes,
            decimal totalIVA)
        {
            decimal retefuente = 0;
            decimal reteica = 0;
            decimal reteiva = 0;

            decimal baseiva = 0;
            decimal pretiva = 0;
            decimal baseica = 0;
            decimal pretica = 0;
            decimal baseretencion = 0;
            decimal pretretencion = 0;
            // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
            tercero_proveedor buscarProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == nit);
            icb_terceros buscarRetenciones = context.icb_terceros.FirstOrDefault(x => x.tercero_id == nit);
            int? regimen_proveedor = buscarRetenciones != null ? buscarRetenciones.tpregimen_id : 0;
            if (buscarRetenciones.retfuente == null)
            {
                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                    x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                    x.tipo_regimenid == buscarRetenciones.tpregimen_id);
                if (buscarPerfilTributario != null)
                {
                    baseretencion = buscarPerfilTributario.baseretfuente != null ? buscarPerfilTributario.baseretfuente.Value : 0;
                    pretretencion = buscarPerfilTributario.pretfuente != null ? buscarPerfilTributario.pretfuente.Value : 0;
                    baseiva = buscarPerfilTributario.baseretiva != null ? buscarPerfilTributario.baseretiva.Value : 0;
                    pretiva = buscarPerfilTributario.pretiva != null ? buscarPerfilTributario.pretiva.Value : 0;
                    baseica = buscarPerfilTributario.baseretica != null ? buscarPerfilTributario.baseretica.Value : 0;
                    pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;
                    if (buscarPerfilTributario.retfuente == "A" &&
                        subTotal - totalDes >= baseretencion)
                    {
                        retefuente = Math.Round((subTotal - totalDes) *
                                                Convert.ToDecimal(pretretencion / 100, miCultura));
                    }

                    if (buscarPerfilTributario.retiva == "A" && totalIVA >= baseiva)
                    {
                        reteiva = Math.Round(totalIVA * Convert.ToDecimal(pretiva / 100, miCultura));
                    }

                    if (buscarPerfilTributario.retica == "A" && subTotal - totalDes >= baseica)
                    {
                        terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                            x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
                        decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                        if (bodega_acteco != null)
                        {
                            reteica = Math.Round((subTotal - totalDes) *
                                                 Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                        }

                        if (tercero_acteco != 0)
                        {
                            reteica = Math.Round((subTotal - totalDes) *
                                                 Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                        }
                        else
                        {
                            reteica = Math.Round((subTotal - totalDes) *
                                                 Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                        }
                    }
                }
            }
            //var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);

            if (buscarRetenciones.retfuente != null)
            {
                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                    x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                    x.tipo_regimenid == buscarRetenciones.tpregimen_id);
                if (buscarPerfilTributario != null)
                {
                    baseretencion = buscarPerfilTributario.baseretfuente != null ? buscarPerfilTributario.baseretfuente.Value : 0;
                    pretretencion = buscarPerfilTributario.pretfuente != null ? buscarPerfilTributario.pretfuente.Value : 0;
                    baseiva = buscarPerfilTributario.baseretiva != null ? buscarPerfilTributario.baseretiva.Value : 0;
                    pretiva = buscarPerfilTributario.pretiva != null ? buscarPerfilTributario.pretiva.Value : 0;
                    baseica = buscarPerfilTributario.baseretica != null ? buscarPerfilTributario.baseretica.Value : 0;
                    pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;
                }
                else
                {
                    baseretencion = buscarTipoDocRegistro.baseretencion;
                    pretretencion = Convert.ToDecimal(buscarTipoDocRegistro.retencion.ToString(), miCultura);
                    baseiva = Convert.ToDecimal(buscarTipoDocRegistro.baseiva, miCultura);
                    pretiva = Convert.ToDecimal(buscarTipoDocRegistro.retiva.ToString(), miCultura);
                    baseica = Convert.ToDecimal(buscarTipoDocRegistro.baseica, miCultura);
                    //pretica = buscarPerfilTributario.pretica != null ? buscarPerfilTributario.pretica.Value : 0;
                }

                if (buscarRetenciones.retfuente == "A" && subTotal - totalDes >= baseretencion)
                {
                    retefuente = Math.Round((subTotal - totalDes) *
                                            Convert.ToDecimal(pretretencion / 100, miCultura));
                }

                if (buscarRetenciones.retiva == "A" && totalIVA >= baseiva)
                {
                    reteiva = Math.Round(totalIVA * Convert.ToDecimal(pretiva / 100, miCultura));
                }

                if (buscarRetenciones.retica == "A" && subTotal - totalDes >= baseica)
                {
                    terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                        x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
                    decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                    if (bodega_acteco != null)
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                    }

                    if (tercero_acteco != 0)
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                    }
                    else
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                    }
                }
            }

            var data = new
            {
                retefuente,
                reteica,
                reteiva
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult existeReferencia(string alterno)
        {
            int data = (from r in context.icb_referencia
                        where r.ref_codigo == alterno
                        select r).Count();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarIva(string codigo)
        {
            var data = (from r in context.icb_referencia
                        where r.ref_codigo == codigo
                        select new
                        {
                            iva = r.por_iva_compra
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarMoneda(int moneda)
        {
            DateTime fecha = DateTime.Now;

            var data = (from t in context.moneda_conversion
                        where t.fecha.Day == fecha.Day
                              && t.fecha.Month == fecha.Month
                              && t.fecha.Year == fecha.Year
                              && t.idmoneda == moneda
                        select new
                        {
                            t.conversion,
                            t.id
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormaPago(int id)
        {
            var data = (from c in context.tercero_proveedor
                        join f in context.fpago_tercero
                            on c.fpago_id equals f.fpago_id
                        where c.tercero_id == id
                        select new
                        {
                            id = f.fpago_id,
                            fpago = f.fpago_nombre
                        }).FirstOrDefault();
            if (data == null)
            {
                return Json(new { info = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarOrdenesCompra(int proveedor, int bodega)
        {
            var info = (from o in context.rordencompra
                        join dr in context.rdetalleordencompra 
                        on o.idorden equals dr.idencaborden
                        where o.proveedor == proveedor && o.bodega == bodega
                        && dr.cantidad - dr.despachada > 0
                        select  new
                        {
                            o.idorden,
                            o.numero,
                            o.fec_creacion
                        }).Distinct().ToList();

            var info2 = info.Select(x => new
            {
                x.idorden,
                x.numero,
                fec_creacion = x.fec_creacion.ToShortDateString()
            }).ToList();
            var data = new
            {
                info2
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRepuestosOrdenesCompra(int[] oc)
        {
            var data = (from dr in context.rdetalleordencompra
                        join r in context.icb_referencia
                            on dr.codigo_referencia equals r.ref_codigo
                        join orc in context.rordencompra on
                        dr.idencaborden equals orc.idorden
                        where  oc.Contains(dr.idencaborden)
                        select new
                        {
                            dr.id,
                            dr.idencaborden,
                            dr.codigo_referencia,
                            r.ref_descripcion,
                            dr.seq,
                            cantidad = dr.cantidad - dr.despachada,
                            dr.valorunitario,
                            dr.porceniva,
                            dr.porcendescto,
                            orc.ivafletes,
                            orc.fletes,
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDetalleCompras(int id)
        {
            var info = (from l in context.lineas_documento
                        join r in context.icb_referencia
                            on l.codigo equals r.ref_codigo
                        where l.id_encabezado == id
                        select new
                        {
                            l.codigo,
                            r.ref_descripcion,
                            l.fec,
                            l.valor_unitario,
                            l.cantidad_pedida,
                            l.cantidad
                        }).ToList();

            var data = info.Select(l => new
            {
                referencia = l.codigo + " -  " + l.ref_descripcion,
                fecha = l.fec.ToString("yyyy/MM/dd"),
                valor = l.valor_unitario,
                cantidad = l.cantidad_pedida,
                cantidadReal = l.cantidad,
                total = l.cantidad * l.valor_unitario
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarUbicaciones(string codigo)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            var buscar = (from a in context.ubicacion_repuesto
                          join u in context.ubicacion_repuestobod on
                              a.ubicacion equals u.id
                          join b in context.estanterias
                              on u.id_estanteria equals b.id
                          join c in context.area_bodega
                              on b.id_area equals c.areabod_id
                          where a.bodega == bodega && a.codigo == codigo
                          select new
                          {
                              ubicacion = u.descripcion,
                              estanteria = b.descripcion,
                              c.areabod_nombre
                          }
                ).Distinct().OrderBy(x => x.areabod_nombre).ToList();

            var data = buscar.Select(x => new
            {
                x.ubicacion,
                x.estanteria,
                x.areabod_nombre
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPermisoModificar()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 28
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            return Json(permiso, JsonRequestBehavior.AllowGet);
        }
        public JsonResult buscarPermisoFechacompra()
            {

            int rol = Convert.ToInt32(Session["user_rolid"]);
            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol
                            select new { rolPerm.codigo }).ToList();

            var resultado1 = permisos.Where(x => x.codigo == "P54").Count() > 0 ? "Si" : "No";

            return Json(resultado1, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Cambiarfecha(DateTime fecha, int id)
            {
            encab_documento encab_documento = context.encab_documento.Where(x => x.idencabezado == id).FirstOrDefault();
            encab_documento.fecha = fecha;
            context.Entry(encab_documento).State = EntityState.Modified;
            context.SaveChanges();


            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public JsonResult ModificarFactura(string facturaNueva, string motivoCambio, int idEncabezado)
        {
            encab_documento buscarDocumento = context.encab_documento.Find(idEncabezado);

            factura_modificada nuevo = new factura_modificada
            {
                id_encabezado = idEncabezado,
                numero_anterior = buscarDocumento.documento,
                numero_nuevo = facturaNueva,
                usuario_modifica = Convert.ToInt32(Session["user_usuarioid"]),
                fecha_modifica = DateTime.Now,
                motivo = motivoCambio
            };
            context.factura_modificada.Add(nuevo);
            context.SaveChanges();

            buscarDocumento.fec_actualizacion = DateTime.Now;
            buscarDocumento.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            buscarDocumento.documento = facturaNueva;
            context.Entry(buscarDocumento).State = EntityState.Modified;

            context.SaveChanges();

            return Json(facturaNueva, JsonRequestBehavior.AllowGet);
        }

        public JsonResult busqueda(int idEncabezado)
        {
            var buscar = (from a in context.factura_modificada
                          join b in context.users
                              on a.usuario_modifica equals b.user_id
                          where a.id_encabezado == idEncabezado
                          select new { a, b }).ToList();
            var data = buscar.Select(x => new
            {
                x.a.numero_anterior,
                x.a.numero_nuevo,
                usuario = x.b.user_nombre + " " + x.b.user_apellido,
                fecha = x.a.fecha_modifica.ToString("yyyy/MM/dd HH:mm"),
                x.a.motivo
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult solicitudRepuestos()
        {
            var buscar = (from a in context.rsolicitudesrepuestos
                          join b in context.rtiposolicitudrepuesto
                              on a.tiposolicitud equals b.id
                          join c in context.bodega_concesionario
                              on a.bodega equals c.id
                          join d in context.icb_terceros
                              on a.cliente equals d.tercero_id into terceros
                          from d in terceros.DefaultIfEmpty()
                          join e in context.users
                              on a.usuario equals e.user_id
                          select new
                          {
                              bodega = c.bodccs_nombre,
                              a.fecha,
                              d.prinom_tercero,
                              d.segnom_tercero,
                              d.apellido_tercero,
                              d.segapellido_tercero,
                              d.doc_tercero,
                              e.user_nombre,
                              e.user_apellido,
                              a.Detalle,
                              b.Descripcion,
                              a.id,
                              cantidadRepuestos = (from aa in context.rdetallesolicitud
                                                   where aa.id_solicitud == a.id
                                                   select aa.referencia).Count(),
                              cantidad = (from aa in context.rdetallesolicitud
                                          where aa.id_solicitud == a.id
                                          select aa.cantidad).Sum()
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.bodega,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                cliente = x.doc_tercero != null ? "(" + x.doc_tercero + ") " + x.prinom_tercero + " " + x.segnom_tercero + " " +
                          x.apellido_tercero + " " + x.segapellido_tercero : "",
                usuario = x.user_nombre + " " + x.user_apellido,
                Detalle = x.Detalle != null ? x.Detalle : "",
                tipo = x.Descripcion,
                x.cantidadRepuestos,
                cantidad = x.cantidad != null ? x.cantidad : 0,
                x.id
            }).OrderByDescending(x => x.fecha);

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

        #region Codigo marley

        //public ActionResult DevolucionManual(int id, int? menu)
        //{
        //    ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 14);
        //    var encab = context.encab_documento.Find(id);
        //    ViewBag.numero = encab.numero;
        //    ViewBag.valor_total = encab.valor_total;
        //    ViewBag.fecha = encab.fec_creacion;

        //    //se cargan los datos del mismo numero que aun no se han comprado
        //    var datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
        //    BuscarFavoritos(menu);
        //    return View(datos);
        //}

        //[HttpPost]
        //public ActionResult DevolucionManual(IEnumerable<lineas_documento> lista, int? menu)
        //{
        //    var ids = Request["item.id"];
        //    string[] listaId = ids.Split(',');

        //    var tipo_doc = Convert.ToInt32(Request["selectTipoDocumento"]);
        //    var bodega = Convert.ToInt32(Request["selectBodegas"]);
        //    var perfil_seleccionado = Convert.ToInt32(Request["perfil"]);
        //    var id_devolucion = 0;

        //    decimal valor_totalenca = 0;
        //    decimal total_iva = 0;

        //    foreach (var item in listaId)
        //    {
        //        var id = Convert.ToInt32(item);
        //        var linea = context.lineas_documento.Find(id);
        //        id_devolucion = linea.id_encabezado;

        //        decimal vrtotal = linea.valor_unitario * Convert.ToDecimal(Request["cant_real_" + item]);
        //        var dcto = vrtotal * Convert.ToDecimal(linea.porcentaje_descuento) / 100;
        //        vrtotal = vrtotal - dcto;
        //        total_iva += (vrtotal) * ((Convert.ToDecimal(linea.porcentaje_iva)) / 100);
        //        valor_totalenca = valor_totalenca + vrtotal + total_iva;
        //    }


        //    encab_documento encabezado = new encab_documento();

        //    var devolucion = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_devolucion);

        //    //insertar un nuevo documento

        //    //consecutivo
        //    var grupo = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == tipo_doc && x.bodega_id == bodega).grupo;
        //    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
        //    var consecutivo = doc.BuscarConsecutivo(grupo);


        //    // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
        //    var buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
        //    var buscarProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == encabezado.nit);
        //    var regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
        //    var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
        //    decimal retenciones = 0;

        //    if (buscarPerfilTributario != null)
        //    {
        //        if (buscarPerfilTributario.retfuente == "A" && encabezado.costo >= buscarTipoDocRegistro.baseretencion)
        //        {
        //            encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
        //            encabezado.retencion = encabezado.costo * Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100);
        //            retenciones += encabezado.retencion;
        //        }
        //        if (buscarPerfilTributario.retiva == "A" && encabezado.costo >= buscarTipoDocRegistro.baseiva)
        //        {
        //            encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
        //            encabezado.retencion_iva = encabezado.costo * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100);
        //            retenciones += encabezado.retencion_iva;
        //        }
        //        if (buscarPerfilTributario.retica == "A" && encabezado.costo >= buscarTipoDocRegistro.baseica)
        //        {
        //            var bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x => x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
        //            if (bodega_acteco != null)
        //            {
        //                encabezado.porcen_retica = Convert.ToInt32(bodega_acteco.porcentaje);
        //                encabezado.retencion_ica = encabezado.costo * Convert.ToDecimal(bodega_acteco.porcentaje / 100);
        //                retenciones += encabezado.retencion_ica;
        //            }
        //            else
        //            {
        //                encabezado.porcen_retica = Convert.ToInt32(buscarProveedor.acteco_tercero.tarifa);
        //                encabezado.retencion_ica = encabezado.costo * Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 100);
        //                retenciones += encabezado.retencion_ica;
        //            }
        //        }
        //    }

        //    decimal fletes = devolucion.fletes + devolucion.iva_fletes;

        //    encabezado.perfilcontable = perfil_seleccionado;
        //    encabezado.tipo = tipo_doc;
        //    encabezado.bodega = bodega;
        //    encabezado.fecha = DateTime.Now;
        //    encabezado.valor_total = valor_totalenca - retenciones + fletes;
        //    encabezado.iva = total_iva;
        //    encabezado.costo = valor_totalenca - total_iva;
        //    encabezado.valor_mercancia = valor_totalenca - total_iva;
        //    encabezado.fec_creacion = DateTime.Now;
        //    encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
        //    encabezado.estado = true;
        //    encabezado.numero = consecutivo;
        //    encabezado.nit = devolucion.nit;
        //    encabezado.fletes = devolucion.fletes;
        //    encabezado.iva_fletes = devolucion.iva_fletes;
        //    context.encab_documento.Add(encabezado);
        //    context.SaveChanges();

        //    var id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault().idencabezado;

        //    //movimiento contable
        //    var j = 0;
        //    var idperfil = encabezado.perfilcontable;
        //    var perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == idperfil).ToList();
        //    decimal totalDebitos = 0;
        //    decimal totalCreditos = 0;
        //    List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
        //    List<bool> cuentas_valores_nuevos = new List<bool>();
        //    var centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
        //    var idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
        //    var terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
        //    var idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

        //    foreach (var item in perfil)
        //    {
        //        j++;
        //        var cuentas = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == item.cuenta);
        //        var descripcionParametro = context.paramcontablenombres.FirstOrDefault(x => x.id == item.id_nombre_parametro).descripcion_parametro;

        //        mov_contable mov_contable = new mov_contable();
        //        mov_contable.id_encab = id_encabezado;
        //        mov_contable.seq = j;
        //        mov_contable.idparametronombre = item.id_nombre_parametro;
        //        mov_contable.cuenta = item.cuenta;
        //        mov_contable.centro = item.centro;
        //        mov_contable.fec = DateTime.Now;

        //        if (cuentas.tercero == true)
        //        {
        //            mov_contable.nit = encabezado.nit;
        //        }
        //        if (cuentas.manejabase == true)
        //        {
        //            mov_contable.basecontable = encabezado.valor_mercancia;
        //        }

        //        if (item.id_nombre_parametro == 1)
        //        {
        //            mov_contable.credito = encabezado.valor_total;
        //        }
        //        else if (item.id_nombre_parametro == 2)
        //        {
        //            mov_contable.debito = encabezado.iva;
        //            mov_contable.basecontable = encabezado.iva;
        //        }
        //        else if (item.id_nombre_parametro == 3)
        //        {
        //            mov_contable.credito = encabezado.retencion;
        //        }
        //        else if (item.id_nombre_parametro == 4)
        //        {
        //            mov_contable.credito = encabezado.retencion_iva;
        //            mov_contable.basecontable = encabezado.iva;
        //        }
        //        else if (item.id_nombre_parametro == 5)
        //        {
        //            mov_contable.credito = encabezado.retencion_ica;
        //        }
        //        else if (item.id_nombre_parametro == 6)
        //        {
        //            mov_contable.debito = encabezado.fletes != null ? encabezado.fletes : 0;
        //        }
        //        else if (item.id_nombre_parametro == 9)
        //        {
        //            mov_contable.debito = encabezado.valor_total;
        //        }
        //        else if (item.id_nombre_parametro == 14)
        //        {
        //            mov_contable.debito = encabezado.iva_fletes != null ? encabezado.iva_fletes : 0;
        //        }
        //        else
        //        {
        //            TempData["mensaje_error"] = "El documento no tiene todos los parametros contables configurados, por favor comuniquese con el administrador";

        //            //string QueryPagos = "Delete from mov_contable where id_encab =" + id_encabezado;
        //            //context.Database.ExecuteSqlCommand(QueryPagos);

        //            string QueryDocumento = "Delete from encab_documento where idencabezado =" + id_encabezado;
        //            context.Database.ExecuteSqlCommand(QueryDocumento);

        //            context.SaveChanges();
        //            return RedirectToAction("ComprasRealizadas", "CompraRepuestos", new { menu });
        //        }

        //        if (cuentas.concepniff == 1)
        //        {
        //            mov_contable.debitoniif = mov_contable.credito;
        //            mov_contable.creditoniif = mov_contable.debito;
        //        }
        //        if (cuentas.concepniff == 4)
        //        {
        //            mov_contable.debitoniif = mov_contable.credito;
        //            mov_contable.creditoniif = mov_contable.debito;
        //        }
        //        mov_contable.detalle = "Devolución compra de repuestos consecutivo # " + encabezado.numero;
        //        mov_contable.fec_creacion = DateTime.Now;
        //        mov_contable.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);


        //        var buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x => x.centro == item.centro && x.cuenta == item.cuenta && x.nit == mov_contable.nit);

        //        var fechaHoy = DateTime.Now;
        //        if (buscar_cuentas_valores != null)
        //        {
        //            buscar_cuentas_valores.ano = fechaHoy.Year;
        //            buscar_cuentas_valores.mes = fechaHoy.Month;
        //            buscar_cuentas_valores.cuenta = mov_contable.cuenta;
        //            buscar_cuentas_valores.centro = mov_contable.centro;
        //            //buscar_cuentas_valores.nit = mov_contable.nit ?? idTerceroCero;
        //            buscar_cuentas_valores.nit = mov_contable.nit;
        //            buscar_cuentas_valores.saldo_ini = (buscar_cuentas_valores.saldo_ini) + (mov_contable.debito) - (mov_contable.credito);
        //            buscar_cuentas_valores.debito += mov_contable.debito;
        //            buscar_cuentas_valores.credito += mov_contable.credito;
        //            buscar_cuentas_valores.saldo_ininiff = (buscar_cuentas_valores.saldo_ininiff) + (mov_contable.debitoniif) - (mov_contable.creditoniif);
        //            buscar_cuentas_valores.debitoniff += mov_contable.debitoniif;
        //            buscar_cuentas_valores.creditoniff += mov_contable.creditoniif;
        //            //context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
        //            ids_cuentas_valores.Add(buscar_cuentas_valores);
        //            cuentas_valores_nuevos.Add(false);
        //        }
        //        else
        //        {
        //            var crearCuentaValor = new cuentas_valores();
        //            crearCuentaValor.ano = fechaHoy.Year;
        //            crearCuentaValor.mes = fechaHoy.Month;
        //            crearCuentaValor.cuenta = mov_contable.cuenta;
        //            crearCuentaValor.centro = mov_contable.centro;
        //            //crearCuentaValor.nit = mov_contable.nit ?? idTerceroCero;
        //            crearCuentaValor.nit = mov_contable.nit;
        //            crearCuentaValor.saldo_ini = (mov_contable.debito) - (mov_contable.credito);
        //            crearCuentaValor.debito = mov_contable.debito;
        //            crearCuentaValor.credito = mov_contable.credito;
        //            crearCuentaValor.saldo_ininiff = (mov_contable.debitoniif) - (mov_contable.creditoniif);
        //            crearCuentaValor.debitoniff = mov_contable.debitoniif;
        //            crearCuentaValor.creditoniff = mov_contable.creditoniif;
        //            //context.cuentas_valores.Add(crearCuentaValor);
        //            ids_cuentas_valores.Add(crearCuentaValor);
        //            cuentas_valores_nuevos.Add(true);
        //        }


        //        context.mov_contable.Add(mov_contable);
        //        totalCreditos += mov_contable.credito;
        //        totalDebitos += mov_contable.debito;

        //    }
        //    context.SaveChanges();

        //    if (totalDebitos != totalCreditos)
        //    {
        //        TempData["mensaje_error"] = "El documento no tiene los movimientos calculados correctamente";
        //        TempData["total_debitos"] = totalDebitos;
        //        TempData["total_creditos"] = totalCreditos;

        //        string QueryPagos = "Delete from mov_contable where id_encab =" + id_encabezado;
        //        context.Database.ExecuteSqlCommand(QueryPagos);

        //        string QueryDocumento = "Delete from encab_documento where idencabezado =" + id_encabezado;
        //        context.Database.ExecuteSqlCommand(QueryDocumento);

        //        context.SaveChanges();
        //        return RedirectToAction("ComprasRealizadas", "CompraRepuestos", new { menu });
        //    }


        //    for (var inc = 0; inc < ids_cuentas_valores.Count; inc++)
        //    {
        //        if (cuentas_valores_nuevos.ElementAt(inc) == true)
        //        {
        //            context.cuentas_valores.Add(ids_cuentas_valores.ElementAt(inc));
        //        }
        //        else
        //        {
        //            context.Entry(ids_cuentas_valores.ElementAt(inc)).State = EntityState.Modified;
        //        }
        //    }
        //    context.SaveChanges();


        //    //actualizar el encabezado de la compra
        //    devolucion.documento = Convert.ToString(consecutivo);
        //    devolucion.valor_aplicado = devolucion.valor_total;
        //    devolucion.fec_actualizacion = DateTime.Now;
        //    context.Entry(devolucion).State = System.Data.Entity.EntityState.Modified;


        //    var p = 0;
        //    //var lineas_devolucion = context.lineas_documento.Where(x => x.id_encabezado == devolucion.idencabezado).ToList();
        //    foreach (var item in listaId)
        //    {
        //        p++;
        //        var id = Convert.ToInt32(item);
        //        var row = context.lineas_documento.Find(id);

        //        lineas_documento linea = new lineas_documento();
        //        linea.id_encabezado = id_encabezado;
        //        linea.codigo = row.codigo;
        //        linea.seq = p;
        //        linea.fec = DateTime.Now;
        //        linea.nit = encabezado.nit;
        //        linea.cantidad = Convert.ToDecimal(Request["cant_real_" + item]);
        //        linea.cantidad_pedida = row.cantidad_pedida;
        //        linea.valor_unitario = row.valor_unitario;
        //        linea.costo_unitario = row.costo_unitario;
        //        linea.bodega = bodega;
        //        linea.fec_creacion = DateTime.Now;
        //        linea.estado = true;
        //        linea.pedido = Convert.ToInt32(encabezado.pedido);
        //        linea.porcentaje_iva = row.porcentaje_iva;
        //        linea.porcentaje_descuento = row.porcentaje_descuento;
        //        context.lineas_documento.Add(linea);

        //        //referencias_inventario
        //        var codigo = row.codigo;
        //        var referencia = context.referencias_inven.FirstOrDefault(x => x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month && x.bodega == bodega);
        //        if (referencia != null)
        //        {
        //            referencia.can_dev_com = referencia.can_dev_com + linea.cantidad;
        //            referencia.cos_dev_com = referencia.cos_dev_com + row.costo_unitario;
        //            referencia.can_sal += linea.cantidad;
        //            referencia.cos_sal = referencia.cos_sal + row.costo_unitario;
        //            context.Entry(referencia).State = System.Data.Entity.EntityState.Modified;
        //        }
        //        else
        //        {
        //            referencias_inven refe = new referencias_inven();
        //            refe.codigo = codigo;
        //            refe.ano = Convert.ToInt16(DateTime.Now.Year);
        //            refe.mes = Convert.ToInt16(DateTime.Now.Month);
        //            refe.can_dev_com = linea.cantidad;
        //            refe.cos_dev_com = row.costo_unitario;
        //            refe.can_sal = linea.cantidad;
        //            refe.cos_sal = row.costo_unitario;
        //            refe.bodega = bodega;
        //            refe.modulo = "R";
        //            context.referencias_inven.Add(refe);
        //        }
        //    }

        //    try
        //    {
        //        context.SaveChanges();
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
        //    {
        //        Exception raise = dbEx;
        //        foreach (var validationErrors in dbEx.EntityValidationErrors)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {
        //                string message = string.Format("{0}:{1}",
        //                    validationErrors.Entry.Entity.ToString(),
        //                    validationError.ErrorMessage);
        //                // raise a new exception nesting
        //                // the current instance as InnerException
        //                raise = new InvalidOperationException(message, raise);
        //                TempData["mensaje_error"] = raise;
        //            }
        //        }
        //        BuscarFavoritos(menu);
        //        return View(encabezado);
        //    }

        //    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
        //    doc.ActualizarConsecutivo(grupo, consecutivo);

        //    // eliminar los datos que quedaron en 0
        //    string QueryMovContable = "Delete mov_contable where (debito = 0 or credito = 0) and id_encab =" + id_encabezado;
        //    context.Database.ExecuteSqlCommand(QueryMovContable);

        //    context.SaveChanges();

        //    TempData["mensaje"] = "Devolución de compra realizada correctamente";

        //    return RedirectToAction("DetalleDevoluciones", new { id = id_encabezado, menu = @ViewBag.id_menu });
        //}

        //public ActionResult BrowserDevoluciones(int ? menu)
        //{
        //    BuscarFavoritos(menu);
        //    return View();
        //}

        #endregion

        public class Agrupado
        {
            public string coderef { get; set; }
            public string cantiTotal { get; set; }
            //public string numpedido_gm { get; set; }
            public string notas { get; set; }
            public string solicitudes { get; set; }
        }
    }
}