using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class inventario_vhUsadoController : Controller
    {
        //private VehiculosContexto context = new VehiculosContexto();
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void listas(icb_vehiculo vh)
        {
            ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre", vh.tpvh_id);
            ViewBag.marcvh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre", vh.marcvh_id);
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre", vh.modvh_id);
            ViewBag.estilos = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre", vh.colvh_id);
            ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre");
            ViewBag.ciumanf_vh = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", vh.ciumanf_vh);

            ViewBag.asignado = context.icb_vehiculo.Where(x => x.asignado != null).Count();
            ViewBag.totalVehiculos = context.icb_vehiculo.Where(x => x.usado == true).Count();

            users creator = context.users.Find(vh.icbvhuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(vh.icbvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                items.Add(new SelectListItem
                {
                    Text = item.nombreTErcero,
                    Value = item.idTercero.ToString()
                });
            }

            ViewBag.proveedor_id = items;
        }

        // GET: inventario_vhUsado
        public ActionResult Index(int? menu)
        {
            icb_vehiculo vh = new icb_vehiculo();
            listas(vh);
            BuscarFavoritos(menu);
            return View(vh);
        }


        public ActionResult Create(int? menu)
        {
            icb_vehiculo vh = new icb_vehiculo();
            listas(vh);
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Update(string plan_mayor, int? menu)
        {
            icb_vehiculo vehiculo = context.icb_vehiculo.Find(plan_mayor);
            listas(vehiculo);
            BuscarFavoritos(menu);
            return View(vehiculo);
        }

        [HttpPost]
        public ActionResult Update(icb_vehiculo vehiculo, int? menu)
        {
            vehiculo.icbvhfec_actualizacion = DateTime.Now;
            vehiculo.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            context.Entry(vehiculo).State = EntityState.Modified;
            context.SaveChanges();

            listas(vehiculo);
            BuscarFavoritos(menu);
            return View(vehiculo);
        }


        public JsonResult PaginarLista(int pagina)
        {
            var result = (from icb in context.icb_vehiculo
                          join mar in context.marca_vehiculo
                              on icb.marcvh_id equals mar.marcvh_id
                          join est in context.estilo_vehiculo
                              on icb.estvh_id equals est.estvh_id
                          join mod in context.modelo_vehiculo
                              on icb.modvh_id equals mod.modvh_codigo
                          orderby icb.icbvh_id
                          select new
                          {
                              marca = mar.marcvh_nombre,
                              motor = icb.nummot_vh,
                              estilo = est.estvh_nombre,
                              modelo = mod.modvh_nombre,
                              serie = icb.vin,
                              placa = icb.plac_vh,
                              id = icb.icbvh_id
                          }).Skip(pagina * 10 - 10).Take(10).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarModelos(int marca)
        {
            List<modelo_vehiculo> marcas = context.modelo_vehiculo.Where(x => x.mar_vh_id == marca).ToList();
            var result = marcas.Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult BuscarEstilos(int modelo)
        //{
        //    var modelos = context.estilo_vehiculo.Where(x => x.mod_vh_id == modelo).ToList();
        //    var result = modelos.Select(x => new {
        //        x.estvh_id,
        //        x.estvh_nombre
        //    });
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}


        public JsonResult AgregarVehiculo(int marca, string modelo, int estilo, string color, string serie,
            string motor, int anio, string placa, int clasificacion)
        {
            int result = 0;
            int motorVh = context.icb_vehiculo.Where(x => x.nummot_vh == motor).Count();
            if (motorVh > 0 && motor.Length > 0)
            {
                result = -1;
            }

            int serieVh = context.icb_vehiculo.Where(x => x.vin == serie).Count();
            if (serieVh > 0 && serie.Length > 0)
            {
                result = -2;
            }

            int placaVh = context.icb_vehiculo.Where(x => x.plac_vh == placa).Count();
            if (placaVh > 0 && placa.Length > 0)
            {
                result = -3;
            }

            if (motorVh == 0 && serieVh == 0 && placaVh == 0)
            {
                icb_vehiculo vehiculoNuevo = new icb_vehiculo
                {
                    anio_vh = anio,
                    colvh_id = color,
                    estvh_id = estilo,
                    icbvhfec_creacion = DateTime.Now,
                    icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    marcvh_id = marca,
                    modvh_id = modelo,
                    nummot_vh = motor,
                    plac_vh = placa,
                    // Id del tipo de vehiculo usado registrado en la BD
                    tpvh_id = 5,
                    vin = serie,
                    clavh_id = clasificacion
                };
                context.icb_vehiculo.Add(vehiculoNuevo);
                result = context.SaveChanges();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarVhUsadosCant(string texto)
        {
            int result = (from icb in context.icb_vehiculo
                          join mar in context.marca_vehiculo
                              on icb.marcvh_id equals mar.marcvh_id
                          join est in context.estilo_vehiculo
                              on icb.estvh_id equals est.estvh_id
                          join mod in context.modelo_vehiculo
                              on icb.modvh_id equals mod.modvh_codigo
                          where (mar.marcvh_nombre.Contains(texto)
                                 || est.estvh_nombre.Contains(texto)
                                 || mod.modvh_nombre.Contains(texto)
                                 || icb.vin.Contains(texto)
                                 || icb.plac_vh.Contains(texto)
                                 || icb.nummot_vh.Contains(texto)) && icb.tpvh_id == 5
                          orderby icb.icbvh_id
                          select new
                          {
                              marca = mar.marcvh_nombre,
                              motor = icb.nummot_vh,
                              estilo = est.estvh_nombre,
                              modelo = mod.modvh_nombre,
                              serie = icb.vin,
                              placa = icb.plac_vh,
                              id = icb.icbvh_id
                          }).ToList().Count;

            var result2 = (from icb in context.icb_vehiculo
                           join mar in context.marca_vehiculo
                               on icb.marcvh_id equals mar.marcvh_id
                           join est in context.estilo_vehiculo
                               on icb.estvh_id equals est.estvh_id
                           join mod in context.modelo_vehiculo
                               on icb.modvh_id equals mod.modvh_codigo
                           where (mar.marcvh_nombre.Contains(texto)
                                  || est.estvh_nombre.Contains(texto)
                                  || mod.modvh_nombre.Contains(texto)
                                  || icb.vin.Contains(texto)
                                  || icb.plac_vh.Contains(texto)
                                  || icb.nummot_vh.Contains(texto)) && icb.tpvh_id == 5
                           orderby icb.icbvh_id
                           select new
                           {
                               marca = mar.marcvh_nombre,
                               motor = icb.nummot_vh,
                               estilo = est.estvh_nombre,
                               modelo = mod.modvh_nombre,
                               serie = icb.vin ?? "",
                               placa = icb.plac_vh,
                               id = icb.icbvh_id
                           }).Skip(1 * 10 - 10).Take(10).ToList();
            var data = new
            {
                result,
                result2
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarDatosBrowser()
        {
            var data = (from vehiculo in context.icb_vehiculo
                        join modelo in context.modelo_vehiculo
                            on vehiculo.modvh_id equals modelo.modvh_codigo
                        join color in context.color_vehiculo
                            on vehiculo.colvh_id equals color.colvh_id
                        join evento in context.icb_tpeventos
                            on vehiculo.id_evento equals evento.tpevento_id
                        where vehiculo.usado == true
                        select new
                        {
                            vehiculo.nummot_vh,
                            modelo.modvh_nombre,
                            color.colvh_nombre,
                            placa = vehiculo.plac_vh,
                            fecfact_fabrica = vehiculo.fecfact_fabrica.ToString(),
                            anomodelo = vehiculo.anio_vh,
                            vehiculo.plan_mayor,
                            evento.tpevento_nombre,
                            dias_inventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                        }).ToList().OrderByDescending(x => x.fecfact_fabrica);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        // GET: inventario_vhUsado
        //public ActionResult Update(int id)
        //{
        //    var vehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);
        //    ViewBag.tipos = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
        //    ViewBag.marcas = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre",vehiculo.marcvh_id);
        //    var modelosActual = context.modelo_vehiculo.Where(x => x.mar_vh_id == vehiculo.marcvh_id);
        //    ViewBag.modelos = new SelectList(modelosActual, "modvh_id", "modvh_nombre",vehiculo.modvh_id);
        //    var estilosActual = context.estilo_vehiculo.Where(x => x.mod_vh_id == vehiculo.modvh_id);
        //    ViewBag.estilos = new SelectList(estilosActual, "estvh_id", "estvh_nombre",vehiculo.estvh_id);
        //    ViewBag.color = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre",vehiculo.colvh_id);
        //    ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre",vehiculo.clavh_id);

        //    return View(vehiculo);
        //}


        public JsonResult ActualizarVehiculo(int id, int marca, string modelo, int estilo, string color, string serie,
            string motor, int anio, string placa, int clasificacion)
        {
            int result = 0;
            icb_vehiculo QueryVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);

            icb_vehiculo motorVh = context.icb_vehiculo.FirstOrDefault(x => x.nummot_vh == motor);
            icb_vehiculo serieVh = context.icb_vehiculo.FirstOrDefault(x => x.vin == serie);
            icb_vehiculo placaVh = context.icb_vehiculo.FirstOrDefault(x => x.plac_vh == placa);

            if (motorVh != null && QueryVehiculo.icbvh_id != motorVh.icbvh_id)
            {
                result = -1;
            }
            else if (serieVh != null && QueryVehiculo.icbvh_id != serieVh.icbvh_id)
            {
                result = -2;
            }
            else if (placaVh != null && QueryVehiculo.icbvh_id != placaVh.icbvh_id)
            {
                result = -3;
            }
            else
            {
                QueryVehiculo.anio_vh = anio;
                QueryVehiculo.colvh_id = color;
                QueryVehiculo.estvh_id = estilo;
                QueryVehiculo.icbvhfec_actualizacion = DateTime.Now;
                QueryVehiculo.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                QueryVehiculo.marcvh_id = marca;
                QueryVehiculo.modvh_id = modelo;
                QueryVehiculo.nummot_vh = motor;
                QueryVehiculo.plac_vh = placa;
                QueryVehiculo.vin = serie;
                QueryVehiculo.clavh_id = clasificacion;
                //icbvh_id=id,
                //anio_vh = anio,
                //colvh_id = color,
                //estvh_id = estilo,
                //icbvhfec_actualizacion = DateTime.Now,
                //icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                //marcvh_id = marca,
                //modvh_id = modelo,
                //nummot_vh = motor,
                //plac_vh = placa,
                //// Id del tipo de vehiculo usado registrado en la BD
                //tpvh_id = 5,
                //num_serie = serie,
                //clavh_id = clasificacion

                context.Entry(QueryVehiculo).State = EntityState.Modified;
                result = context.SaveChanges();
            }

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