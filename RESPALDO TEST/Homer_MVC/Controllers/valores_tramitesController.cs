using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class valores_tramitesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: valores_trasmites
        public ActionResult Index(int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            var valores_trasmites = (from a in db.valores_trasmites
                                     join b in db.valortramitebodega
                                         on a.idvalor equals b.idvalortramite
                                     where b.idbodega == bodega
                                     select new
                                     {
                                         a.sijin
                                     }).ToList();
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            BuscarFavoritos(menu);
            return View(valores_trasmites.ToList());
        }


        // GET: valores_trasmites/Create
        public ActionResult Create(int? menu)
        {
            //ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x=>x.ciu_nombre).ToList(), "ciu_id", "ciu_nombre");
            //ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id", "dpto_nombre");
            ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: valores_trasmites/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(valoresTramiteModel valores, int? menu)

        {
            valores_trasmites valores_trasmites = new valores_trasmites();
            string bodegasSeleccionadas = Request["bodccs_cod"];

            string[] arregloBodegas = bodegasSeleccionadas.Split(',');

            if (ModelState.IsValid)
            {
                valores_trasmites existe = db.valores_trasmites.FirstOrDefault(x =>
                    x.ciudad_id == valores_trasmites.ciudad_id && x.bodega == valores_trasmites.bodega);
                if (existe == null)
                {
                    valores_trasmites.fec_creacion = DateTime.Now;
                    valores_trasmites.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    valores_trasmites.estado = valores.estado;
                    valores_trasmites.razon_inactivo = valores.razon_inactivo;
                    valores_trasmites.ciudad_id = valores.ciudad_id;
                    valores_trasmites.sijin = Convert.ToDecimal(valores.sijin);
                    valores_trasmites.traspaso = Convert.ToDecimal(valores.traspaso);
                    valores_trasmites.tarjeta = Convert.ToDecimal(valores.tarjeta);
                    valores_trasmites.runt = Convert.ToDecimal(valores.runt);
                    valores_trasmites.traslado_cuenta = Convert.ToDecimal(valores.traslado_cuenta);
                    valores_trasmites.radicacion_cuenta = Convert.ToDecimal(valores.radicacion_cuenta);
                    valores_trasmites.consig_minist_tte = Convert.ToDecimal(valores.consig_minist_tte);
                    valores_trasmites.prenda = Convert.ToDecimal(valores.prenda);
                    valores_trasmites.serv_tramitador = Convert.ToDecimal(valores.serv_tramitador);
                    valores_trasmites.antec_pazysalvo = Convert.ToDecimal(valores.antec_pazysalvo);
                    valores_trasmites.estampillas = Convert.ToDecimal(valores.estampillas);
                    valores_trasmites.semaforizacion = Convert.ToDecimal(valores.semaforizacion);
                    valores_trasmites.tradicion = Convert.ToDecimal(valores.tradicion);
                    valores_trasmites.copia_factura = Convert.ToDecimal(valores.copia_factura);
                    valores_trasmites.sistematizacion_impuestos = Convert.ToDecimal(valores.sistematizacion_impuestos);
                    valores_trasmites.derechos_transito = Convert.ToDecimal(valores.derechos_transito);
                    valores_trasmites.sistematizacion = Convert.ToDecimal(valores.sistematizacion);
                    valores_trasmites.cert_transito = Convert.ToDecimal(valores.cert_transito);
                    valores_trasmites.bodega = Request["bodccs_cod"];
                    db.valores_trasmites.Add(valores_trasmites);
                    db.SaveChanges();
                    valortramitebodega obj = new valortramitebodega();
                    for (int i = 0; i < arregloBodegas.Length; i++)
                    {
                        obj.idvalortramite = valores_trasmites.idvalor;
                        obj.idbodega = Convert.ToInt32(arregloBodegas[i]);
                        db.valortramitebodega.Add(obj);
                        db.SaveChanges();
                    }

                    TempData["mensaje"] = "Valores guardados correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Ya existen valores para la ciudad ingresada, por favor valide";
                }

                ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                    "ciu_nombre", valores_trasmites.ciudad_id).OrderBy(x => x.Text);
                ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id",
                    "dpto_nombre");
                ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
                ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

                //return RedirectToAction("Edit", new { id= valores_trasmites.idvalor, menu });
                return RedirectToAction("Create");
            }

            TempData["mensaje_error"] = "Error al guardar los valores, por favor valide";

            ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                "ciu_nombre", valores_trasmites.ciudad_id).OrderBy(x => x.Text);
            ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id", "dpto_nombre");
            ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            BuscarFavoritos(menu);
            return View(valores_trasmites);
        }

        // GET: valores_trasmites/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            valores_trasmites valores_trasmites = db.valores_trasmites.Find(id);
            if (valores_trasmites == null)
            {
                return HttpNotFound();
            }

            valoresTramiteModel valores = new valoresTramiteModel
            {
                fecha_creacion = valores_trasmites.fec_creacion,
                user_creacion = valores_trasmites.userid_creacion
            };
            valores.fecha_actualizacion = valores.fecha_actualizacion;
            valores.user_actualizacion = valores_trasmites.user_idactualizacion;
            valores.estado = valores_trasmites.estado;
            valores.razon_inactivo = valores_trasmites.razon_inactivo;
            valores.idvalor = valores_trasmites.idvalor;
            valores.ciudad_id = valores_trasmites.ciudad_id;
            valores.sijin = Convert.ToString(valores_trasmites.sijin);
            valores.traspaso = Convert.ToString(valores_trasmites.traspaso);
            valores.tarjeta = Convert.ToString(valores_trasmites.tarjeta);
            valores.runt = Convert.ToString(valores_trasmites.runt);
            valores.traslado_cuenta = Convert.ToString(valores_trasmites.traslado_cuenta);
            valores.radicacion_cuenta = Convert.ToString(valores_trasmites.radicacion_cuenta);
            valores.consig_minist_tte = Convert.ToString(valores_trasmites.consig_minist_tte);
            valores.prenda = Convert.ToString(valores_trasmites.prenda);
            valores.serv_tramitador = Convert.ToString(valores_trasmites.serv_tramitador);
            valores.antec_pazysalvo = Convert.ToString(valores_trasmites.antec_pazysalvo);
            valores.estampillas = Convert.ToString(valores_trasmites.estampillas);
            valores.semaforizacion = Convert.ToString(valores_trasmites.semaforizacion);
            valores.tradicion = Convert.ToString(valores_trasmites.tradicion);
            valores.copia_factura = Convert.ToString(valores_trasmites.copia_factura);
            valores.sistematizacion = Convert.ToString(valores_trasmites.sistematizacion);
            valores.derechos_transito = Convert.ToString(valores_trasmites.derechos_transito);
            valores.sistematizacion_impuestos = Convert.ToString(valores_trasmites.sistematizacion_impuestos);
            valores.cert_transito = Convert.ToString(valores_trasmites.cert_transito);


            nom_ciudad buscarCiudad = db.nom_ciudad.FirstOrDefault(x => x.ciu_id == valores_trasmites.ciudad_id);
            ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                "ciu_nombre", valores_trasmites.ciudad_id).OrderBy(x => x.Text);
            int dptoId = buscarCiudad != null ? buscarCiudad.dpto_id : 0;
            ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id", "dpto_nombre",
                dptoId);
            nom_departamento buscarDpto = db.nom_departamento.FirstOrDefault(x => x.dpto_id == dptoId);
            int paisId = buscarDpto != null ? buscarDpto.pais_id : 0;
            ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre", paisId);
            //var buscarBodega = db.bodega_concesionario.FirstOrDefault(x => x.id == Convert.ToInt32(valores_trasmites.valortramitebodega.Select(a => a.idbodega)));
            //ViewBag.bodega = new SelectList(db.bodega_concesionario.OrderByDescending(x => x.bodccs_nombre), "id", "bodccs_nombre");
            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            var buscarBodegas = from bodegas in db.valortramitebodega
                                where bodegas.idvalortramite == id
                                select new { bodegas.idbodega };
            string bodegasString = "";
            bool primera = true;
            foreach (var item in buscarBodegas)
            {
                if (primera)
                {
                    bodegasString += item.idbodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.idbodega;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;
            BuscarFavoritos(menu);
            return View(valores);
        }

        // POST: valores_trasmites/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(valoresTramiteModel valores, int? menu)
        {
            nom_ciudad buscarCiudad = db.nom_ciudad.FirstOrDefault(x => x.ciu_id == valores.ciudad_id);
            int dptoId = buscarCiudad != null ? buscarCiudad.dpto_id : 0;
            nom_departamento buscarDpto = db.nom_departamento.FirstOrDefault(x => x.dpto_id == dptoId);
            int paisId = buscarDpto != null ? buscarDpto.pais_id : 0;
            string bodegasSeleccionadas = Request["bodccs_cod"];

            string[] arregloBodegas = bodegasSeleccionadas.Split(',');

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                    ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id",
                        "dpto_nombre", dptoId);
                    ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre",
                        paisId);
                    ViewBag.bodega = new SelectList(db.bodega_concesionario.OrderByDescending(x => x.bodccs_nombre),
                        "id", "bodccs_nombre");
                    ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                        "ciu_nombre", valores.ciudad_id).OrderBy(x => x.Text);
                    BuscarFavoritos(menu ?? 0);
                    return View();
                }

                int buscar = db.valores_trasmites.Where(x => x.idvalor == valores.idvalor).Count();
                if (buscar == 1)
                {
                    valores_trasmites seleccionar = db.valores_trasmites.FirstOrDefault(x => x.idvalor == valores.idvalor);
                    seleccionar.fec_actualizacion = DateTime.Now;
                    seleccionar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    seleccionar.estado = valores.estado;
                    seleccionar.razon_inactivo = valores.razon_inactivo;
                    seleccionar.ciudad_id = valores.ciudad_id;
                    seleccionar.sijin = Convert.ToDecimal(valores.sijin);
                    seleccionar.traspaso = Convert.ToDecimal(valores.traspaso);
                    seleccionar.tarjeta = Convert.ToDecimal(valores.tarjeta);
                    seleccionar.runt = Convert.ToDecimal(valores.runt);
                    seleccionar.traslado_cuenta = Convert.ToDecimal(valores.traslado_cuenta);
                    seleccionar.radicacion_cuenta = Convert.ToDecimal(valores.radicacion_cuenta);
                    seleccionar.consig_minist_tte = Convert.ToDecimal(valores.consig_minist_tte);
                    seleccionar.prenda = Convert.ToDecimal(valores.prenda);
                    seleccionar.serv_tramitador = Convert.ToDecimal(valores.serv_tramitador);
                    seleccionar.antec_pazysalvo = Convert.ToDecimal(valores.antec_pazysalvo);
                    seleccionar.estampillas = Convert.ToDecimal(valores.estampillas);
                    seleccionar.semaforizacion = Convert.ToDecimal(valores.semaforizacion);
                    seleccionar.tradicion = Convert.ToDecimal(valores.tradicion);
                    seleccionar.copia_factura = Convert.ToDecimal(valores.copia_factura);
                    seleccionar.sistematizacion_impuestos = Convert.ToDecimal(valores.sistematizacion_impuestos);
                    seleccionar.derechos_transito = Convert.ToDecimal(valores.derechos_transito);
                    seleccionar.sistematizacion = Convert.ToDecimal(valores.sistematizacion);
                    seleccionar.cert_transito = Convert.ToDecimal(valores.cert_transito);
                    seleccionar.bodega = Request["bodccs_cod"];
                    db.Entry(seleccionar).State = EntityState.Modified;
                    int guardar = db.SaveChanges();
                    if (guardar > 0)
                    {
                        // Se agregan los tipos de tramites correspondientes al usuario que se va a crear


                        if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                        {
                            const string query = "DELETE FROM [dbo].[valortramitebodega] WHERE [idvalortramite]={0}";
                            int rows = db.Database.ExecuteSqlCommand(query, valores.idvalor);
                            string[] bodegasId = bodegasSeleccionadas.Split(',');
                            foreach (string substring in bodegasId)
                            {
                                db.valortramitebodega.Add(new valortramitebodega
                                {
                                    idbodega = Convert.ToInt32(substring),
                                    idvalortramite = valores.idvalor
                                });
                            }

                            int guardarBodegas = db.SaveChanges();
                        }

                        TempData["mensaje"] = "La actualización de los valores tramites fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "No se pudo actualizar!";
                    }
                }
            }

            ViewBag.dpto_id = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id", "dpto_nombre",
                dptoId);
            ViewBag.pais_id = new SelectList(db.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre", paisId);
            ViewBag.bodega = new SelectList(db.bodega_concesionario.OrderByDescending(x => x.bodccs_nombre), "id",
                "bodccs_nombre");
            ViewBag.ciudad_id = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                "ciu_nombre", valores.ciudad_id).OrderBy(x => x.Text);
            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            var buscarBodegas = from bodegas in db.valortramitebodega
                                where bodegas.idvalortramite == valores.idvalor
                                select new { bodegas.idbodega };
            string bodegasString = "";
            bool primera = true;
            foreach (var item in buscarBodegas)
            {
                if (primera)
                {
                    bodegasString += item.idbodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.idbodega;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;


            BuscarFavoritos(menu);
            return View(valores);
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

        public JsonResult BuscarValoresPaginados()
        {
            var paginarDatos = (from a in db.valores_trasmites
                                join b in db.nom_ciudad
                                    on a.ciudad_id equals b.ciu_id
                                where a.estado==true
                                select new
                                {
                                    id = a.idvalor,
                                    ciudad = b.ciu_nombre != null ? b.ciu_nombre : "",
                                    sijin = a.sijin != null ? a.sijin : 0,
                                    traspaso = a.traspaso != null ? a.traspaso : 0,
                                    tarjeta = a.tarjeta != null ? a.tarjeta : 0,
                                    runt = a.runt != null ? a.runt : 0,
                                    traslado_cuenta = a.traslado_cuenta != null ? a.traslado_cuenta : 0,
                                    radicacion_cuenta = a.radicacion_cuenta != null ? a.radicacion_cuenta : 0,
                                    consig_minist_tte = a.consig_minist_tte != null ? a.consig_minist_tte : 0,
                                    prenda = a.prenda != null ? a.prenda : 0,
                                    serv_tramitador = a.serv_tramitador != null ? a.serv_tramitador : 0,
                                    antec_pazysalvo = a.antec_pazysalvo != null ? a.antec_pazysalvo : 0,
                                    estampillas = a.estampillas != null ? a.estampillas : 0,
                                    semaforizacion = a.semaforizacion != null ? a.semaforizacion : 0,
                                    tradicion = a.tradicion != null ? a.tradicion : 0,
                                    copia_factura = a.copia_factura != null ? a.copia_factura : 0,
                                    sistematizacion_impuestos = a.sistematizacion_impuestos != null ? a.sistematizacion_impuestos : 0,
                                    derechos_transito = a.derechos_transito != null ? a.derechos_transito : 0,
                                    sistematizacion = a.sistematizacion != null ? a.sistematizacion : 0,
                                    cert_transito = a.cert_transito != null ? a.cert_transito : 0,
                                    a.estado
                                }).ToList();


            var data = new
            {
                paginarDatos
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}