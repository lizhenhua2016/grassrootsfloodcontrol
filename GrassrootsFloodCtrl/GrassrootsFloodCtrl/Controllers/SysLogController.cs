﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrassrootsFloodCtrl.Controllers
{
    [Authorize]
    public class SysLogController : ControllerBase
    {
        // GET: SysLog
        public ActionResult Index()
        {
            return View();
        }
    }
}