using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FSPOC2.Models;
using Entitron;

namespace FSPOC2.Controllers
{
    public class DBManagerController : Controller
    {
        public ActionResult Index(string appName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            List<DBTable> tables = DBTable.GetAll();

            return View(tables);
        }
        public ActionResult Details(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            return View(table);
        }
        public ActionResult Data(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            return View(table);
        }

        public ActionResult Create(string appName)
        {

            return View();
        }
        [HttpPost]
        public ActionResult Create(string appName, DBTable model)
        {
            if (!string.IsNullOrWhiteSpace(model.tableName))
            {
                DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

                model.AppName = appName;
                model.Create();
                
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
                .columns.AddToDB(column);
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
                DBTable table = DBTable.GetTable(tableName);
                bool isEqual;
                foreach (DBColumn c in model.columns)
                {
                    isEqual = false;
                    foreach (DBColumn d in table.columns)
                    {
                        if (c.Name == d.Name)
                        {
                            table.columns.ModifyInDB(c.Name, c.type, c.allowColumnLength, c.maxLength, c.canBeNull, c.isPrimaryKey, c.isUnique, c.additionalOptions);
                            isEqual = true;
                            break;
                        }

                    }
                    if (isEqual == false)
                    {
                        table.columns.AddToDB(c.Name, c.type, c.allowColumnLength, c.maxLength, c.canBeNull, c.isPrimaryKey, c.isUnique, c.additionalOptions);
                    }

                }

                DBTable.SaveChanges();

                return RedirectToAction("Index", new { @appName = appName });
            }

            return View();

        }

        public ActionResult DropColumn(string appName, string tableName, string columnName)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;

            DBTable.GetTable(tableName)
                .columns.DropFromDB(columnName);
            DBTable.SaveChanges();
            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult CreateIndex(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            return View(table);
        }
         [HttpPost]
        public ActionResult AddIndex(string appName, string tableName,FormCollection fc, List<string> indexColumns)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            table.indices.AddToDB(fc["indexName"], indexColumns);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName=appName});
        }

        public ActionResult DropIndex(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);
            return View(table);
        }
         [HttpPost]
        public ActionResult DeleteIndex(string appName, string tableName, string indexName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.indices.DropFromDB(indexName);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult CreateForeignKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);
            
            ViewBag.Tables = DBTable.GetAll().Select(t=>t.tableName).ToList();
            ViewBag.Columns = table.columns.Select(x => x.Name);
            return View(table);
        }
         [HttpPost]
        public ActionResult AddForeignKey(string appName, string tableName , FormCollection fc)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.AddForeignKey(fc.Get("foreignName"), tableName, fc.Get("TableAColumns"), fc.Get("TableB"), fc.Get("TableBColumns"), fc.Get("deleteAction"), fc.Get("updateAction"));
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult DropForeignKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            ViewBag.ForeignKeys = table.GetForeignKeys();
            return View(table);
        }

        public ActionResult DeleteForeignKey(string appName, string tableName, string foreignKeyName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.DropForeignKey(foreignKeyName);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult CreatePrimaryKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            ViewBag.Columns = table.columns.Select(x => x.Name);
            return View(table);
        }

        [HttpPost]
        public ActionResult AddPrimaryKey(string appName, string tableName, List<string> primaryKeys)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.AddPrimaryKey(primaryKeys);
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult DropPrimaryKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.DropPrimaryKey();
            DBTable.SaveChanges();
            return RedirectToAction("Index",new{@appName=appName});
        }

        [HttpPost]
        public ActionResult InsertRow(string appName, string tableName, FormCollection fc)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            Dictionary<DBColumn, object> val=new Dictionary<DBColumn, object>();
            
            foreach (DBColumn c in table.columns)
            {
                val.Add(c, fc.Get("valueOf" + c.Name));
            }

            table.Insert(val);
            DBTable.SaveChanges();
            return RedirectToAction("Data", new {@appName = appName, @tableName = tableName});
        }

        public ActionResult UpdateRow(string appName, string tableName, FormCollection fc, int rowNumber)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            Dictionary<DBColumn, object> values = new Dictionary<DBColumn, object>();

            foreach (DBColumn c in table.columns)
            {
                values.Add(c, fc.Get("valueOf" + c.Name));
            }
            table.Update(values, rowNumber);
            DBTable.SaveChanges();

            return RedirectToAction("Data", new {@appName = appName, @tableName = tableName});
        }
        [HttpPost]
        public ActionResult DeleteRow(string appName, string tableName, FormCollection fs )
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            Dictionary<DBColumn,object> values=new Dictionary<DBColumn, object>();
            foreach (DBColumn c in table.columns)
            {
                values.Add(c, fs["col" + c.Name]);
            }
            table.Delete(values);
            DBTable.SaveChanges();

            return RedirectToAction("Data", new {@appName = appName, @tableName = tableName});
        }

        public JsonResult getTableColumns(string tableName, string appName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            List<string> tableColumns = table.columns.Select(x => x.Name).ToList();

            return Json(tableColumns, JsonRequestBehavior.AllowGet);
        }
    }
}