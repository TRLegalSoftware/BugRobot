using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugRobot.Lib;

namespace BugRobot.Web.Controllers
{
    public class BotController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.URL = @"http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";
            ViewBag.Interval = "5";

            return View();
        }

        [HttpGet]
        public JsonResult GetBugsFromTFS(string queryUrl, string userName, bool autoAssign, bool notifyOnlyNewBugs, string notifiedBugs)
        {
            var bugRobot = new BugRobot.Lib.BugRobot(queryUrl, userName, autoAssign, notifyOnlyNewBugs, notifiedBugs);

            var result = bugRobot.Run();

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
