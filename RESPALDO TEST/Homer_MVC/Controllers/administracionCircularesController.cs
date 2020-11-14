using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class administracionCircularesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: administracionCirculares
        public ActionResult Index()
        {
            ViewBag.TipoCircular = new SelectList(context.tipocargaarchivo, "id", "tipodocumento");
            ViewBag.roles = context.rols.ToList();
            ViewBag.circulares = context.cargacirculares.Where(d=> d.vencido==false).ToList();
            return View();
        }


        public ActionResult Visualizar()
        {
            int rolActual = Convert.ToInt32(Session["user_rolid"]);
            System.Collections.Generic.List<int> buscarCircularPorRol =
                context.circulares_rol.Where(x => x.rol == rolActual).Select(x => x.idcircular).ToList();
            ViewBag.circulares = context.cargacirculares.Where(x => buscarCircularPorRol.Contains(x.id) && x.vencido==false).ToList();
            return View();
        }


        public ActionResult VisualizarArchivoAdmin(int id)
        {
            cargacirculares buscarArchivo = context.cargacirculares.FirstOrDefault(x => x.id == id);
            if (buscarArchivo != null)
            {
                string path = Server.MapPath("~/Pdf/" + buscarArchivo.nombre);
                if (System.IO.File.Exists(path))
                {
                    return File(path, "application/pdf");
                }

                TempData["mensaje_error"] = "El archivo que desea visualizar se ha eliminado";
                ViewBag.TipoCircular = new SelectList(context.tipocargaarchivo, "id", "tipodocumento");
                ViewBag.roles = context.rols.ToList();
                ViewBag.circulares = context.cargacirculares.ToList();
                return RedirectToAction("Index");
            }

            TempData["mensaje_error"] = "El archivo que desea visualizar se ha eliminado";
            ViewBag.TipoCircular = new SelectList(context.tipocargaarchivo, "id", "tipodocumento");
            ViewBag.roles = context.rols.ToList();
            ViewBag.circulares = context.cargacirculares.ToList();
            return RedirectToAction("Index");
        }


        public ActionResult VisualizarArchivoPorRol(int id)
        {
            cargacirculares buscarArchivo = context.cargacirculares.FirstOrDefault(x => x.id == id);
            if (buscarArchivo != null)
            {
                string path = Server.MapPath("~/Pdf/" + buscarArchivo.nombre);
                if (System.IO.File.Exists(path))
                {
                    return File(path, "application/pdf");
                }

                TempData["mensaje_error"] = "El archivo que desea visualizar se ha eliminado";
                return RedirectToAction("Visualizar");
            }

            TempData["mensaje_error"] = "El archivo que desea visualizar se ha eliminado";
            return RedirectToAction("Visualizar");
        }


        public ActionResult EliminarArchivo(int id)
        {
            cargacirculares buscarArchivo = context.cargacirculares.FirstOrDefault(x => x.id == id);
            if (buscarArchivo != null)
            {
                string path = Server.MapPath("~/Pdf/" + buscarArchivo.nombre);
                if (System.IO.File.Exists(path))
                {
                    const string query = "DELETE FROM [dbo].[circulares_rol] WHERE [idcircular]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, buscarArchivo.id);
                    context.Entry(buscarArchivo).State = EntityState.Deleted;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        System.IO.File.Delete(path);
                        TempData["mensaje"] = "La nueva circular se ha eliminado exitosamente";
                        return RedirectToAction("Index");
                    }

                    TempData["mensaje_error"] = "Error de base de datos, por favor verifique...!";
                    return RedirectToAction("Index");
                }

                TempData["mensaje_error"] = "El archivo fue eliminado del servidor...!";
                return RedirectToAction("Index");
            }

            return HttpNotFound();
        }


        [HttpPost]
        public ActionResult GuardarCircular(HttpPostedFileBase file, int? menu)
        {
            string id_tipoCircular = Request["selectTipoCircular"];
            string ids_roles = Request["selectRoles"];

            if (file.FileName.EndsWith("pdf"))
            {
                string path = Server.MapPath("~/Pdf/" + file.FileName);
                if (System.IO.File.Exists(path))
                {
                    TempData["mensaje_error"] =
                        "Un archivo con el mismo nombre ya fue subido, cambie el nombre y vuelva a intentarlo!";
                    //System.IO.File.Delete(path);
                }
                else
                {
                    file.SaveAs(path);
                    context.cargacirculares.Add(new cargacirculares
                    {
                        tipoarchivo = Convert.ToInt32(id_tipoCircular),
                        nombre = file.FileName,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        int buscarUltimaCircular = context.cargacirculares.OrderByDescending(x => x.id)
                            .Select(x => x.id).FirstOrDefault();
                        string[] rolId = ids_roles.Split(',');
                        foreach (string substring in rolId)
                        {
                            context.circulares_rol.Add(new circulares_rol
                            {
                                rol = Convert.ToInt32(substring),
                                idcircular = buscarUltimaCircular
                            });
                        }

                        int guardarBodegas = context.SaveChanges();
                        if (guardarBodegas > 0)
                        {
                            TempData["mensaje"] = "La nueva circular se ha agregado exitosamente";
                            return RedirectToAction("Index");
                        }

                        TempData["mensaje_error"] = "Error de base de datos, por favor verifique...!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de base de datos, por favor verifique...!";
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "El archivo subido no es tipo pdf, por favor verifique...!";
            }

            return RedirectToAction("Index");
        }
    }
}