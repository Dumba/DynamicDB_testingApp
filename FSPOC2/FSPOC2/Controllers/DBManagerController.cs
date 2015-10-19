using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using FSPOC2.Models;
using Entitron;
using Microsoft.SqlServer.Server;

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
            TempData.Remove("oldTableName");
            TempData.Add("oldTableName", tableName);

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
            return View();
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
                TempData["message-success"] = "Table "+ model.tableName +" was successfully created.";
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
            TempData["message-success"] = "Table "+tableName+" was successfully dropped.";
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

        [HttpPost]
        public ActionResult RenameTable(string appName, DBTable model)
        {
            string tableName = TempData["oldTableName"].ToString();
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                DBApp app = new DBApp()
                {
                    Name = appName,
                    ConnectionString = (new Entities()).Database.Connection.ConnectionString
                };
                
                DBTable table = app.GetTable(tableName);
                table.Rename(model.tableName);
                app.SaveChanges();
                TempData["message-success"] = "Table "+tableName+" was successfully renamed to "+model.tableName+".";
                return RedirectToAction("Index", new { @appName = appName });
            }

            return View();

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
            TempData["message-success"] = "Column " + column.Name + " was successfully added.";

            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult AlterColumn(string appName, string tableName, string columnName)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);
            DBColumn col = table.columns.SingleOrDefault(c => c.Name == columnName);

            return View(col);
        }

        [HttpPost]
        public ActionResult AlterColumn(string appName, string tableName, DBColumn column)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            app.GetTable(tableName)
                .columns.ModifyInDB(column);
            app.SaveChanges();

            TempData["message-success"] = "Column " + column.Name + " was successfully altered.";
            return RedirectToAction("Details", new { @appName = appName, @tableName = tableName });
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

            TempData["message-success"] = "Column " + columnName + " was successfully dropped.";
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
                        
            TempData["message-success"] = "Index "+fc["indexname"]+" of table "+tableName+" was successfully created.";
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
            List<string> indexNames = new List<string>();

            foreach (DBIndex index in table.indices)
            {
                if (index.indexName != "index_" + appName + tableName)
                {
                    indexNames.Add(index.indexName);
                }
            }

            if (indexNames.Count != 0)
            {
                ViewBag.indeces = indexNames;
                return View(table);
            }
            TempData["message-error"] = "Table has no index to drop.";
            return RedirectToAction("Index", new { @appName = appName });
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
            TempData["message-success"] = "Index " + indexName + " of table " + tableName + " was successfully dropped.";
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
            ViewBag.tables = app.GetTables().Select(t => t.tableName);
            ViewBag.Columns = table.columns.Select(x => x.Name);

            return View(new DBForeignKey() { sourceTable = table });
        }
        [HttpPost]
        public ActionResult AddForeignKey(string appName, DBForeignKey model)
        { 
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };

            DBTable sTable = app.GetTable(model.sourceTable.tableName);
            DBTable tTable = app.GetTable(model.targetTable.tableName);
            DBColumn sColumn = sTable.columns.SingleOrDefault(x => x.Name == model.sourceColumn);
            DBColumn tColumn = tTable.columns.SingleOrDefault(x => x.Name == model.targetColumn);
            
            if (sColumn.type != tColumn.type) //columns must have equal data types
            {
                TempData["message-error"] = "Keys have different data types.";
                return RedirectToAction("CreateForeignKey", new { @appName = appName, @tableName = sTable.tableName });
            }
            sTable.foreignKeys.AddToDB(model);
            app.SaveChanges();
            
            TempData["message-success"] = "Foreign key " + model.name + " of table " + sTable.tableName + " was successfully created.";
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
            if (table.foreignKeys.GetForeignKeyForDrop().Count != 0)
            {
                ViewBag.foreignKeys = table.foreignKeys.GetForeignKeyForDrop();
                return View(table);
            }
            TempData["message-error"] = "Table has no foreign key to drop.";
            return RedirectToAction("Index", new { @appName = appName });
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
            TempData["message-success"] = "Foreign key " + foreignKeyName + " of table " + tableName + " was successfully dropped.";
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
            TempData["message-success"] = "Primary key of table " + tableName + " was successfully created.";
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
            if (table.primaryKeys.Count != 0)
            {
                table.DropPrimaryKey();
                app.SaveChanges();
            }
            else
            {
                TempData["message-error"] = "Table has no primary key to drop.";
            }
            TempData["message-success"] = "Primary key of table " + tableName + " was successfully dropped.";
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
                if (fc.Get("col" + c.Name) == "")
                {
                    row[c.Name] = null;
                }
                else
                {


                    switch (c.type.ToLower())
                    {
                        case "int":
                            row[c.Name] = Convert.ToInt32(fc.Get("col" + c.Name));
                            break;
                        case "bigint":
                            row[c.Name] = Convert.ToInt64(fc.Get("col" + c.Name));
                            break;
                        case "smallint":
                            row[c.Name] = Convert.ToInt16(fc.Get("col" + c.Name));
                            break;
                        case "tinyint":
                            row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                            break;
                        case "decimal":
                            row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                            break;
                        case "smallmoney":
                            row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                            break;
                        case "money":
                            row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                            break;
                        case "float":
                            row[c.Name] = Convert.ToDouble(fc.Get("col" + c.Name));
                            break;
                        case "real":
                            row[c.Name] = Convert.ToSingle(fc.Get("col" + c.Name));
                            break;
                        case "date":
                            row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                            break;
                        case "time":
                            row[c.Name] = TimeSpan.Parse(fc.Get("col" + c.Name));
                            break;
                        case "datetime":
                            row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                            break;
                        case "datetime2":
                            row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                            break;
                        case "datetimeoffset":
                            row[c.Name] = DateTimeOffset.Parse(fc.Get("col" + c.Name));
                            break;
                        case "timestamp":
                            row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                            break;
                        case "varbinary":
                            row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                            break;
                        case "bit":
                            row[c.Name] = Convert.ToBoolean(fc.Get("col" + c.Name));
                            break;
                        case "binary":
                            row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                            break;
                        case "uniqueidentifier":
                            row[c.Name] = Guid.Parse(fc.Get("col" + c.Name));
                            break;
                        default:
                            row[c.Name] = fc.Get("col" + c.Name);
                            break;

                    }
                }
                //if (c.type.ToLower() == "int")
                //    row[c.Name] = Convert.ToInt32(fc.Get("col" + c.Name));
                //else
                //    row[c.Name] = fc.Get("col" + c.Name);
            }

            table.Add(row);
            app.SaveChanges();
            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        [HttpPost]
        public ActionResult DeleteOrUpdate(string appName, string tableName, FormCollection fc)
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
                switch (c.type.ToLower())
                {
                    case "int":
                        row[c.Name] = Convert.ToInt32(fc.Get("col" + c.Name));
                        break;
                    case "bigint":
                        row[c.Name] = Convert.ToInt64(fc.Get("col" + c.Name));
                        break;
                    case "smallint":
                        row[c.Name] = Convert.ToInt16(fc.Get("col" + c.Name));
                        break;
                    case "tinyint":
                        row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "decimal":
                        row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "smallmoney":
                        row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "money":
                        row[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "float":
                        row[c.Name] = Convert.ToDouble(fc.Get("col" + c.Name));
                        break;
                    case "real":
                        row[c.Name] = Convert.ToSingle(fc.Get("col" + c.Name));
                        break;
                    case "date":
                        row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "time":
                        row[c.Name] = TimeSpan.Parse(fc.Get("col" + c.Name));
                        break;
                    case "datetime":
                        row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "datetime2":
                        row[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "datetimeoffset":
                        row[c.Name] = DateTimeOffset.Parse(fc.Get("col" + c.Name));
                        break;
                    case "timestamp":
                        row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "varbinary":
                        row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "bit":
                        row[c.Name] = Convert.ToBoolean(fc.Get("col" + c.Name));
                        break;
                    case "binary":
                        row[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "uniqueidentifier":
                        row[c.Name] = Guid.Parse(fc.Get("col" + c.Name));
                        break;
                    default:
                        row[c.Name] = fc.Get("col" + c.Name);
                        break;
                }
                TempData.Remove(c.Name);
                TempData.Add(c.Name, row[c.Name]);
            }
            if (fc.Get("Update") != null)
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
                switch (c.type.ToLower())
                {
                    case "int":
                        changes[c.Name] = Convert.ToInt32(fc.Get("col" + c.Name));
                        break;
                    case "bigint":
                        changes[c.Name] = Convert.ToInt64(fc.Get("col" + c.Name));
                        break;
                    case "smallint":
                        changes[c.Name] = Convert.ToInt16(fc.Get("col" + c.Name));
                        break;
                    case "tinyint":
                        changes[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "decimal":
                        changes[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "smallmoney":
                        changes[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "money":
                        changes[c.Name] = Convert.ToDecimal(fc.Get("col" + c.Name));
                        break;
                    case "float":
                        changes[c.Name] = Convert.ToDouble(fc.Get("col" + c.Name));
                        break;
                    case "real":
                        changes[c.Name] = Convert.ToSingle(fc.Get("col" + c.Name));
                        break;
                    case "date":
                        changes[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "time":
                        changes[c.Name] = TimeSpan.Parse(fc.Get("col" + c.Name));
                        break;
                    case "datetime":
                        changes[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "datetime2":
                        changes[c.Name] = Convert.ToDateTime(fc.Get("col" + c.Name));
                        break;
                    case "datetimeoffset":
                        changes[c.Name] = DateTimeOffset.Parse(fc.Get("col" + c.Name));
                        break;
                    case "timestamp":
                        changes[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "varbinary":
                        changes[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "bit":
                        changes[c.Name] = Convert.ToBoolean(fc.Get("col" + c.Name));
                        break;
                    case "binary":
                        changes[c.Name] = Convert.ToByte(fc.Get("col" + c.Name));
                        break;
                    case "uniqueidentifier":
                        changes[c.Name] = Guid.Parse(fc.Get("col" + c.Name));
                        break;
                    default:
                        changes[c.Name] = fc.Get("col" + c.Name);
                        break;
                }
                oldVal[c.Name] = TempData[c.Name];
            }
            table.Update(changes, oldVal);
            app.SaveChanges();

            return RedirectToAction("Data", new { @appName = appName, @tableName = tableName });
        }

        public ActionResult Constraint(string appName, string tableName, bool isDisable)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);
            ViewBag.Constraints = table.getConstraints(isDisable);

            if (isDisable)
            {
                if (table.getConstraints(isDisable).Count != 0)
                {
                    return View("DisableConstraint", table);
                }
                TempData["message-error"] = "Table has no constraint to disable.";
                return RedirectToAction("Index", new { @appName = appName });
            }
            else
            {
                if (table.getConstraints(isDisable).Count != 0)
                {
                    return View("EnableConstraint", table);

                }
                TempData["message-error"] = "Table has no constraint to enable.";
                return RedirectToAction("Index", new { @appName = appName });
            }
        }

        [HttpPost]
        public ActionResult DisableOrEnableConstraint(string appName, string tableName, FormCollection fc, bool isDisable)
        {
            DBApp app = new DBApp()
            {
                Name = appName,
                ConnectionString = (new Entities()).Database.Connection.ConnectionString
            };
            DBTable table = app.GetTable(tableName);

            string constraintName = (fc["all"] != null) ? "ALL" : fc["constraintName"];
            if (isDisable)
            {
                table.DisableConstraint(constraintName);
                TempData["message-success"] = "Constraint  of table " + tableName + " was successfully disabled.";
            }
            else
            {
                table.EnableConstraint(constraintName);
                TempData["message-success"] = "Constraint  of table " + tableName + " was successfully enabled.";
            }

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