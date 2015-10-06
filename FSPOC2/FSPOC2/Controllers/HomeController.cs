using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FSPOC2.Models;
using Entitron.Sql;

namespace FSPOC2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Entities e = new Entities();
            List<Application> apps = e.Applications.ToList();

            return View(apps);
        }

        public ActionResult Create()
        {
            return View(new Application()
            {
                DbTablePrefix = "Dynamic_",
                DbMetaTables = "Dynamic__Metadata_tables"
            });
        }
        [HttpPost]
        public ActionResult Create(Application model)
        {
            if (ModelState.IsValid)
            {
                Entities e = new Entities();

                SqlQuery_Simple_Table_Create sql = new SqlQuery_Simple_Table_Create(model.DbMetaTables);
                sql.AddColumn("Id", "INT", false, canBeNull: false, additionalOptions: "IDENTITY")
                    .AddColumn("Name", "NVARCHAR", true, maxLength: 50, canBeNull: false, additionalOptions: "UNIQUE")
                    .AddColumn("tableId", "INT", false, canBeNull: true, additionalOptions: "UNIQUE")
                    .AddParameters("CONSTRAINT [PK_" + model.DbMetaTables + "] PRIMARY KEY ([Id])");
                sql.Execute(e.Database.Connection.ConnectionString);

                e.Applications.Add(model);
                e.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}