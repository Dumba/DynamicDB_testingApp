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
            return View(table);
        }

        public ActionResult Create(string appName)
        {
            ViewBag.appName = appName;

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

            ViewBag.appName = appName;

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

            ViewBag.appName = appName;
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
            ViewBag.appName = appName;

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
            ViewBag.appName = appName;
            return View(table);
        }
         [HttpPost]
        public ActionResult AddForeignKey(string appName, DBTable model , FormCollection fc)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;

            DBTable.AddForeignKey(model.tableName, fc.Get("tableAColumns"), fc.Get("tableB"), fc.Get("tableBColumns"));
            DBTable.SaveChanges();

            return RedirectToAction("Index", new {@appName = appName});
        }

        public ActionResult CreatePrimaryKey(string appName, string tableName)
        {
            DBTable.ApplicationName = appName;
            DBTable.connectionString = (new Entities()).Database.Connection.ConnectionString;
            DBTable table = DBTable.GetTable(tableName);

            ViewBag.appName = appName;
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