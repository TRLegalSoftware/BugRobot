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
            return View();
        }

        [HttpGet]
        public JsonResult GetBugsFromTFS(string queryUrl, string userName, bool autoAssign)
        {
            var bugRobot = new BugRobot.Lib.BugRobot(queryUrl, userName, autoAssign);

            var result = bugRobot.Run();

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
