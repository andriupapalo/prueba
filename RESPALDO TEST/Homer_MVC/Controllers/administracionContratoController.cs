using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class administracionContratoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: administracionContrato
        public ActionResult Create(int? menu)
        {
            ViewBag.tipocontrato = new SelectList(context.tipocontratocomercial.OrderBy(x => x.descripcion), "id",
                "descripcion");
            ViewBag.porceniva = new SelectList(context.codigo_iva.OrderBy(x => x.Descripcion), "id", "porcentaje");
            var buscarTerceros = (from tercero in context.icb_terceros
                                  join cliente in context.tercero_cliente
                                      on tercero.tercero_id equals cliente.tercero_id
                                  select new
                                  {
                                      tercero_id = cliente.cltercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.tercero = new SelectList(buscarTerceros.OrderBy(x => x.nombre), "tercero_id", "nombre");
            BuscarFavoritos(menu);
            return View(new ContratoModel { estado = true });
        }


        [HttpPost]
        public ActionResult Create(ContratoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                if (modelo.fechainicial >= modelo.fechafinal)
                {
                    TempData["mensaje_error"] =
                        "La fecha de inicio no puede ser mayor o igual a la fecha final del contrato!";
                }
                else
                {
                    contratoscomerciales buscarContratoNumero = context.contratoscomerciales.FirstOrDefault(x =>
                        x.numerocontrato == modelo.numerocontrato && x.tercero == modelo.tercero);
                    if (buscarContratoNumero != null)
                    {
                        TempData["mensaje_error"] = "El numero de contrato para el cliente seleccionado ya existe!";
                    }
                    else
                    {
                        context.contratoscomerciales.Add(new contratoscomerciales
                        {
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            numerocontrato = modelo.numerocontrato,
                            tercero = modelo.tercero,
                            razon_inactivo = modelo.razon_inactivo,
                            estado = modelo.estado,
                            fechainicial = modelo.fechainicial ?? DateTime.Now,
                            fechafinal = modelo.fechafinal ?? DateTime.Now,
                            valorcontrato = Convert.ToDecimal(modelo.valorcontrato, miCultura),
                            porceniva = modelo.porceniva,
                            valordescontado = Convert.ToDecimal(modelo.valordescontado, miCultura),
                            tipocontrato = modelo.tipocontrato
                        });
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El registro del nuevo contrato fue exitoso!";
                            ViewBag.tipocontrato =
                                new SelectList(context.tipocontratocomercial.OrderBy(x => x.descripcion), "id",
                                    "descripcion", modelo.tipocontrato);
                            ViewBag.porceniva = new SelectList(context.codigo_iva.OrderBy(x => x.Descripcion), "id",
                                "porcentaje");
                            var buscarTerceros1 = (from tercero in context.icb_terceros
                                                   join cliente in context.tercero_cliente
                                                       on tercero.tercero_id equals cliente.tercero_id
                                                   select new
                                                   {
                                                       tercero_id = cliente.cltercero_id,
                                                       nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " +
                                                                tercero.segnom_tercero + " " + tercero.apellido_tercero + " " +
                                                                tercero.segapellido_tercero + " " + tercero.razon_social
                                                   }).ToList();
                            ViewBag.tercero = new SelectList(buscarTerceros1.OrderBy(x => x.nombre), "tercero_id",
                                "nombre", modelo.tercero);
                            BuscarFavoritos(menu);
                            return RedirectToAction("Create", menu);
                        }

                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
            }

            ViewBag.tipocontrato = new SelectList(context.tipocontratocomercial.OrderBy(x => x.descripcion), "id",
                "descripcion", modelo.tipocontrato);
            ViewBag.porceniva = new SelectList(context.codigo_iva.OrderBy(x => x.Descripcion), "id", "porcentaje");
            var buscarTerceros = (from tercero in context.icb_terceros
                                  join cliente in context.tercero_cliente
                                      on tercero.tercero_id equals cliente.tercero_id
                                  select new
                                  {
                                      tercero_id = cliente.cltercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.tercero = new SelectList(buscarTerceros.OrderBy(x => x.nombre), "tercero_id", "nombre",
                modelo.tercero);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            contratoscomerciales contrato = context.contratoscomerciales.Find(id);
            if (contrato == null)
            {
                return HttpNotFound();
            }

            ContratoModel modelo = new ContratoModel
            {
                fec_creacion = contrato.fec_creacion,
                userid_creacion = contrato.userid_creacion,
                fec_actualizacion = contrato.fec_actualizacion,
                user_idactualizacion = contrato.user_idactualizacion,
                numerocontrato = contrato.numerocontrato,
                tercero = contrato.tercero,
                razon_inactivo = contrato.razon_inactivo,
                estado = contrato.estado,
                fechainicial = contrato.fechainicial,
                fechafinal = contrato.fechafinal,
                valorcontrato = contrato.valorcontrato.ToString(),
                porceniva = contrato.porceniva,
                valordescontado = contrato.valordescontado.ToString(),
                tipocontrato = contrato.tipocontrato,
                idcontrato = contrato.idcontrato
            };
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ViewBag.tipocontrato = new SelectList(context.tipocontratocomercial.OrderBy(x => x.descripcion), "id",
                "descripcion", modelo.tipocontrato);
            ViewBag.porceniva = new SelectList(context.codigo_iva.OrderBy(x => x.Descripcion), "id", "porcentaje");
            var buscarTerceros = (from tercero in context.icb_terceros
                                  join cliente in context.tercero_cliente
                                      on tercero.tercero_id equals cliente.tercero_id
                                  select new
                                  {
                                      tercero_id = cliente.cltercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.tercero = new SelectList(buscarTerceros.OrderBy(x => x.nombre), "tercero_id", "nombre",
                modelo.tercero);
            return View(modelo);
        }


        // POST: acteco_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(ContratoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                if (modelo.fechainicial >= modelo.fechafinal)
                {
                    TempData["mensaje_error"] =
                        "La fecha de inicio no puede ser mayor o igual a la fecha final del contrato!";
                }
                else
                {
                    contratoscomerciales buscarContratoNumero = context.contratoscomerciales.FirstOrDefault(x =>
                        x.numerocontrato == modelo.numerocontrato && x.tercero == modelo.tercero);
                    if (buscarContratoNumero != null)
                    {
                        if (buscarContratoNumero.idcontrato != modelo.idcontrato)
                        {
                            TempData["mensaje_error"] = "El cliente ya tiene un contrato asignado con ese número!";
                        }
                        else
                        {
                            buscarContratoNumero.numerocontrato = modelo.numerocontrato;
                            buscarContratoNumero.tipocontrato = modelo.tipocontrato;
                            buscarContratoNumero.tercero = modelo.tercero;
                            buscarContratoNumero.valorcontrato = Convert.ToDecimal(modelo.valorcontrato, miCultura);
                            buscarContratoNumero.valordescontado = Convert.ToDecimal(modelo.valordescontado, miCultura);
                            buscarContratoNumero.porceniva = modelo.porceniva;
                            buscarContratoNumero.fechainicial = modelo.fechainicial ?? DateTime.Now;
                            buscarContratoNumero.fechafinal = modelo.fechafinal ?? DateTime.Now;
                            buscarContratoNumero.estado = modelo.estado;
                            buscarContratoNumero.razon_inactivo = modelo.razon_inactivo;
                            buscarContratoNumero.fec_actualizacion = DateTime.Now;
                            buscarContratoNumero.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            context.Entry(buscarContratoNumero).State = EntityState.Modified;
                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                TempData["mensaje"] = "La actualización del nuevo contrato fue exitosa!";
                            }
                            else
                            {
                                TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                            }
                        }
                    }
                    else
                    {
                        contratoscomerciales contrato = context.contratoscomerciales.Find(modelo.idcontrato);
                        contrato.fec_actualizacion = DateTime.Now;
                        contrato.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        contrato.numerocontrato = modelo.numerocontrato;
                        contrato.tipocontrato = modelo.tipocontrato;
                        contrato.tercero = modelo.tercero;
                        contrato.valorcontrato = Convert.ToDecimal(modelo.valorcontrato, miCultura);
                        contrato.valordescontado = Convert.ToDecimal(modelo.valordescontado, miCultura);
                        contrato.porceniva = modelo.porceniva;
                        contrato.fechainicial = modelo.fechainicial ?? DateTime.Now;
                        contrato.fechafinal = modelo.fechafinal ?? DateTime.Now;
                        contrato.estado = modelo.estado;
                        contrato.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(contrato).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del nuevo contrato fue exitosa!";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                        }
                    }
                }
            }

            modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            modelo.fec_actualizacion = DateTime.Now;
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ViewBag.tipocontrato = new SelectList(context.tipocontratocomercial.OrderBy(x => x.descripcion), "id",
                "descripcion", modelo.tipocontrato);
            ViewBag.porceniva = new SelectList(context.codigo_iva.OrderBy(x => x.Descripcion), "id", "porcentaje");
            var buscarTerceros = (from tercero in context.icb_terceros
                                  join cliente in context.tercero_cliente
                                      on tercero.tercero_id equals cliente.tercero_id
                                  select new
                                  {
                                      tercero_id = cliente.cltercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.tercero = new SelectList(buscarTerceros.OrderBy(x => x.nombre), "tercero_id", "nombre",
                modelo.tercero);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(ContratoModel modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.contratoscomerciales on c.user_id equals b.userid_creacion
                             where b.idcontrato == modelo.idcontrato
                             select c).FirstOrDefault();
            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.contratoscomerciales on c.user_id equals b.user_idactualizacion
                                  where b.idcontrato == modelo.idcontrato
                                  select c).FirstOrDefault();
            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult ConsultarContratosJson()
        {
            var buscarContratos = (from contrato in context.contratoscomerciales
                                   join tipoContrato in context.tipocontratocomercial
                                       on contrato.tipocontrato equals tipoContrato.id
                                   join cliente in context.tercero_cliente
                                       on contrato.tercero equals cliente.cltercero_id
                                   join tercero in context.icb_terceros
                                       on cliente.tercero_id equals tercero.tercero_id
                                   select new
                                   {
                                       contrato.idcontrato,
                                       tipoContrato.descripcion,
                                       contrato.numerocontrato,
                                       tercero = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                                                 " " + tercero.segapellido_tercero + " " + tercero.razon_social,
                                       contrato.fechainicial,
                                       contrato.fechafinal,
                                       contrato.estado
                                   }).ToList();
            var data = buscarContratos.Select(x => new
            {
                x.idcontrato,
                x.descripcion,
                x.numerocontrato,
                x.tercero,
                fechainicial = x.fechainicial != null ? x.fechainicial.ToShortDateString() : "",
                fechafinal = x.fechafinal != null ? x.fechafinal.ToShortDateString() : "",
                estado = x.estado ? "Activo" : "Inactivo"
            });
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