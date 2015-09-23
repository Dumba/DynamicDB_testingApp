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
        public ActionResult Details(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            ViewBag.appName = appName;
            DBTable table = DBTable.GetTable(tableName);

            return View(table);
        }
        public ActionResult Data(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
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
               
                foreach (DBColumn c in model.columns)
                {
                    table.columns.Add(c.Name, c.type, c.maxLength, c.canBeNull);
                }
                
                DBTable.SaveChanges();

                return RedirectToAction("Index", new { @appName = appName });
            }

            return View();
        }

        public ActionResult DropTable(string appName, string tableName)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;

            DBTable table = DBTable.GetTable(tableName);
            table.Drop();
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});

        }

        public ActionResult AddColumn(string appName, string tableName)
        {
            return View(new DBColumn());
        }
        [HttpPost]
        public ActionResult AddColumn(string appName, string tableName, DBColumn column)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;
            DBTable.GetTable(tableName)
                .columns.Add(column);
            DBTable.SaveChanges();
            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        [HttpPost]
        public ActionResult AlterTable(string appName, string tableName, DBTable model)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
                DBTable.ApplicationName = appName;
                ViewBag.appName = appName;
                DBTable table = DBTable.GetTable(tableName);
                bool isEqual;
                foreach (DBColumn c in model.columns)
                {
                    isEqual = false;
                    foreach (DBColumn d in table.columns)
                    {
                        if (c.Name == d.Name)
                        {
                            table.columns.Modify(c.Name, c.type, c.maxLength, c.canBeNull);
                            isEqual = true;
                        }

                    }
                    if (isEqual == false)
                    {
                        table.columns.Add(c.Name, c.type, c.maxLength, c.canBeNull);
                    }

                }

                DBTable.SaveChanges();

                return RedirectToAction("Index", new {@appName = appName});
            }

            return View();

        }

        public ActionResult DropColumn(string appName, string tableName, string columnName)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;

            DBTable.GetTable(tableName)
                .columns.Drop(columnName);
            DBTable.SaveChanges();
            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult CreateIndex(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            ViewBag.Columns = table.columns.colums.Select(x=>x.Name);
            ViewBag.TableName = table.tableName;
            return View(table);
        }

        public ActionResult AddIndex(string appName, string tableName,FormCollection fc, List<string> indexColumns)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            table.createIndex(fc["indexName"], indexColumns);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName=appName});
        }

        public ActionResult DropIndex(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);
            ViewBag.Indexes = table.getIndexNames(tableName);
            return View(table);
        }

        public ActionResult DeleteIndex(string appName, string tableName, string indexName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.dropIndex(indexName);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }
    }
}