using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FSPOC2.Models;
using DynamicDB;

namespace FSPOC2.Controllers
{
    public class DBManagerController : Controller
    {
        public ActionResult Index(string appName)
        {
            ViewBag.appName = appName;
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            List<DBTable> tables = DBTable.GetAll();

            return View(tables);
        }
        public ActionResult Details(string appName, string id)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(id);

            return View(table);
        }
        public ActionResult Data(string appName, string id)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(id);
            ViewBag.TableName = table.tableName;
            return View(table);
        }

        public ActionResult Create(string appName)
        {
            ViewBag.appName = appName;

            return View();
        }
        [HttpPost]
        public ActionResult Create(string appName, string tableName, DBTable model)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
                DBTable.ApplicationName = appName;

                DBTable table = DBTable.Create(tableName);
               
                foreach (DBColumn c in model._columns)
                {
                    table.AddColumn(c.Name, c.type, c.maxLength, c.canBeNull);
                }
                
                DBTable.SaveChanges();

                return RedirectToAction("Index", new { @appName = appName });
            }

            return View();
        }

        //public ActionResult AddColumn(int id)
        //{
        //    return View(new DBItems_Metadata_columns() { ItemId = id });
        //}
        //[HttpPost]
        //public ActionResult AddColumn(int id, DBItems_Metadata_columns column, int type)
        //{
        //    Entities e = new Entities();
        //    column.ItemId = id;
        //    column.DBItems_Metadata_tables = e.DBItems_Metadata_tables.SingleOrDefault(i => i.Id == id);
        //    column.DataTypeId = type;

        //    if (ModelState.IsValid && column.DBItems_Metadata_tables != null)
        //    {
        //        e.DBItems_Metadata_columns.Add(column);
        //        DBTable.GetTable(column.DBItems_Metadata_tables.Name)
        //            .AddColumn(column.Name);
        //        e.SaveChanges();

        //        return RedirectToAction("Details", new { @id = id });
        //    }

        //    return View(column);
        //}
    }
}