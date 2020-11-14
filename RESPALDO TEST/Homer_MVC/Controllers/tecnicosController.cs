using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tecnicosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tecnicos
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int id_user = id ?? 0;
            ttecnicos tecnico = context.ttecnicos.FirstOrDefault(x => x.idusuario == id_user);
            if (tecnico == null)
            {
                users buscarUsuario = context.users.FirstOrDefault(x => x.user_id == id_user);
                if (buscarUsuario != null)
                {
                    tecnico = new ttecnicos { idusuario = id_user };
                }
                else
                {
                    return HttpNotFound();
                }
            }

            icb_sysparameter parametroTecnicos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P48");
            int idTecnicos = parametroTecnicos != null ? Convert.ToInt32(parametroTecnicos.syspar_value) : 0;
            var buscarUsuarios = (from usuarios in context.users
                                  where usuarios.rol_id == idTecnicos
                                  select new
                                  {
                                      usuarios.user_id,
                                      nombres = usuarios.user_nombre + " " + usuarios.user_apellido
                                  }).ToList();
            ViewBag.idusuario = new SelectList(buscarUsuarios, "user_id", "nombres", tecnico.idusuario);


            var tipoTecnico = (from t in context.ttipotecnico
                               select new
                               {
                                   t.id,
                                   tipo = t.tipo + "-" + t.Especializacion,

                               }).ToList();

            ViewBag.tipo_tecnico = new SelectList(tipoTecnico, "id", "tipo", tecnico.tipo_tecnico);


            ConsultaDatosCreacion(tecnico);
            BuscarFavoritos(menu);
            return View(tecnico);
        }

        [HttpPost]
        public ActionResult Edit(ttecnicos modelo, int? menu)
        {
            TimeSpan? horaInicia = modelo.iniciodescanso;
            //var minutoInicia = modelo.horaIniciaDescanso.Minutes;
            //var segundoInicia = modelo.horaIniciaDescanso.Seconds;

            //var horaTermina = modelo.horaFinDescanso.Hours;
            //var minutoTermina = modelo.horaFinDescanso.Minutes;
            //var segundoTermina = modelo.horaFinDescanso.Seconds;
            if (ModelState.IsValid)
            {
                ttecnicos buscarSiYaExiste = context.ttecnicos.FirstOrDefault(x => x.idusuario == modelo.idusuario);
                if (buscarSiYaExiste != null)
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarSiYaExiste.fec_actualizacion = DateTime.Now;
                    buscarSiYaExiste.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarSiYaExiste.contratista = modelo.contratista;

                    if (modelo.iniciodescanso == null)
                    {
                        buscarSiYaExiste.iniciodescanso = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        buscarSiYaExiste.iniciodescanso = modelo.iniciodescanso;
                    }

                    if (modelo.findescanso == null)
                    {
                        buscarSiYaExiste.findescanso = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        buscarSiYaExiste.findescanso = modelo.findescanso;
                    }
                    buscarSiYaExiste.claveSeguridad = modelo.claveSeguridad;
                    buscarSiYaExiste.estado = modelo.estado;
                    buscarSiYaExiste.condicion_adicional = modelo.condicion_adicional;
                    buscarSiYaExiste.valorhora = modelo.valorhora;
                    buscarSiYaExiste.porcenhora = modelo.porcenhora;
                    buscarSiYaExiste.razon_inactivo = modelo.razon_inactivo;
                    buscarSiYaExiste.valor_adicional = modelo.valor_adicional;
                    buscarSiYaExiste.tipo_tecnico = modelo.tipo_tecnico;
                    buscarSiYaExiste.otros_casos = modelo.otros_casos;

                    context.Entry(buscarSiYaExiste).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del tecnico fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
                else
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.ttecnicos.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del tecnico fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            icb_sysparameter parametroTecnicos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P48");
            int idTecnicos = parametroTecnicos != null ? Convert.ToInt32(parametroTecnicos.syspar_value) : 0;
            var buscarUsuarios = (from usuarios in context.users
                                  where usuarios.rol_id == idTecnicos
                                  select new
                                  {
                                      usuarios.user_id,
                                      nombres = usuarios.user_nombre + " " + usuarios.user_apellido
                                  }).ToList();
            ViewBag.idusuario = new SelectList(buscarUsuarios, "user_id", "nombres", modelo.idusuario);

            var tipoTecnico=(from t in context.ttipotecnico
                             select new {
                                t.id,
                                tipo=t.tipo + "-" + t.Especializacion,

                             }).ToList();

            ViewBag.tipo_tecnico = new SelectList(tipoTecnico, "id", "tipo", modelo.tipo_tecnico);

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void ConsultaDatosCreacion(ttecnicos tecnico)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tecnico.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tecnico.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarTecnicos()
        {
            icb_sysparameter parametroTecnicos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P48");
            int idTecnicos = parametroTecnicos != null ? Convert.ToInt32(parametroTecnicos.syspar_value) : 0;
            var buscarTecnicos = (from usuarios in context.users
                                  join tecnicos in context.ttecnicos
                                      on usuarios.user_id equals tecnicos.idusuario into tec
                                  from tecnicos in tec.DefaultIfEmpty()
                                  join tipoTecnico in context.ttipotecnico
                                      on tecnicos.tipo_tecnico equals tipoTecnico.id into tipo
                                  from tipoTecnico in tipo.DefaultIfEmpty()
                                  where usuarios.rol_id == idTecnicos
                                  select new
                                  {
                                      usuarios.user_id,
                                      usuarios.user_nombre,
                                      usuarios.user_apellido,
                                      usuarios.user_numIdent,
                                      tipo = tipoTecnico.tipo != null ? tipoTecnico.tipo : "",
                                      Especializacion = tipoTecnico.Especializacion != null ? tipoTecnico.Especializacion : "",
                                      valorHr = tipoTecnico.valorHr != null ? tipoTecnico.valorHr : 0,
                                      porcentaje = tipoTecnico.porcentaje != null ? tipoTecnico.porcentaje : 0,
                                      contratista = tecnicos.contratista ? "Si" : "No",
                                      valorhora = tecnicos.valorhora != null ? tecnicos.valorhora.ToString() : "",
                                      porcenhora = tecnicos.porcenhora != null ? tecnicos.porcenhora.ToString() : "",
                                      estado = tecnicos.estado==true ? "Activo" : "Inactivo",
                                      valorAdicional = tecnicos.valor_adicional != null ?tecnicos.valor_adicional.ToString() : "",
                                  }).ToList();

            return Json(buscarTecnicos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult agregarCondicion(int? id, string valor_adicional) {

            if (id != null && !string.IsNullOrWhiteSpace(valor_adicional))
            {
                int data = 0;

                var buscar = context.ttecnicos.Where(d => d.idusuario == id).Select(d => d.id).FirstOrDefault();

                var t = context.ttecnicos.Find(buscar);
                t.valor_adicional = Convert.ToDecimal(valor_adicional);
                context.Entry(t).State = EntityState.Modified;

                int resultado = context.SaveChanges();

                if (resultado > 0)
                {
                    data = 1;
                }
                else
                {
                    data = 0;
                }

                return Json(data, JsonRequestBehavior.AllowGet);

            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerDatos(int? tipo) {

            if (tipo!= null)
            {
                var tipoTecnico = (from t in context.ttipotecnico
                                   where t.id == tipo
                                   select new
                                   {
                                       t.id,
                                       t.valorHr,
                                       t.porcentaje,

                                   }).ToList();

                return Json(tipoTecnico, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserTecnicos(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        //public JsonResult BuscarDatos()
        //{
        //	var data = (from ot in context.tencabezaorden
        //				join b in context.bodega_concesionario
        //				on ot.bodega equals b.id
        //				join t in context.icb_terceros
        //				on ot.tercero equals t.tercero_id
        //				join v in context.icb_vehiculo
        //				on ot.placa equals v.plan_mayor
        //				join c in context.color_vehiculo
        //				on v.colvh_id equals c.colvh_id
        //				join u in context.ubicacion_bodega
        //				on v.ubicacionactual equals u.id into tmp
        //				from u in tmp.DefaultIfEmpty()
        //				join m in context.modelo_vehiculo
        //				on v.modvh_id equals m.modvh_codigo
        //				join a in context.users
        //				on ot.asesor equals a.user_id
        //				join sp in context.icb_sysparameter
        //				on )

        //	return Json(JsonRequestBehavior.AllowGet);
        //}

        public ActionResult DescargarExcel(int? bodega, int? tecnico, string fechaini, string fechafin)
        {

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            Expression<Func<vw_browser_Liquidacion_MO, bool>> predicado = PredicateBuilder.True<vw_browser_Liquidacion_MO>();

            if (bodega != null)
            {
                predicado = predicado.And(d => d.bodega == bodega);
            }

            if (tecnico != null)
            {

                predicado = predicado.And(d => d.tecnico == tecnico);

            }

            if (!string.IsNullOrWhiteSpace(fechaini))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(fechaini, out fecha);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fec_creacion >= fecha);
                }
            }

            if (!string.IsNullOrWhiteSpace(fechafin))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(fechafin, out fecha);
                if (convertir == true)
                {
                    fecha = fecha.AddDays(1);
                    predicado = predicado.And(d => d.fec_creacion <= fecha);
                }
            }


            List<vw_browser_Liquidacion_MO> query2 = context.vw_browser_Liquidacion_MO.Where(predicado).ToList();

            var query = query2.Select(d => new
            {
                d.placa,
                ot = d.codigoentrada,
                fecha = d.fec_creacion.ToString("yyyy/MM/dd"),
                d.codigo,
                d.descripcion,
                operacion = d.idtempario,
                d.horasClientes,
                d.horasMC,
                d.horasSR,
                d.horasLYP,
                totalHoraCliente=query2.Sum(x=> x.horasClientes),
                totalHoraMC = query2.Sum(x => x.horasMC),
                totalHoraSR = query2.Sum(x => x.horasSR),
                totalHoraLYP = query2.Sum(x => x.horasLYP),
                totalGeneral = query2.Sum(x => x.horasClientes) + query2.Sum(x => x.horasMC) + query2.Sum(x => x.horasSR) + query2.Sum(x => x.horasLYP),

            }).ToList();

            var queryarreglo = query.Select(d => new
            {
                d.placa,
                d.ot,
                d.fecha,
                d.codigo,
                d.descripcion,
                d.operacion,
                d.horasClientes,
                d.horasMC,
                d.horasSR,
                d.horasLYP,
                d.totalHoraCliente,
                d.totalHoraMC,
                d.totalHoraSR,
                d.totalHoraLYP,
                d.totalGeneral,
            }).ToArray();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].Value = "Liquidacion de Mano de Obra de los Tecnicos";

            workSheet.Cells[3, 1].Value = "Placa / Plan";
            workSheet.Cells[3, 2].Value = "OT";
            workSheet.Cells[3, 3].Value = "Fecha Liquidacion";
            workSheet.Cells[3, 4].Value = "Codigo Operario";
            workSheet.Cells[3, 5].Value = "Tarifa";
            workSheet.Cells[3, 6].Value = "Operacion";
            workSheet.Cells[3, 7].Value = " Horas Cliente";
            workSheet.Cells[3, 8].Value = " Horas MC";
            workSheet.Cells[3, 9].Value = " Horas SR";
            workSheet.Cells[3, 10].Value = " Horas LyP";
            workSheet.Cells[3, 11].Value = "Total Horas Cliente";
            workSheet.Cells[3, 12].Value = "Total  Horas MC";
            workSheet.Cells[3, 13].Value = "Total Horas SR";
            workSheet.Cells[3, 14].Value = "Total Horas LYP";
            workSheet.Cells[3, 15].Value = "Total General";



            workSheet.Cells[4, 1].LoadFromCollection(queryarreglo, false);

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=LiquidacionMO.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public JsonResult BuscarLiquidacionMO() {

            var buscar = context.vw_browser_Liquidacion_MO.ToList();

            var data = buscar.Select(x => new {

                x.id,
                x.placa,
                ot = x.codigoentrada,
                fecha = x.fec_creacion.ToString("yyyy/MM/dd"),
                x.codigo,
                x.descripcion,
                operacion = x.idtempario,
                x.horasClientes,
                x.horasMC,
                x.horasSR,
                x.horasLYP,

            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult BuscarLiquidacionTecnicoMO(int? bodega, int? tecnico, string fechaini, string fechafin)
        {

            Expression<Func<vw_browser_Liquidacion_MO, bool>> predicado = PredicateBuilder.True<vw_browser_Liquidacion_MO>();

            if (bodega != null)
            {
                predicado = predicado.And(d => d.bodega == bodega);
            }

            if (tecnico != null)
            {

                predicado = predicado.And(d => d.tecnico == tecnico);

            }
            if (!string.IsNullOrWhiteSpace(fechaini))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(fechaini, out fecha);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fec_creacion >= fecha);
                }
            }
            if (!string.IsNullOrWhiteSpace(fechafin))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(fechafin, out fecha);
                if (convertir == true)
                {
                    fecha = fecha.AddDays(1);
                    predicado = predicado.And(d => d.fec_creacion <= fecha);
                }
            }

            List<vw_browser_Liquidacion_MO> lista = new List<vw_browser_Liquidacion_MO>();
            lista = context.vw_browser_Liquidacion_MO.Where(predicado).ToList();


            var data = lista.Select(x => new {

                x.id,
                x.placa,
                ot=x.codigoentrada,
                fecha = x.fec_creacion.ToString("yyyy/MM/dd"),
                x.codigo,
                x.descripcion,
                operacion= x.idtempario,
                x.horasClientes,
                x.horasMC,
                x.horasSR,
                x.horasLYP,
            }).ToList();

            var data2 = data.Select(x=> new {
                totalHoraCliente = data.Sum(d => d.horasClientes),
                totalHoraMC = data.Sum(d => d.horasMC),
                totalHoraSR = data.Sum(d => d.horasSR),
                totalHoraLYP = data.Sum(d => d.horasLYP),
                totalGeneral = data.Sum(d => d.horasClientes) + data.Sum(d => d.horasMC) + data.Sum(d => d.horasSR) + data.Sum(d => d.horasLYP),

            }).Distinct().ToList();

            return Json( new { data, data2 }, JsonRequestBehavior.AllowGet);

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

        public ActionResult LiquidacionTecnicosMO() {

            var list = (from bod in context.bodega_concesionario
                        select new
                        {
                            id = bod.id,
                            bodega = bod.bodccs_nombre,
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.bodega,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodccs_cod = lista;

            var list2 = (from t in context.ttecnicos
                        select new
                        {
                            id = t.id,
                            tecnico = t.users.user_nombre+" "+ t.users.user_apellido,
                        }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in list2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.tecnico,
                    Value = item.id.ToString()
                });
            }
            ViewBag.tecnico = lista2;

            return View();

        }

    }
}