using GrassrootsFloodCtrl.ServiceInterface;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrassrootsFloodCtrl.Controllers
{
    [Authorize]
    public class LeaderController : ControllerBase
    {
        // GET: Leader
        public ActionResult Index()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            return View();
        }
    }
}