using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FSPOC2.Models;
using Entitron;

namespace FSPOC2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DBApp.connectionString = (new Entities()).Database.Connection.ConnectionString;

            return View(DBApp.GetAll());
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(DBApp model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return View(model);

            DBApp.connectionString = (new Entities()).Database.Connection.ConnectionString;
            if (string.IsNullOrWhiteSpace(model.DisplayName))
                model.DisplayName = model.Name;
            
            model.Create();

            return RedirectToAction("Index");
        }
    }
}