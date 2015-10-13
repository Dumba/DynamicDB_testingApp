﻿using System;
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
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;

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
            TempData["message-success"] = "Table " + model.tableName + " was create successfuly.";

            return View(model);
        }

        public ActionResult DropTable(string appName, string tableName)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;
            DBTable table = DBTable.GetTable(tableName);
            table.Drop();
            DBTable.SaveChanges();

            TempData["message-success"] = "Table " + tableName + " was drop successfuly.";

            return RedirectToAction("Index", new { @appName = appName });

        }

        public ActionResult TruncateTable(string appName, string tableName)
        {
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable.ApplicationName = appName;

            DBTable table = DBTable.GetTable(tableName);
            table.Truncate();
            DBTable.SaveChanges();

            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
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
                            table.columns.ModifyInDB(c.Name, c.type, c.allowColumnLength, c.allowPrecisionScale, c.maxLength, c.precision, c.scale, c.canBeNull, c.isUnique, c.additionalOptions);
                            isEqual = true;
                            break;
                        }

                    }
                    if (isEqual == false)
                    {
                        table.columns.AddToDB(c.Name, c.type, c.allowColumnLength, c.allowPrecisionScale, c.maxLength, c.precision, c.scale, c.canBeNull, c.isUnique, c.additionalOptions);
                    }

                }

                DBTable.SaveChanges();
                TempData["message-success"] = "Table " + tableName + " was alter successfuly.";

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
        public ActionResult AddIndex(string appName, string tableName, FormCollection fc, List<string> indexColumns)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            table.indices.AddToDB(fc["indexName"], indexColumns);
            DBTable.SaveChanges();

            TempData["message-success"] = "Index " + fc["indexName"] + " was create successfuly.";

            return RedirectToAction("Index", new { @appName = appName });
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
            TempData["message-success"] = "Index " + indexName + " was drop successfuly.";
            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult CreateForeignKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            ViewBag.Columns = table.columns.Select(x => x.Name);

            return View(new DBForeignKey() { sourceTable = tableName });
        }
        [HttpPost]
        public ActionResult AddForeignKey(string appName, string tableName, DBForeignKey model)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.foreignKeys.AddToDB(model);

            DBTable.SaveChanges();
            TempData["message-success"] = "Foreign key " + model.name + " was create successfuly.";

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult DropForeignKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            return View(table);
        }

        public ActionResult DeleteForeignKey(string appName, string tableName, string foreignKeyName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.foreignKeys.DropFromDB(foreignKeyName);
            DBTable.SaveChanges();
            TempData["message-success"] = "Foreign key " + foreignKeyName + " was drop successfuly.";

            return RedirectToAction("Index", new { @appName = appName });
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
            TempData["message-success"] = "Primary key of table " + tableName + " was create successfuly.";

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult DropPrimaryKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            table.DropPrimaryKey();
            DBTable.SaveChanges();
            TempData["message-success"] = "Primary key of table " + tableName + " was drop successfuly.";

            return RedirectToAction("Index", new { @appName = appName });
        }
        [HttpPost]
        public ActionResult InsertRow(string appName, string tableName, FormCollection fc)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            DBItem row = new DBItem();
            foreach (DBColumn c in table.columns)
            {
                row[c.Name] = fc.Get("col" + c.Name);
            }

            table.Add(row);
            DBTable.SaveChanges();
            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        [HttpPost]
        public ActionResult DeleteOrUpdate(string appName, string tableName, FormCollection fs)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable table = DBTable.GetTable(tableName);
            DBItem row = new DBItem();

            foreach (DBColumn c in table.columns)
            {
                row[c.Name] = fs.Get("col" + c.Name);
                TempData.Remove(c.Name);
                TempData.Add(c.Name, row[c.Name]);
            }
            if (fs.Get("Update") != null)
            {
                ViewBag.Row = row.getAllProperties();
                return View("UpdateView", table);
            }
            else
            {
                table.Remove(row);
                DBTable.SaveChanges();

                return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
            }

        }

        [HttpPost]
        public ActionResult UpdateRow(string appName, string tableName, FormCollection fc)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);
            DBItem changes = new DBItem();
            DBItem oldVal = new DBItem();

            foreach (DBColumn c in table.columns)
            {
                changes[c.Name] = fc.Get("col" + c.Name);
                oldVal[c.Name] = TempData[c.Name];
            }
            table.Update(changes, oldVal);
            DBTable.SaveChanges();

            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult Constraint(string appName, string tableName, bool isDisable)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);
            ViewBag.Constraints = table.getConstraints();

            if (isDisable == true)
            {
                 return View("DisableConstraint", table);
            }
            else if(isDisable==false)
            {
                return View("EnableConstraint", table);
            }

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult DisableOrEnableConstraint(string appName, string tableName, FormCollection fc, bool isDisable)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            string constraintName = (fc["all"] != null) ? "ALL" : fc["constraintName"];
            if (isDisable == true)
            {
                table.DisableConstraint(constraintName);
            }
            else if (isDisable==false)
            {
                table.EnableConstraint(constraintName);
            }

            DBTable.SaveChanges();

            return RedirectToAction("Index", new{@appName=appName});
        }

        //public ActionResult EnableConstraint(string appName, string tableName)
        //{
        //    DBTable.ApplicationName = appName;
        //    DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
        //    DBTable table = DBTable.GetTable(tableName);

        //    ViewBag.Constraints = table.getConstraints();
        //    return View(table);
        //}

        //public ActionResult EnableCon(string appName, string tableName, FormCollection fc)
        //{
        //    DBTable.ApplicationName = appName;
        //    DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
        //    DBTable table = DBTable.GetTable(tableName);

        //    string constraintName = (fc["all"] != null) ? "ALL" : fc["constraintName"];
        //    table.EnableConstraint(constraintName);
        //    DBTable.SaveChanges();

        //    return RedirectToAction("Index", new { @appName = appName });
        //}

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