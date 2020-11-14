using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class movcontablesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void listas()
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 19);

            ViewBag.nit = context.icb_terceros.ToList();
            ViewBag.nitAdmin = context.icb_terceros.ToList();

            var nits = from t in context.icb_terceros
                       join i in context.tercero_empleado
                           on t.tercero_id equals i.tercero_id
                       select new
                       {
                           nombre = t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                    t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social,
                           i.emp_tercero_id
                       };
            List<SelectListItem> nitAdmin = new List<SelectListItem>();
            foreach (var item in nits)
            {
                nitAdmin.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.emp_tercero_id.ToString()
                });
            }

            ViewBag.nitAdmin = nitAdmin;

            ViewBag.cuenta = context.cuenta_puc.Where(x => x.esafectable).ToList();
            ViewBag.ccosto = context.centro_costo.ToList();
        }

        // GET: movcontables
        public ActionResult Create(int? menu)
        {
            listas();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(encab_documento encabezado, int? menu)
        {
            listas();

            string lista = Request["lista"];
            int datos = Convert.ToInt32(lista);

            //consecutivo
            int grupo = context.grupoconsecutivos
                .FirstOrDefault(x => x.documento_id == encabezado.tipo && x.bodega_id == encabezado.bodega).grupo;
            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
            long consecutivo = doc.BuscarConsecutivo(grupo);

            encabezado.numero = consecutivo;
            encabezado.impoconsumo = 0;
            encabezado.fec_creacion = DateTime.Now;
            encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            encabezado.estado = true;
            context.encab_documento.Add(encabezado);
            context.SaveChanges();


            int prueba = encabezado.idencabezado;
            int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault()
                .idencabezado;

            int total = Convert.ToInt32(lista);
            for (int i = 1; i <= total; i++)
            {
                if (!string.IsNullOrEmpty(Request["cuenta_" + i]))
                {
                    mov_contable mov = new mov_contable
                    {
                        id_encab = id_encabezado,
                        seq = i,
                        idparametronombre = 19,
                        cuenta = Convert.ToInt32(Request["cuenta_" + i])
                    };

                    if (!string.IsNullOrEmpty(Request["centro_" + i]))
                    {
                        mov.centro = Convert.ToInt32(Request["centro_" + i]);
                    }
                    else
                    {
                        var centroCosto = (from c in context.centro_costo
                                           where c.pre_centcst == "0"
                                           select new
                                           {
                                               c.centcst_id
                                           }).FirstOrDefault();

                        mov.centro = Convert.ToInt32(centroCosto.centcst_id);
                    }

                    if (!string.IsNullOrEmpty(Request["nit_" + i]))
                    {
                        mov.nit = Convert.ToInt32(Request["nit_" + i]);
                    }
                    else
                    {
                        var nitCero = (from t in context.icb_terceros
                                       where t.doc_tercero == "0"
                                       select new
                                       {
                                           t.tercero_id
                                       }).FirstOrDefault();

                        mov.nit = Convert.ToInt32(nitCero.tercero_id);
                    }

                    if (!string.IsNullOrEmpty(Request["nitAdmin_" + i]))
                    {
                        mov.idterceroadmin = Convert.ToInt32(Request["nitAdmin_" + i]);
                    }

                    mov.fec = encabezado.fecha;
                    if (!string.IsNullOrEmpty(Request["debito_" + i]))
                    {
                        mov.debito = Convert.ToDecimal(Request["debito_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["credito_" + i]))
                    {
                        mov.credito = Convert.ToDecimal(Request["credito_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["base_" + i]))
                    {
                        mov.basecontable = Convert.ToDecimal(Request["debitoNiff_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["debitoNiff_" + i]))
                    {
                        mov.debitoniif = Convert.ToDecimal(Request["debitoNiff_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["creditoNiff_" + i]))
                    {
                        mov.creditoniif = Convert.ToDecimal(Request["creditoNiff_" + i]);
                    }

                    mov.detalle = Request["detalle_" + i];
                    mov.fec_creacion = DateTime.Now;
                    mov.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    mov.estado = true;
                    context.mov_contable.Add(mov);
                    context.SaveChanges();
                }
            }

            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
            doc.ActualizarConsecutivo(grupo, consecutivo);

            TempData["mensaje"] = "Registro creado correctamente";

            context.SaveChanges();
            return RedirectToAction("Create", new { id = encabezado.idencabezado, menu });
        }


        public ActionResult Edit(int id, int? menu)
        {
            listas();

            encab_documento encabezado = context.encab_documento.Find(id);

            ViewBag.movimientos = context.mov_contable.Where(x => x.id_encab == encabezado.idencabezado).ToList();

            BuscarFavoritos(menu);

            return View(encabezado);
        }

        [HttpPost]
        public ActionResult Edit(encab_documento encabezado, int? menu)
        {
            listas();

            string lista = Request["lista"];
            int datos = Convert.ToInt32(lista);

            encabezado.impoconsumo = 0;
            encabezado.fec_actualizacion = DateTime.Now;
            encabezado.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            encabezado.estado = true;
            context.Entry(encabezado).State = EntityState.Modified;

            string Query = "Delete from mov_contable where id_encab =" + encabezado.idencabezado;
            context.Database.ExecuteSqlCommand(Query);

            int total = Convert.ToInt32(lista);
            for (int i = 1; i <= total; i++)
            {
                if (!string.IsNullOrEmpty(Request["cuenta_" + i]))
                {
                    mov_contable mov = new mov_contable
                    {
                        id_encab = encabezado.idencabezado,
                        seq = i,
                        idparametronombre = 19,
                        cuenta = Convert.ToInt32(Request["cuenta_" + i])
                    };
                    if (!string.IsNullOrEmpty(Request["centro_" + i]))
                    {
                        mov.centro = Convert.ToInt32(Request["centro_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["nit_" + i]))
                    {
                        mov.nit = Convert.ToInt32(Request["nit_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["nitAdmin_" + i]))
                    {
                        mov.idterceroadmin = Convert.ToInt32(Request["nitAdmin_" + i]);
                    }

                    mov.fec = encabezado.fecha;
                    if (!string.IsNullOrEmpty(Request["debito_" + i]))
                    {
                        string f = Request["debito_" + i];
                        mov.debito = Convert.ToDecimal(Request["debito_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["credito_" + i]))
                    {
                        mov.credito = Convert.ToDecimal(Request["credito_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["base_" + i]))
                    {
                        mov.basecontable = Convert.ToInt32(Request["debitoNiff_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["debitoNiff_" + i]))
                    {
                        mov.debitoniif = Convert.ToDecimal(Request["debitoNiff_" + i]);
                    }

                    if (!string.IsNullOrEmpty(Request["creditoNiff_" + i]))
                    {
                        mov.creditoniif = Convert.ToDecimal(Request["creditoNiff_" + i]);
                    }

                    mov.detalle = Request["detalle_" + i];
                    mov.fec_creacion = encabezado.fec_creacion;
                    mov.userid_creacion = encabezado.userid_creacion;
                    mov.estado = true;
                    context.mov_contable.Add(mov);
                }
            }

            context.SaveChanges();
            TempData["mensaje"] = "Registro editado correctamente";
            BuscarFavoritos(menu);
            return View(encabezado);
        }


        public JsonResult ValidarMesCerrado(DateTime fecha)
        {
            meses_cierre buscarMes = context.meses_cierre.FirstOrDefault(x => x.ano == fecha.Year && x.mes == fecha.Month);
            if (buscarMes != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarCentroFecha(int? centro, DateTime? fecha)
        {
            int data = (from c in context.centro_costo
                        where c.centcst_id == centro && fecha >= c.centcstfec_creacion
                        select c
                ).Count();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarConceptosPorDocumento(int tipo)
        {
            var concepto2 = (from t in context.tpdocconceptos2
                             where t.tipodocid == tipo
                             select new
                             {
                                 t.Descripcion,
                                 t.id
                             }).ToList();


            var concepto = (from t in context.tpdocconceptos
                            where t.tipodocid == tipo
                            select new
                            {
                                t.Descripcion,
                                t.id
                            }).ToList();

            var data = new
            {
                concepto2,
                concepto
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCuentao(int cuenta_id)
        {
            var data = (from c in context.cuenta_puc
                        where c.cntpuc_id == cuenta_id
                        select new
                        {
                            c.manejabase,
                            c.tercero,
                            c.ccostos,
                            c.documeto,
                            c.concepniff,
                            c.terceroadministrativo
                        }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosBrowser()
        {
            var data = (from e in context.encab_documento
                        join tp in context.tp_doc_registros
                            on e.tipo equals tp.tpdoc_id
                        join b in context.bodega_concesionario
                            on e.bodega equals b.id
                        join t in context.icb_terceros
                            on e.nit equals t.tercero_id
                        where tp.tipo == 19
                        select new
                        {
                            fecha = e.fecha.ToString(),
                            e.valor_total,
                            e.numero,
                            nit = t.prinom_tercero != null
                                ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                  t.apellido_tercero + " " + t.segapellido_tercero
                                : "(" + t.doc_tercero + ") " + t.razon_social,
                            bodega = b.bodccs_nombre,
                            id = e.idencabezado
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public int EliminarCuenta(int id)
        {
            mov_contable dato = context.mov_contable.Find(id);
            context.Entry(dato).State = EntityState.Deleted;
            int result = context.SaveChanges();

            return result;
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