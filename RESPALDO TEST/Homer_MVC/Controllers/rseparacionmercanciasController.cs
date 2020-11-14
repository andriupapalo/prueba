using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Newtonsoft.Json;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class rseparacionmercanciasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        //private int seperacion_consecutivo = 0;
        // GET: rseparacionmercancias
        public ActionResult Browser(int? menu)
        {
            IQueryable<rseparacionmercancia> rseparacionmercancia = db.rseparacionmercancia.Include(r => r.bodega_concesionario)
                .Include(r => r.icb_referencia).Include(r => r.icb_terceros);
            BuscarFavoritos(menu);
            return View(rseparacionmercancia.ToList());
        }

        public void listas()
        {
            var list = (from t in db.icb_terceros
                        join tp in db.tercero_cliente
                            on t.tercero_id equals tp.tercero_id
                        where t.tercero_estado
                        select new
                        {
                            t.tercero_id,
                            nombre = t.prinom_tercero + t.razon_social,
                            t.doc_tercero,
                            t.segnom_tercero,
                            t.apellido_tercero,
                            t.segapellido_tercero
                        }).OrderBy(x => x.nombre).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = '(' + item.doc_tercero + ") - " + item.nombre + ' ' + item.segnom_tercero + ' ' +
                           item.apellido_tercero + ' ' + item.segapellido_tercero,
                    Value = item.tercero_id.ToString()
                });
            }

            var listC = (from t in db.icb_referencia
                         select new
                         {
                             t.ref_codigo,
                             t.ref_descripcion
                         }).ToList();

            List<SelectListItem> listaC = new List<SelectListItem>();
            foreach (var item in listC)
            {
                listaC.Add(new SelectListItem
                {
                    Text = item.ref_codigo + ' ' + item.ref_descripcion,
                    Value = item.ref_codigo
                });
            }

            object nombreb = Session["user_bodegaNombre"];
            object bodegab = Session["user_bodega"];
            //ViewBag.bodegan = nombreb;
            ViewBag.bodega = bodegab;
            ViewBag.cliente = lista;
            ViewBag.codigo = listaC;

            var id1 = Convert.ToInt32(bodegab);
            //ViewBag.bodegan = new SelectList(db.bodega_concesionario.Where(x => x.id == Convert.ToInt32(bodegab)));

            var listaB = (from b in db.bodega_concesionario
                          where b.id == id1
                          select new
                         {
                             id = b.id,
                             nombre = b.bodccs_nombre
                         }).ToList();

            List<SelectListItem> list1 = new List<SelectListItem>();
            foreach (var item in listaB)
            {
                list1.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodegan = list1;

        }

        public JsonResult BuscarPedidosPorCliente(int? idcliente)
        {
            List<icb_referencia_mov> pedidos = db.icb_referencia_mov.Where(x => x.cliente == idcliente && x.estado).ToList();
            var data = pedidos.Select(x => new
            {
                id = x.refmov_id,
                numero = x.refmov_numero,
                fecha = x.refmov_fecela.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult permisoDescomprometer()
        {
            int rolid = Convert.ToInt32(Session["user_rolid"]);
            rolpermisos permiso = db.rolpermisos.Where(x => x.codigo == "P32").FirstOrDefault();
            if (permiso != null)
            {
                rolacceso acceso = db.rolacceso.Where(x => x.idpermiso == permiso.id && x.idrol == rolid).FirstOrDefault();
                if (acceso != null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public class Anticipo
        {
            public int id { get; set; }
            public string valor { get; set; }
        }

        // GET: rseparacionmercancias/Create
        public ActionResult Create(int? menu)
        {
            double diasValidez =
                Convert.ToDouble(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P75").syspar_value);
            ViewBag.fechaFinal = DateTime.Now.AddDays(diasValidez).ToString("yyyy/MM/dd");
            //ViewBag.color = new SelectList(db.color_vehiculo.Where(x => x.colvh_estado != false).OrderBy(x => x.colvh_nombre), "colvh_id", "colvh_nombre");
            listas();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: rseparacionmercancias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(rseparacionmercancia entrada, int? menu)
        {

            int codseparacion = 0;
            int nuevocodigo = 0;
            bool result = false;
            double diasValidez =
                Convert.ToDouble(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P75").syspar_value);
            int idbodega = Convert.ToInt32(Session["user_bodega"]);
            int cantidadLineas = Convert.ToInt32(Request["listaReferencias"]);
            string codigoseparacion = (from sm in db.rseparacionmercancia
                                       orderby sm.fec_creacion descending
                                       select sm.separacion).FirstOrDefault().ToString();
            string Ant = Request["idAnt"];

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < cantidadLineas; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["codigoTabla" + i]))
                        {
                            rseparacionmercancia rseparacion = new rseparacionmercancia();
                            if (!string.IsNullOrEmpty(codigoseparacion))
                            {
                                bool separacion = int.TryParse(codigoseparacion, out codseparacion);
                                rseparacion.separacion = codseparacion + 1;
                                nuevocodigo = codseparacion + 1;
                            }
                            else
                            {
                                codseparacion = 1;
                                nuevocodigo = 1;
                                rseparacion.separacion = codseparacion;
                            }
                            rseparacion.cliente = Convert.ToInt32(Request["clienteOculto"]);
                            rseparacion.codigo = Request["codigoTabla" + i];
                            rseparacion.cantidad = Convert.ToInt32(Request["cantidadTabla" + i]);
                            rseparacion.placa = Request["placa" + i];
                            rseparacion.fecha = DateTime.Now;
                            rseparacion.estado = true;
                            rseparacion.fec_creacion = DateTime.Now;
                            rseparacion.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            rseparacion.fechaFinal = DateTime.Now.AddDays(diasValidez);
                            rseparacion.bodega = idbodega;
                            rseparacion.idpedido = entrada.idpedido;
                            var traslado = Convert.ToInt32(Request["stockreferencia" + i]);
                            if (traslado == 1)
                            {
                                rseparacion.traslado = true;
                                rseparacion.solicitud = false;
                                rseparacion.bodega_traslado = Convert.ToInt32(Request["listabodega" + i]);
                            }
                            else if (traslado == 0)
                            {
                                rseparacion.traslado = false;
                                rseparacion.solicitud = true;
                            }
                            if (entrada.idpedido != null)
                            {
                                var refer = Request["codigoTabla" + i].ToString();
                                var cantidad = Convert.ToInt32(Request["cantidadTabla" + i]);
                                //busco en el pedido la referencia y la marco como solicitada.
                                var pedi = db.icb_referencia_movdetalle.Where(d => d.refmov_id == entrada.idpedido && d.ref_codigo == refer && d.refdet_cantidad == cantidad).FirstOrDefault();
                                if (pedi != null)
                                {
                                    pedi.solicitado = true;
                                    db.Entry(pedi).State = EntityState.Modified;
                                }
                            }
                            db.rseparacionmercancia.Add(rseparacion);
                            db.SaveChanges();
                            agregarAnticipos(rseparacion, Ant);

                            var icb_referencia_mov = db.icb_referencia_mov.Find(entrada.idpedido);
                            icb_referencia_mov.estado = true;

                            db.Entry(icb_referencia_mov).State = EntityState.Modified;
                            result = db.SaveChanges() > 0;

                        }
                    }

                    //veo si hay items sin stock
                    var solicitudex = db.rseparacionmercancia.Where(d => d.separacion == nuevocodigo && d.solicitud == true).Count();
                    var trasladox = db.rseparacionmercancia.Where(d => d.separacion == nuevocodigo && d.traslado == true).Count();
                    if (solicitudex > 0)
                    {
                        solicitarRepuestos(nuevocodigo);
                    }
                    //veo si hay items para trasladar
                    if (trasladox > 0)
                    {
                        solicitarTraslado(nuevocodigo);
                    }
                    dbTran.Commit();
                    TempData["mensaje"] = "Registro creado correctamente";
                    ViewBag.fechaFinal = DateTime.Now.AddDays(diasValidez).ToString("yyyy/MM/dd");

                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }






            listas();
            BuscarFavoritos(menu);
            return View();

        }

        public int solicitarRepuestos(int codigo)
        {
            rsolicitudesrepuestos rsolicitudrepu = new rsolicitudesrepuestos();
            rsolicitudrepu.fecha = DateTime.Now;
            int uno = 1;
            icb_sysparameter sysparameter = db.icb_sysparameter.Where(x => x.syspar_cod == "P143").FirstOrDefault();
            //busco la separacion
            var sepa = db.rseparacionmercancia.Where(d => d.separacion == codigo && d.solicitud == true && (d.cantidad_recibida == null || d.cantidad_recibida < d.cantidad)).ToList();
            rsolicitudrepu.bodega = sepa.Select(d=>d.bodega).First();
            rsolicitudrepu.cliente = sepa.Select(d=>d.cliente).First();
            rsolicitudrepu.usuario = Convert.ToInt32(Session["user_usuarioid"]);
            rsolicitudrepu.estado_solicitud = uno;
            //DE MOMENTO ESTE TIPO DE COMPRA QUEDA QUEMADO PORQUE no sabemos cual es
            rsolicitudrepu.tipo_compra = 2;
            //fiN DEL QUEMADO
            rsolicitudrepu.tiposolicitud = Convert.ToInt32(sysparameter.syspar_value);
            rsolicitudrepu.separacion_consecutivo = codigo;
            rsolicitudrepu.Detalle = "Separacion de repuestos consecutivo No " + codigo;
            db.rsolicitudesrepuestos.Add(rsolicitudrepu);
            db.SaveChanges();
            foreach (var item in sepa)
            {
                rdetallesolicitud rdetallesol = new rdetallesolicitud();
                rdetallesol.id_solicitud = rsolicitudrepu.id;
                rdetallesol.referencia =item.codigo;
                rdetallesol.cantidad = item.cantidad;
                rdetallesol.valor = 0;
                rdetallesol.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                rdetallesol.fecha_creacion = DateTime.Now;
                rdetallesol.IDRSEPARACION = item.id;
                rdetallesol.esta_pedido = 1;
                db.rdetallesolicitud.Add(rdetallesol);
            }
            db.SaveChanges();
            return 1;
        }

        public int solicitarTraslado(int codigo)
        {
            //busco los items en traslado y los agrupo por bdegas
            var items = db.rseparacionmercancia.Where(d => d.separacion == codigo && d.traslado == true).ToList();

            var bodegas = items.GroupBy(d => d.bodega_traslado).Select(d => d.Key).ToList();

            foreach (var item in bodegas)
            {
                //busco los items paa trasladar desde esa bodega
                var bodegaorigen = item;
                var items2 = items.Where(d => d.bodega_traslado == bodegaorigen).ToList();
                //elaboro la nueva solicitud de traslado
                var solicitudtra = new Solicitud_traslado
                {
                    Id_bodega_origen= bodegaorigen.Value,
                    Id_bodega_destino=items2.Select(d=>d.bodega).FirstOrDefault(),
                    Id_solicitante= items2.Select(d => d.userid_creacion).First(),
                    Fecha_creacion = DateTime.Now,
                    Estado_atendido=1,
                };
                db.Solicitud_traslado.Add(solicitudtra);
                db.SaveChanges();

                //guardo los detalles del traslado
                foreach (var item3 in items2)
                {
                    var detallesoli2 = new tempSolicitud
                    {
                        dataBogeda = item3.bodega_traslado,
                        dataBogeda2 = item3.bodega,
                        Stock = item3.cantidad,
                        idreferencia = item3.codigo,
                        idseparacion = item3.id,
                        dataCliente = item3.cliente,
                    };
                    db.tempSolicitud.Add(detallesoli2);

                    var detallesoli = new Detalle_Solicitud_Traslado
                    {
                        Cantidad=item3.cantidad,
                        Cod_referencia=item3.codigo,
                        Id_Solicitud_Traslado=solicitudtra.Id,
                        idseparacion=item3.id,                       
                    };
                    db.Detalle_Solicitud_Traslado.Add(detallesoli);
                }
                db.SaveChanges();

            }
            return 1;
        }


        public JsonResult validarAnticipo(string anticipos, string valorTotal)
        {
            string parametro = db.icb_sysparameter.Where(x => x.syspar_cod == "P137").FirstOrDefault().syspar_value;
            int valor = parametro != null ? Convert.ToInt32(parametro) : 50;
            int sumAnticipos = 0;
            int porcentajeVT = (Convert.ToInt32(valorTotal) * valor) / 100;
            Anticipo[] listado = JsonConvert.DeserializeObject<Anticipo[]>(anticipos);
            for (int i = 0; i < listado.Length; i++)
            {
                sumAnticipos += Convert.ToInt32(listado[i].valor);
            }
            if (sumAnticipos >= porcentajeVT)
            {
                return Json(new { cumple = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { cumple = false, porcentaje = valor }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult agregarAnticipos(rseparacionmercancia model, string Ant)
        {
            string[] id = Ant.Split(new string[] { "|" }, StringSplitOptions.None);


            for (int i = 0; i < id.Length; i++)
            {
                string[] datocadena = id[i].Split(new string[] { "," }, StringSplitOptions.None);


                rseparacion_anticipo rsep = new rseparacion_anticipo
                {
                    separacion_id = model.id,
                    anticipo_id = Convert.ToInt32(datocadena[0]),
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    Valor = Convert.ToInt32(datocadena[1])
                };
                db.rseparacion_anticipo.Add(rsep);
                db.SaveChanges();
                int idencab = Convert.ToInt32(datocadena[0]);

                var Anticipo = (from anticipo in db.encab_documento
                                where anticipo.idencabezado == idencab
                                select anticipo.valor_aplicado
                                ).FirstOrDefault();


                int total = Convert.ToInt32(datocadena[1]) + Convert.ToInt32(Anticipo);

                encab_documento UpdtAnticipo = db.encab_documento.FirstOrDefault(x => x.idencabezado == idencab);


                if (UpdtAnticipo!= null)
                {
                    UpdtAnticipo.valor_aplicado = total;
                    db.Entry(UpdtAnticipo).State = EntityState.Modified;
                    db.SaveChanges();
                }
                                  


             }
            return Json(0);
        }

        // GET: rseparacionmercancias/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            rseparacionmercancia rseparacionmercancia = db.rseparacionmercancia.Find(id);
            if (rseparacionmercancia == null)
            {
                return HttpNotFound();
            }
            //ViewBag.color = new SelectList(db.color_vehiculo.Where(x => x.colvh_estado != false).OrderBy(x => x.colvh_nombre), "colvh_id", "colvh_nombre", rseparacionmercancia.color);
            //ViewBag.bodega = new SelectList(db.bodega_concesionario, "id", "bodccs_nombre", rseparacionmercancia.bodega);
            var list = (from t in db.icb_terceros
                        join tp in db.tercero_cliente
                            on t.tercero_id equals tp.tercero_id
                        where t.tercero_estado
                        select new
                        {
                            t.tercero_id,
                            nombre = t.prinom_tercero + t.razon_social,
                            t.doc_tercero,
                            t.segnom_tercero,
                            t.apellido_tercero,
                            t.segapellido_tercero
                        }).OrderBy(x => x.nombre).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = '(' + item.doc_tercero + ") - " + item.nombre + ' ' + item.segnom_tercero + ' ' +
                           item.apellido_tercero + ' ' + item.segapellido_tercero,
                    Value = item.tercero_id.ToString()
                });
            }

            var listC = (from t in db.icb_referencia
                         select new
                         {
                             t.ref_codigo,
                             t.ref_descripcion
                         }).ToList();

            List<SelectListItem> listaC = new List<SelectListItem>();

            foreach (var item in listC)
            {
                listaC.Add(new SelectListItem
                {
                    Text = item.ref_codigo + ' ' + item.ref_descripcion,
                    Value = item.ref_codigo
                });
            }

            object nombreb = Session["user_bodegaNombre"];
            object bodegab = Session["user_bodega"];
            ViewBag.bodegan = nombreb;
            ViewBag.bodega = bodegab;
            ViewBag.cliente = new SelectList(lista, "Value", "text", rseparacionmercancia.cliente);
            ViewBag.codigo = new SelectList(listaC, "Value", "text", rseparacionmercancia.codigo);
            ConsultaDatosCreacion(rseparacionmercancia);
            //listas();
            BuscarFavoritos(menu);
            return View(rseparacionmercancia);
        }

        // POST: rseparacionmercancias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(rseparacionmercancia rseparacionmercancia, int? menu)
        {
            double diasValidez =
                Convert.ToDouble(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P75").syspar_value);
            ConsultaDatosCreacion(rseparacionmercancia);
            if (ModelState.IsValid)
            {
                int contar = db.rseparacionmercancia.Where(x =>
                        x.estado && x.codigo == rseparacionmercancia.codigo && rseparacionmercancia.id != x.id)
                    .Sum(x => x.cantidad);
                decimal stock = db.vw_inventario_hoy.FirstOrDefault(x =>
                    x.bodega == rseparacionmercancia.bodega && x.ref_codigo == rseparacionmercancia.codigo).stock;
                decimal disponible = stock - contar;
                decimal resultado = disponible - rseparacionmercancia.cantidad;

                if (resultado < 1)
                {
                    ViewBag.fechaFinal = DateTime.Now.AddDays(diasValidez).ToString("yyyy/MM/dd");
                    TempData["mensaje_error"] = "No hay Stock disponible para la separación del repuesto. Disponible(" +
                                                Convert.ToInt32(disponible) + ")";
                    listas();
                    BuscarFavoritos(menu);
                    return View(rseparacionmercancia);
                }

                rseparacionmercancia.fecha = DateTime.Now;
                rseparacionmercancia.fec_actualizacion = DateTime.Now;
                rseparacionmercancia.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(rseparacionmercancia).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado correctamente";
            }

            //ViewBag.color = new SelectList(db.color_vehiculo.Where(x => x.colvh_estado != false).OrderBy(x => x.colvh_nombre), "colvh_id", "colvh_nombre", rseparacionmercancia.color);
            listas();
            BuscarFavoritos(menu);
            return View(rseparacionmercancia);
        }

        public void ConsultaDatosCreacion(rseparacionmercancia rseparacionmercancia)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(rseparacionmercancia.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(rseparacionmercancia.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public JsonResult buscarVhClientes(int id)
        {
            var vehiculos = (from ped in db.vpedido
                             join vh in db.icb_vehiculo
                             on ped.planmayor equals vh.plan_mayor
                             where ped.nit == id && vh.plac_vh != null
                             select new
                             {
                                 planmayor = ped.planmayor,
                                 modelo = vh.modelo_vehiculo.modvh_nombre,
                                 placa = vh.plac_vh
                             }).ToList();

            return Json(vehiculos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult stockDisponible(string codigo, int cantidad)
        {
            int bodegab = Convert.ToInt32(Session["user_bodega"]);
            decimal data = (from i in db.vw_inventario_hoy
                            where i.bodega == bodegab && i.ref_codigo == codigo
                            select i.stock).FirstOrDefault();

            if (data >= cantidad)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult inactivarSeparacion()
        {
            DateTime hoy = DateTime.Now;
            List<int> buscar = db.rseparacionmercancia.Where(x => hoy > x.fechaFinal && x.estado).Select(x => x.id).ToList();
            foreach (int item in buscar)
            {
                rseparacionmercancia select = db.rseparacionmercancia.Find(item);
                select.estado = false;
                select.razon_inactivo = "Fecha Vencida";
                db.Entry(select).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAnticipos(int cliente)
        {
            var info = (from e in db.encab_documento
                        join d in db.tp_doc_registros
                            on e.tipo equals d.tpdoc_id
                        where e.nit == cliente && e.anticipo == true && (e.valor_total - e.valor_aplicado) > 0
                        select new
                        {
                            e.idencabezado,
                            documento = "(" + d.prefijo + ") " + d.tpdoc_nombre,
                            e.numero,
                            e.fecha,
                            e.valor_total,
                            e.nota1,
                            e.valor_aplicado,
                            total = e.valor_total - e.valor_aplicado
                        }).ToList();

            var data = info.Select(x => new
            {
                x.idencabezado,
                x.documento,
                x.numero,
                fecha = x.fecha.ToShortDateString(),
                valor_total =
                    db.documentosacruzar.OrderByDescending(d => d.id)
                        .FirstOrDefault(d => d.idencabrecibo == x.idencabezado) != null
                        ? db.documentosacruzar.OrderByDescending(d => d.id)
                            .FirstOrDefault(d => d.idencabrecibo == x.idencabezado).saldo
                        : x.valor_total,
                x.valor_aplicado,
                x.total,
                x.nota1
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BusquedaBrowser()
        {
            var buscar = (from a in db.rseparacionmercancia
                          join b in db.icb_referencia
                              on a.codigo equals b.ref_codigo
                          join c in db.bodega_concesionario
                              on a.bodega equals c.id
                          join d in db.icb_terceros
                              on a.cliente equals d.tercero_id
                          join e in db.icb_referencia_mov
                            on a.idpedido equals e.refmov_id into f
                          from e in f.DefaultIfEmpty()
                          select new
                          {
                              b.ref_codigo,
                              b.ref_descripcion,
                              d.prinom_tercero,
                              d.segnom_tercero,
                              d.apellido_tercero,
                              d.segapellido_tercero,
                              c.bodccs_nombre,
                              a.estado,
                              a.separacion,
                              a.fecha,
                              a.fechaFinal,
                              a.cantidad,
                              a.id,
                              d.doc_tercero,
                              numeroPed = a.idpedido != null ? e.refmov_numero : 0,
                              total = a.cantidad * b.precio_venta
                          }).ToList();

            var data2 = buscar.GroupBy(d => d.separacion).Select(d => new
            {
                idSeparacion = d.Key,
                id = d.Select(e => e.id).FirstOrDefault(),
                estado = d.Select(e => e.estado ? "Activo" : "Inactivo").FirstOrDefault(),
                doc_tercero = d.Select(e => e.doc_tercero).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.prinom_tercero).FirstOrDefault() + " " +
                          d.Select(e => e.segnom_tercero).FirstOrDefault() + " " +
                          d.Select(e => e.apellido_tercero).FirstOrDefault() + " " +
                          d.Select(e => e.segapellido_tercero).FirstOrDefault(),
                fecha = d.Select(e => e.fecha).FirstOrDefault().ToString("yyyy/MM/dd"),
                fechaFinal = d.Select(e => e.fechaFinal).FirstOrDefault() != null
                    ? d.Select(e => e.fechaFinal).FirstOrDefault().Value.ToString("yyyy/MM/dd")
                    : "",
                numPedido = d.Select(e => e.numeroPed != 0 ? e.numeroPed.ToString() : "").FirstOrDefault(),
                total = (d.Sum(e => e.total)).ToString("N2", new CultureInfo("is-IS")),
                listadoreferencias = d.Select(e => new
                {
                    codigoreferencia = e.ref_codigo,
                    descripcion = e.ref_descripcion,
                    e.cantidad
                }).ToList()
            }).ToList();

            //var data = buscar.Select(x => new {
            //    codigo = x.ref_codigo + " " + x.ref_descripcion,
            //    cliente = x.prinom_tercero + " " + x.segnom_tercero +" " + x.apellido_tercero +" "+ x.segapellido_tercero,
            //    fecha = x.fecha.ToString("yyyy/MM/dd"),
            //    fechaFinal = x.fechaFinal.Value.ToString("yyyy/MM/dd"),
            //    x.cantidad,
            //    bodega = x.bodccs_nombre,
            //    estado = x.estado != false ? "Activo" : "Inactivo",
            //    documento = x.doc_tercero,
            //    x.id,
            //    x.separacion
            //}).OrderByDescending(x => x.id).Distinct();

            //var listaSeparacion = buscar.Select(x => new
            //{
            //    separacionCod = x.separacion
            //}).Distinct();


            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult descomprometer(int id, string cantidad)
        {
            rseparacionmercancia separacion = db.rseparacionmercancia.Where(x => x.id == id).FirstOrDefault();
            if (separacion.cantidad == Convert.ToInt32(cantidad))
            {
                db.Entry(separacion).State = EntityState.Deleted;
            }
            else
            {
                separacion.comprometido = false;
                separacion.cantidad = Convert.ToInt32(cantidad);
                separacion.fec_actualizacion = DateTime.Now;
                separacion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(separacion).State = EntityState.Modified;
            }

            int result = db.SaveChanges();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult detallesRef(int id)
        {

            int userid = Convert.ToInt32(Session["user_usuarioid"]);
            var buscar = (from a in db.rseparacionmercancia
                          join b in db.icb_referencia
                              on a.codigo equals b.ref_codigo
                          select new
                          {
                              b.ref_codigo,
                              b.ref_descripcion,
                              a.separacion,
                              a.cantidad,
                              a.id,
                              a.estado,
                              a.userid_creacion,
                              b.ref_valor_unitario
                          }).ToList();

            var data = buscar.GroupBy(d => d.separacion).Select(d => new
            {
                idSeparacion = d.Key,
                id = d.Select(e => e.id).FirstOrDefault(),
                listadoreferencias = d.Select(e => new
                {
                    e.id,
                    codigoreferencia = e.ref_codigo,
                    descripcion = e.ref_descripcion,
                    e.cantidad,
                    e.estado,
                    usuario = e.userid_creacion,
                    vr_unitario = e.ref_valor_unitario,
                    vr_total = e.ref_valor_unitario * e.cantidad
                }).ToList()
            }).Where(x => x.idSeparacion == id);

            var info = (from e in db.encab_documento
                        join d in db.tp_doc_registros
                            on e.tipo equals d.tpdoc_id
                        join sep in db.rseparacion_anticipo
                        on e.idencabezado equals sep.anticipo_id
                        join separar in db.rseparacionmercancia
                        on sep.separacion_id equals separar.id
                        where separar.separacion == id
                        select new
                        {
                            e.idencabezado,
                            documento = "(" + d.prefijo + ") " + d.tpdoc_nombre,
                            e.numero,
                            e.fecha,
                            e.valor_total,
                            e.nota1,
                            separar.separacion
                        }).ToList();

            var data3 = info.Select(x => new
            {
                x.idencabezado,
                x.documento,
                x.numero,
                fecha = x.fecha.ToShortDateString(),
                valor_total =
                    db.documentosacruzar.OrderByDescending(d => d.id)
                        .FirstOrDefault(d => d.idencabrecibo == x.idencabezado) != null
                        ? db.documentosacruzar.OrderByDescending(d => d.id)
                            .FirstOrDefault(d => d.idencabrecibo == x.idencabezado).saldo
                        : x.valor_total,
                x.nota1,
                x.separacion
            }).ToList();


            var data2 = data3.GroupBy(d => d.idencabezado).Select(x => new
            {
                idencabezado = x.Select(e => e.idencabezado).FirstOrDefault(),
                documento = x.Select(e => e.documento).FirstOrDefault(),
                numero = x.Select(e => e.numero).FirstOrDefault(),
                fecha = x.Select(e => e.fecha).FirstOrDefault(),
                valor_total = x.Select(e => e.valor_total).FirstOrDefault(),
                nota1 = x.Select(e => e.nota1).FirstOrDefault()
            }).ToList();

            List<seguimientoseparacion> seguimiento = db.seguimientoseparacion.Where(x => x.separacion == id).ToList();

            var seguimientos = seguimiento.Select(x => new
            {
                x.id,
                x.nota,
                fecha = x.fecha.Value.ToString(@"MM\/dd\/yyyy HH:mm", new CultureInfo("en-US")),
                usuario = x.users.user_nombre + " " + x.users.user_apellido,
            });


            List<ListaDocumentos> listadoc = new List<ListaDocumentos>();
            var soltrans = (from a in db.Solicitud_traslado
                        join d in db.Estado
                        on a.Estado_atendido equals d.id
                        join b in db.Detalle_Solicitud_Traslado
                        on a.Id equals b.Id_Solicitud_Traslado
                        join c in db.rseparacionmercancia
                        on b.idseparacion equals c.id into cseparacion
                        from c in cseparacion.DefaultIfEmpty()
                        where c.separacion == id
                        select new
                            {
                            tipodoc = "Solicitud de Translado",
                            a.Id,
                            fecha = a.Fecha_creacion,
                            d.Tipo

                            }).ToList();

            foreach (var item in soltrans)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = item.Id;
                lista.tipodoc = item.tipodoc;
                lista.fecha = item.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Tipo;
                listadoc.Add(lista);
                }

            var datoscom = (from a in db.rsolicitudesrepuestos
                            join b in db.rseparacionmercancia
                            on a.separacion_consecutivo equals b.separacion
                            join c in db.restado_solicitud_Repuestos
                            on a.estado_solicitud equals c.id_estado_solicitud
                            where b.separacion == id
                            select new
                                {
                                tipodoc = "Solicitud de Compra",
                                a.id,
                                fecha = a.fecha,
                                c.Descripcion
                                }).ToList();

            foreach (var item in datoscom)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = item.id;
                lista.tipodoc = item.tipodoc;
                lista.fecha = item.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Descripcion;
                listadoc.Add(lista);
                }

            int swfactura = Convert.ToInt32(db.icb_sysparameter.Where(z => z.syspar_cod == "P129").Select(x => x.syspar_value).FirstOrDefault());
            var facturas = (from a in db.encab_documento
                            join b in db.tp_doc_registros
                            on a.tipo equals b.tpdoc_id
                            join c in db.tp_doc_sw
                            on b.sw equals c.tpdoc_id
                            join d in db.icb_referencia_mov
                            on a.pedido equals d.refmov_id 
                            join e in db.rseparacionmercancia
                            on d.refmov_id equals e.idpedido                       
                            where c.sw == swfactura && e.separacion == id
                            select new
                                {
                                id = a.numero,
                                documento = b.tpdoc_nombre,
                                fecha = a.fec_creacion,
                                estado = ""

                                }).ToList();

            var facturarpe = facturas.GroupBy(x => new { x.id, x.documento, x.fecha, x.estado });

            foreach (var item in facturarpe)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = Convert.ToInt32(item.Key.id);
                lista.tipodoc = item.Key.documento;
                lista.fecha = item.Key.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Key.estado;
                listadoc.Add(lista);
                }
            var facturasorden = (from aw in db.encab_documento
                            join b in db.tp_doc_registros
                            on aw.tipo equals b.tpdoc_id
                            join c in db.tp_doc_sw
                            on b.sw equals c.tpdoc_id
                           join d in db.rseparacionmercancia
                            on aw.orden_taller equals d.idordentaller
                            where d.separacion == id &&  c.sw == swfactura
                            && aw.orden_taller != null
                                 select new
                                {
                                id = aw.numero,
                                documento = b.tpdoc_nombre,
                                fecha = aw.fec_creacion,
                                estado = "",
                           

                                }).ToList();

            var facturarorden = facturasorden.GroupBy(x => new { x.id, x.documento, x.fecha, x.estado });

            foreach (var item in facturarorden)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = Convert.ToInt32(item.Key.id);
                lista.tipodoc = item.Key.documento;
                lista.fecha = item.Key.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Key.estado;
                listadoc.Add(lista);
                }




            return Json(new { data, userid, data2, seguimientos, listadoc }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult agregarSeguimiento(int separacion, string nota)
        {
            seguimientoseparacion seguimiento = new seguimientoseparacion
            {
                separacion = separacion,
                nota = nota,
                fecha = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };
            db.seguimientoseparacion.Add(seguimiento);
            int resultado = db.SaveChanges();
            if (resultado > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult buscarDatos(string codigo, int cliente)
        {
            IQueryable<icb_terceros> clienteNombre = db.icb_terceros.Where(x => x.tercero_id == cliente);

            var clienteInfo = clienteNombre.Select(x => new
            {
                cliente = x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero + " " +
                          x.segapellido_tercero
            }).ToArray();
            IQueryable<icb_referencia> codigoPrecio = db.icb_referencia.Where(x => x.ref_codigo == codigo);
            var codigoInfo = codigoPrecio.Select(x => new
            {
                codigodesc = x.ref_descripcion,
                codigoPrecio = x.precio_venta
            }).ToArray();

            return Json(new { Codigo = codigoInfo, Cliente = clienteInfo }, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetDatosDelPedido(int? id)
        {
            var linkqData = (from r in db.icb_referencia_movdetalle
                        join mov in db.icb_referencia_mov
                            on r.refmov_id equals mov.refmov_id
                        where r.refmov_id == id && r.tiene_stock==false && r.facturado==false && r.solicitado==false
                        select new
                        {
                            codigo = r.ref_codigo,
                            r.icb_referencia.ref_descripcion,
                            cantidad = r.refdet_cantidad,
                            r.valor_unitario,
                            descuento = r.pordscto,
                            iva = r.poriva,
                            r.valor_total,
                            nit = mov.cliente,
                            mov.vendedor,
                            r.pedido,
                            r.traslado,
                            r.tiene_stock,
                            bodega=r.icb_referencia_mov.bodega_id,
                            nombrebodega=r.icb_referencia_mov.bodega_concesionario.bodccs_nombre,
                        }).ToList();

            AlmacenController metodosstock = new AlmacenController();

            var listavacia = new List<AlmacenController.listabodegas>();
            var data = linkqData.Select(x => new
            {
                x.codigo,
                referencia = x.codigo + " - " + x.ref_descripcion,
                x.cantidad,
                x.valor_unitario,
                x.descuento,
                x.iva,
                x.valor_total,
                x.nit,
                x.vendedor,
                pedido=x.pedido==true?1:0,
                traslado=x.traslado==true?1:0,
                nombrebodega=x.nombrebodega,
                idbodega=x.bodega,
                stocktraslado = x.traslado == true ? metodosstock.buscarStockOtrasCantidad(x.codigo, x.bodega,Convert.ToInt32(x.cantidad)) : listavacia,
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
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


        public ActionResult Descargarrseparacion(int id) {

            var rseparacion = db.rseparacionmercancia.Where(x => x.separacion == id).FirstOrDefault();
            int facturado = Convert.ToInt32(Session["user_usuarioid"]);
          var empresa = db.tablaempresa.FirstOrDefault();
            var ciudad = db.nom_ciudad.Where(x => x.ciu_id == rseparacion.icb_terceros.ciu_id).Select(x => x.ciu_nombre).FirstOrDefault();

            string direccion = db.terceros_direcciones.Where(x => x.idtercero == rseparacion.cliente && x.direccion.Trim() != "").Select(z => z.direccion).FirstOrDefault();
            string nombrecliente = rseparacion.icb_terceros.prinom_tercero != null ? rseparacion.icb_terceros.prinom_tercero + " " + rseparacion.icb_terceros.segnom_tercero +
            " " + rseparacion.icb_terceros.apellido_tercero + " " + rseparacion.icb_terceros.segapellido_tercero : rseparacion.icb_terceros.razon_social;



            PdfRseparacion Rsepracion = new PdfRseparacion
                {

                Aseguradoa = empresa.nombre_empresa,
                Direccionempresa = empresa.direccion,
                Cliente = nombrecliente,
                Direccion = direccion,
                Telefono = rseparacion.icb_terceros.celular_tercero != null ? rseparacion.icb_terceros.celular_tercero : "" ,
                Telefonoempresa = empresa.telefono,
                Documentocliente = empresa.nit,
                Ciudad = ciudad != null ? ciudad : "",
                Bodega = rseparacion.bodega_concesionario.bodccs_nombre,
                Placa = rseparacion.idordentaller != null ? rseparacion.tencabezaorden.icb_vehiculo.plac_vh : "",
                Vehiculo = rseparacion.idordentaller != null ? rseparacion.tencabezaorden.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                CodigorRseparacion = rseparacion.separacion.ToString(),
                Repuestos = db.rseparacionmercancia.Where(x => x.separacion == id).ToList(),
               Fechadocumento = rseparacion.fec_creacion.ToString()

                };
         
            string nombre = "SeparacionMercancia_";
            nombre = nombre + "file.pdf";
  
            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("Cabecera", "rseparacionmercancias", new { codigoentrada = Rsepracion.CodigorRseparacion, dirempre = Rsepracion.Direccionempresa, telempre = Rsepracion.Telefonoempresa, ciudad = Rsepracion.Ciudad}, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("Descargarrseparacion", Rsepracion)
                {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                FileName = nombre,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 5 }
                };


            return something;
            }

        public ActionResult Cabecera(string codigoentrada, string dirempre, string telempre, string ciudad)
            {
            var recibido = Request;
            var fecha = DateTime.Now;
            var modelo2 = new PdfRseparacion
                {
                CodigorRseparacion = codigoentrada,
                Fechadocumento = fecha.ToString("yyyy-MM-dd"),
                Ciudad = ciudad,
                Direccionempresa = dirempre,
                Telefonoempresa = telempre,

                };

            return View(modelo2);
            }





        }
    }