using DbUpgrade.Core;
using DbUpgrade.Web.Models;
using Microsoft.SqlServer.Dac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbUpgrade.Web.Controllers
{
    public class HomeController : Controller
    {

        protected override void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception;
            filterContext.ExceptionHandled = true;

            var model = new HandleErrorInfo(filterContext.Exception, "Controller", "Action");

            filterContext.Result = new ViewResult()
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(model)
            };
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DacPac()
        {
            ViewBag.Message = "Dacpac generation page.";

            return View();
        }

        public ActionResult Script()
        {
            ViewBag.Message = "Script generation page.";

            return View("Script", new ScriptModel
            {
                IgnoreNotForReplication = true,
                IgnoreObjectTypes = new ObjectType[] 
                {
                    ObjectType.Users,
                    ObjectType.Views,
                    ObjectType.RoleMembership,
                    ObjectType.Permissions,
                    ObjectType.ExtendedProperties,
                    ObjectType.StoredProcedures,
                    ObjectType.Logins,
                    ObjectType.DatabaseTriggers,
                    ObjectType.ServerTriggers
                }
            });
        }

        [HttpPost]
        public ActionResult GenerateDacPac(DacPacModel model)
        {
            if (ModelState.IsValid)
            {
                var upgradeService = new DbUpgradeService(DbConnection.From(model.ConnectionString, model.DatabaseName));

                var dacpacFile = upgradeService.GenerateDacPac();

                var stream = new MemoryStream(dacpacFile);
                return File(stream, "application/octet-stream", $"{model.DatabaseName}.dacpac");
            }

            return View();
        }

        [HttpPost]
        public ActionResult GenerateScript(ScriptModel model)
        {
            if (ModelState.IsValid && model.DacPac.ContentLength != 0)
            {
                var upgradeService = new DbUpgradeService(DbConnection.From(model.ConnectionString, model.DatabaseName));
                var upgradeOptions = DbUpgradeOptions.From(model.IgnoreObjectTypes);

                upgradeOptions.IgnoreNotForReplication = model.IgnoreNotForReplication;
                upgradeOptions.DropConstraintsNotInSource = model.DropConstraintsNotInSource;
                upgradeOptions.DropIndexesNotInSource = model.DropIndexesNotInSource;
                upgradeOptions.VerifyDeployment = model.VerifyDeployment;

                using (var memoryStream = new MemoryStream())
                {
                    model.DacPac.InputStream.CopyTo(memoryStream);

                    var dacpac = memoryStream.ToArray();
                    var upgradeScript = upgradeService.GenerateUpgradeScript(dacpac, upgradeOptions, model.DatabaseName);

                    return View("Publish", new PublishScript { Script = upgradeScript });
                }
            }

            return View();
        }
    }
}