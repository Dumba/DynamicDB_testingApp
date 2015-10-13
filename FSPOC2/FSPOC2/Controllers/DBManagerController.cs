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
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            return View(app.GetTables());
        }
        public ActionResult Details(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            
            return View(app.GetTable(tableName));
        }
        public ActionResult Data(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            return View(app.GetTable(tableName));
        }

        public ActionResult Create(string appName)
        {
            return View(new DBTable() { Application = new DBApp() { Name = appName } });
        }
        [HttpPost]
        public ActionResult Create(string appName, DBTable model)
        {
            if (!string.IsNullOrWhiteSpace(model.tableName))
            {
                DBApp app = new DBApp()
                {
                    Name = appName,
                    ConnectionString = (new Entities()).Database.Connection.ConnectionString
                };

                model.Application = app;
                model.Create();

                app.SaveChanges();

                return RedirectToAction("Index", new { @appName = appName });
            }

            return View(model);
        }

        public ActionResult DropTable(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            app.GetTable(tableName).Drop();
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });

        }

        public ActionResult TruncateTable(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
            table.Truncate();
            app.SaveChanges();

            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult AddColumn(string appName, string tableName)
        {
            return View(new DBColumn());
        }
        [HttpPost]
        public ActionResult AddColumn(string appName, string tableName, DBColumn column)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            app.GetTable(tableName)
                .columns.AddToDB(column);
            app.SaveChanges();

            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        [HttpPost]
        public ActionResult AlterTable(string appName, string tableName, DBTable model)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                DBApp app = new DBApp()
                {
                    Name = appName,
                    ConnectionString = (new Entities()).Database.Connection.ConnectionString
                };

                DBTable table = app.GetTable(tableName);
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

                app.SaveChanges();

                return RedirectToAction("Index", new { @appName = appName });
            }

            return View();

        }

        public ActionResult DropColumn(string appName, string tableName, string columnName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            app.GetTable(tableName)
                .columns.DropFromDB(columnName);
            app.SaveChanges();

            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult CreateIndex(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            
            return View(app.GetTable(tableName));
        }
        [HttpPost]
        public ActionResult AddIndex(string appName, string tableName, FormCollection fc, List<string> indexColumns)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
            table.indices.AddToDB(fc["indexName"], indexColumns);
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult DropIndex(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
            return View(table);
        }
        [HttpPost]
        public ActionResult DeleteIndex(string appName, string tableName, string indexName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);

            table.indices.DropFromDB(indexName);
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult CreateForeignKey(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);

            ViewBag.Columns = table.columns.Select(x => x.Name);

            return View(new DBForeignKey() { sourceTable = table });
        }
        [HttpPost]
        public ActionResult AddForeignKey(string appName, string tableName, DBForeignKey model)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);

            table.foreignKeys.AddToDB(model);

            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult DropForeignKey(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            return View(table);
        }

        public ActionResult DeleteForeignKey(string appName, string tableName, string foreignKeyName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            table.foreignKeys.DropFromDB(foreignKeyName);
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult CreatePrimaryKey(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            ViewBag.Columns = table.columns.Select(x => x.Name);
            return View(table);
        }

        [HttpPost]
        public ActionResult AddPrimaryKey(string appName, string tableName, List<string> primaryKeys)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            table.AddPrimaryKey(primaryKeys);
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult DropPrimaryKey(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            table.DropPrimaryKey();
            app.SaveChanges();
            return RedirectToAction("Index", new { @appName = appName });
        }
        [HttpPost]
        public ActionResult InsertRow(string appName, string tableName, FormCollection fc)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
            DBItem row = new DBItem();
            foreach (DBColumn c in table.columns)
            {
                if (c.type.ToLower() == "int")
                    row[c.Name] = Convert.ToInt32(fc.Get("col" + c.Name));
                else
                    row[c.Name] = fc.Get("col" + c.Name);
            }

            table.Add(row);
            app.SaveChanges();
            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        [HttpPost]
        public ActionResult DeleteOrUpdate(string appName, string tableName, FormCollection fs)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
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
                app.SaveChanges();

                return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
            }

        }

        [HttpPost]
        public ActionResult UpdateRow(string appName, string tableName, FormCollection fc)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);
            DBItem changes = new DBItem();
            DBItem oldVal = new DBItem();

            foreach (DBColumn c in table.columns)
            {
                changes[c.Name] = fc.Get("col" + c.Name);
                oldVal[c.Name] = TempData[c.Name];
            }
            table.Update(changes, oldVal);
            app.SaveChanges();

            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult DisableConstraint(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            ViewBag.Constraints = table.getConstraints();
            return View(table);
        }

        public ActionResult DisableCon(string appName, string tableName, FormCollection fc)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            string constraintName = (fc["all"] != null) ? "ALL" : fc["constraintName"];
            table.DisableConstraint(constraintName);

            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }

        public ActionResult EnableConstraint(string appName, string tableName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            ViewBag.Constraints = table.getConstraints();
            return View(table);
        }

        public ActionResult EnableCon(string appName, string tableName, FormCollection fc)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            string constraintName = (fc["all"] != null) ? "ALL" : fc["constraintName"];
            table.EnableConstraint(constraintName);
            app.SaveChanges();

            return RedirectToAction("Index", new { @appName = appName });
        }
        public JsonResult getTableColumns(string tableName, string appName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable table = app.GetTable(tableName);
            List<string> tableColumns = table.columns.Select(x => x.Name).ToList();

            return Json(tableColumns, JsonRequestBehavior.AllowGet);
        }
    }
}