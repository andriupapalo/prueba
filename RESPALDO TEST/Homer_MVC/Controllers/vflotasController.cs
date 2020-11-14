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
    public class vflotasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        public void listas(flotasModel vflotas)
        {
            var flota = (from t in db.vcodflota
                         select new
                         {
                             t.id,
                             nombre = t.codigo + " - " + t.Descripcion
                         }).ToList();
            List<SelectListItem> lista_flota = new List<SelectListItem>();
            foreach (var item in flota)
            {
                lista_flota.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = item.id.ToString() == vflotas.flota.ToString() ? true : false
                });
            }

            var cliente = (from t in db.icb_terceros
                           join tp in db.tercero_cliente
                               on t.tercero_id equals tp.tercero_id
                           select new
                           {
                               t.tercero_id,
                               nombre = t.prinom_tercero != null
                                   ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero
                                   : t.doc_tercero + " - " + t.razon_social
                           }).ToList();
            List<SelectListItem> lista_cliente = new List<SelectListItem>();
            foreach (var item in cliente)
            {
                lista_cliente.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id.ToString() == vflotas.nit_flota.ToString() ? true : false
                });
            }

            ViewBag.nit_flota = lista_cliente;
            ViewBag.documentos = db.vdocumentosflota.Where(x => x.estado).ToList();
            ViewBag.flota = lista_flota;
            ViewBag.fec_solicitud = vflotas.fec_solicitud;
        }


        // GET: vflotas/Create
        public ActionResult Create(int? menu)
        {
            flotasModel vflota = new flotasModel();
            listas(vflota);
            BuscarFavoritos(menu);
            return View();
        }


        // POST: vflotas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(flotasModel vflota, int? menu)
        {
            if (ModelState.IsValid)
            {
                vflota existe = db.vflota.FirstOrDefault(x => x.flota == vflota.flota && x.numero == vflota.numero);
                if (existe == null)
                {
                    var flotax = new vflota
                    {
                        nit_flota=vflota.nit_flota,
                        detalle=vflota.detalle,
                        fec_solicitud=Convert.ToDateTime(vflota.fec_solicitud,new CultureInfo("en-US")),
                        flota=vflota.flota,
                        numero=vflota.numero,                       
                    };
                    db.vflota.Add(flotax);
                    db.SaveChanges();

                    TempData["mensaje"] = "Registro creado correctamente";
                    return RedirectToAction("Edit", new { id = flotax.idflota });
                }

                TempData["mensaje_error"] = "La flota ingresada ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error en la creación de la flota, por favor valide";
            }

            listas(vflota);
            BuscarFavoritos(menu);
            return View(vflota);
        }

        // GET: vflotas/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vflota vflota = db.vflota.Where(d=>d.idflota==id).FirstOrDefault();
            if (vflota == null)
            {
                return HttpNotFound();
            }
            var flotax = new flotasModel
            {
                nit_flota = vflota.nit_flota,
                detalle = vflota.detalle,
                fec_solicitud = vflota.fec_solicitud.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                flota = vflota.flota,
                idflota = vflota.idflota,
                numero = vflota.numero,
            };
           
            listas(flotax);
            BuscarFavoritos(menu);
            return View(flotax);
        }

        // POST: vflotas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(flotasModel vflotaz, int? menu)
        {
            if (ModelState.IsValid)
            {
                var flotax = db.vflota.Where(d => d.idflota == vflotaz.idflota).FirstOrDefault();

                
                    flotax.nit_flota = vflotaz.nit_flota;
                    flotax.detalle = vflotaz.detalle;
                    flotax.fec_solicitud = Convert.ToDateTime(vflotaz.fec_solicitud, new CultureInfo("en-US"));
                    flotax.flota = vflotaz.flota;
                    flotax.numero = vflotaz.numero;             

                db.Entry(flotax).State = EntityState.Modified;


                db.SaveChanges();


                TempData["mensaje"] = "Registro actualizado correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Error en la actualización de la flota, por favor valide";
            }

            listas(vflotaz);
            BuscarFavoritos(menu);
            return View(vflotaz);
        }


        public JsonResult BuscarDatos()
        {
            var data = from f in db.vflota
                       select new
                       {
                           f.flota,
                           id = f.idflota,
                           detalle = f.detalle.ToString(),
                           fecha = f.fec_solicitud != null ? f.fec_solicitud.ToString() : "",
                           nit_flota = f.icb_terceros.prinom_tercero != null
                               ? f.icb_terceros.prinom_tercero + " " + f.icb_terceros.apellido_tercero
                               : f.icb_terceros.razon_social
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
    }
}