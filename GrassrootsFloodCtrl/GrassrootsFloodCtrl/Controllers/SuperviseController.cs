using GrassrootsFloodCtrl.Logic.Common;
using GrassrootsFloodCtrl.Logic.SumAppUser;
using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.Enums;
using GrassrootsFloodCtrl.Model.SumAppUser;
using GrassrootsFloodCtrl.Model.Supervise;
using GrassrootsFloodCtrl.ServiceInterface;
using GrassrootsFloodCtrl.ServiceModel.Route;
using GrassrootsFloodCtrl.ServiceModel.Village;
using NPOI.HSSF.UserModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrassrootsFloodCtrl.Controllers
{
    [Authorize]
    public class SuperviseController : ControllerBase
    {
        public ISunAppUserLogic logic { get; set; }
        #region 统计
        #region 省查看
        public ActionResult Index()
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            //var r= HostContext.Resolve<SuperviseService>().Post(new SPersonLiable() { adcd=adcd });
            return View();
        }
        public ActionResult LeaderIndex()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            //var r= HostContext.Resolve<SuperviseService>().Post(new SPersonLiable() { adcd=adcd });
            return View();
        }
        #endregion

        #region 市查看
        [HttpGet]
        [ValidateInput(false)]
        public ActionResult County(string id)
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = string.IsNullOrEmpty(id) ?adcd:id;
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult LeaderCounty(string id)
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = string.IsNullOrEmpty(id) ? adcd : id;
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult CountyInfo(string id)
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。。");
            }
            if (string.IsNullOrEmpty(id)) throw new Exception("参数不能为空！");
            ViewData["adcd"] = string.IsNullOrEmpty(id) ? adcd : id;
            return View();
        }
        #endregion

        #region 县查看
        public ActionResult Towns(string id)
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID !=(int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            var ac= string.IsNullOrEmpty(id) ? adcd : id;
            ViewData["adcd"] = ac;
            ViewData["cadcd"] = ac.Substring(0,4)+ "00000000000";
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult LeaderTowns(string id)
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            var ac = string.IsNullOrEmpty(id) ? adcd : id;
            ViewData["adcd"] = ac;
            ViewData["cadcd"] = ac.Substring(0, 4) + "00000000000";
            ViewData["rid"] = RoleID;
            return View();
        }
        #endregion

        #region 镇级信息
        public ActionResult TownInfo(string id)
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            var ac = string.IsNullOrEmpty(id) ? adcd : id;
            ViewData["adcd"] = ac;
            ViewData["ccadcd"] = ac.Substring(0, 4) + "00000000000";
            ViewData["cadcd"] = ac.Substring(0, 6) + "000000000";
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult LeaderTownInfo(string id)
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            var ac = string.IsNullOrEmpty(id) ? adcd : id;
            ViewData["adcd"] = ac;
            ViewData["ccadcd"] = ac.Substring(0, 4) + "00000000000";
            ViewData["cadcd"] = ac.Substring(0, 6) + "000000000";
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult TownVillage(string _adcd,string lng,string lat)
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。。");
            }
            ViewData["adcd"] = _adcd;
            ViewData["lng"] = lng;
            ViewData["lat"] = lat;
            return View();
        }
        #endregion

        #region 县级统计
        public ActionResult tj()
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            //var r= HostContext.Resolve<SuperviseService>().Post(new SPersonLiable() { adcd=adcd });
            return View();
        }
        #endregion
        #endregion
        #region 菜单
        public ActionResult left()
        {
            if (RoleID == null) { return View("Error", model: "登陆已到期，请重新登陆。"); }
            var url = "";
            var urlkh = "";
            var urlkhjl = "";
            var urllog = "";
            var urlapp = "";
            var urltask = "";
            var usrMessger = "";
            var userApp = "";
            switch (RoleID)
            {
                case 5:
                    url = "/Supervise";
                    urlkh = "/Supervise/CCKH";
                    urlkhjl = "/Supervise/CCHKJL";
                    urllog = "/Log/LogCityIndex";
                    urlapp = "/Supervise/AppInPostCityIndex";
                    usrMessger = "/Supervise/AppWarnList";
                    //usrMessger = "/Supervise/AppWarnMoment";
                    userApp = "/Supervise/AppUserList";
                    break;
                case 2:
                    url = "/Supervise/County";
                    urlkh = "/Supervise/CCKH";
                    urlkhjl = "/Supervise/CCHKJL";
                    urllog = "/Log/LogCountyIndex";
                    urlapp = "/Supervise/AppInPostCountyIndex";
                    //usrMessger = "/Supervise/AppWarnList";
                    usrMessger = "/Supervise/AppWarnMoment";
                    userApp = "/Supervise/AppUserList";
                    break;
                case 3:
                    url = "/Supervise/Towns";
                    urlkh = "";
                    urlkhjl = "";
                    urllog = "/Log/LogTownIndex";
                    urlapp = "/Supervise/AppInPostTownIndex";
                    urltask = "/TaskArragement/sendmessage";
                    usrMessger = "/Supervise/AppWarnList";
                    //usrMessger = "/Supervise/AppWarnMoment";
                    userApp = "/Supervise/AppUserList";
                    break;
                case 4:
                    usrMessger = "/Supervise/AppWarnList";
                    //usrMessger = "/Supervise/AppWarnMoment";
                    userApp = "/Supervise/AppUserList";
                    break;
            }
            ViewData["url"] = url;
            ViewData["urlkh"] = urlkh;
            ViewData["urlkhjl"] = urlkhjl;
            ViewData["rid"] = RoleID;
            ViewData["urllog"] = urllog;
            ViewData["urlapp"] = urlapp;
            ViewData["urltask"] = urltask;
            ViewData["usrMessger"] = usrMessger;
            ViewData["userApp"] = userApp;
            return View();
        }
        public ActionResult Leaderleft()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if (RoleID == null) { return View("Error", model: "登陆已到期，请重新登陆。"); }
            var url = "";
            var urlkh = "";
            var urlkhjl = "";
            var urllog = "";
            var urlapp = "";
            var urltask = "";
            var usrMessger = "";
            var userApp = "";
            switch (RoleID)
            {
                case 5:
                    url = "/Supervise/LeaderIndex";
                    urlkh = "/Supervise/LeaderCCKH";
                    urlkhjl = "/Supervise/LeaderCCHKJL";
                    urllog = "/Log/LeaderLogCityIndex";
                    urlapp = "/Supervise/LeaderAppInPostCityIndex";
                    usrMessger = "/Supervise/LeaderAppWarnList";
                    userApp = "/Supervise/LeaderAppUserList";
                    break;
                case 2:
                    url = "/Supervise/County";
                    urlkh = "/Supervise/CCKH";
                    urlkhjl = "/Supervise/CCHKJL";
                    urllog = "/Log/LogCountyIndex";
                    urlapp = "/Supervise/AppInPostCountyIndex";
                    usrMessger = "/Supervise/AppWarnList";
                    userApp = "/Supervise/AppUserList";
                    break;
                case 3:
                    url = "/Supervise/Towns";
                    urlkh = "";
                    urlkhjl = "";
                    urllog = "/Log/LogTownIndex";
                    urlapp = "/Supervise/AppInPostTownIndex";
                    urltask = "/TaskArragement/sendmessage";
                    usrMessger = "/Supervise/AppWarnList";
                    userApp = "/Supervise/AppUserList";
                    break;
                case 4:
                    url = "/Supervise/AppUserList";
                    break;
            }
            ViewData["url"] = url;
            ViewData["urlkh"] = urlkh;
            ViewData["urlkhjl"] = urlkhjl;
            ViewData["rid"] = RoleID;
            ViewData["urllog"] = urllog;
            ViewData["urlapp"] = urlapp;
            ViewData["urltask"] = urltask;
            ViewData["usrMessger"] = usrMessger;
            ViewData["userApp"] = userApp;
            return View();
        }
        #endregion
        #region 抽查
        /// <summary>
        /// 抽查考核
        /// </summary>
        /// <returns></returns>
        public ActionResult CCKH()
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            ViewData["userid"] = UserId;
            ViewData["realname"] = UserSession.RealName;
            ViewData["checktime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                ViewData["itemListCounty"] = GetList(5);
                ViewData["itemListTown"] = GetList(4);
                ViewData["itemListVillage"] = GetList(1);
            }
            catch (Exception ex) { }
            return View();
        }
        public ActionResult LeaderCCKH()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            ViewData["userid"] = UserId;
            ViewData["realname"] = UserSession.RealName;
            ViewData["checktime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                ViewData["itemListCounty"] = GetList(5);
                ViewData["itemListTown"] = GetList(4);
                ViewData["itemListVillage"] = GetList(1);
            }
            catch (Exception ex) { }
            return View();
        }
        public List<SelectListItem> GetList(int tid)
        {
            using (var service = HostContext.ResolveService<PostService>())
            {
                var itemListCounty = new List<SelectListItem>();
                service.Get(new GetPostList()
                {
                    PageSize = int.MaxValue,
                    typeId = tid
                }).rows.ForEach(x =>
                {
                    itemListCounty.Add(new SelectListItem() { Text = x.PostName, Value = x.ID.ToString() });

                });
                return itemListCounty;
            }
        }
        /// <summary>
        /// 抽查记录
        /// </summary>
        /// <returns></returns>
        public ActionResult CCHKJL()
        {
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult LeaderCCHKJL()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            if ((RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户) && (RoleID != (int)GrassrootsFloodCtrlEnums.RoleEnums.县级用户))
            {
                return View("Error", model: "登陆已到期，请重新登陆。");
            }
            ViewData["adcd"] = adcd;
            ViewData["rid"] = RoleID;
            return View();
        }

        #endregion
        #region 到港统计
        public ActionResult AppInPostCityIndex()
        {
            //ViewData["s"] = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            // ViewData["e"] = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        public ActionResult AppInPostCountyIndex(int? id, string s, string e,string _adcd, string grade)
        {
            ViewData["s"] = s;
            ViewData["e"] = e;
            ViewData["rid"] = RoleID;
            ViewData["adcd"] = string.IsNullOrEmpty(_adcd) && RoleID == 2 ? adcd : _adcd;
            ViewData["id"] = id == null ? 0:id;
            ViewData["grade"] = grade;
            return View();
        }
        public ActionResult AppInPostTownIndex(int? id, int? pid, string s, string e, string _adcd, string grade)
        {
            var _adcd1 = string.IsNullOrEmpty(_adcd) && RoleID == 3 ? adcd : _adcd;
            ViewData["s"] = s;
            ViewData["e"] = e;
            ViewData["adcd"] = _adcd1;
            ViewData["id"] = id;
            ViewData["pid"] = pid;
            ViewData["grade"] = grade;
            ViewData["adcdparent"] = _adcd1.Substring(0, 4) + "00000000000";
            ViewData["rid"] = RoleID;
            return View();
        }
        public ActionResult AppInPostTownInfo(string _adcd,int? id,int? pid)
        {
            var ac = string.IsNullOrEmpty(_adcd) ? adcd : _adcd;
            ViewData["adcd"] = ac;
            ViewData["ccadcd"] = ac.Substring(0, 4) + "00000000000";
            ViewData["cadcd"] = ac.Substring(0, 6) + "000000000";
            ViewData["rid"] = RoleID;
            ViewData["id"] = id;
            ViewData["pid"] = pid;
            return View();
        }
        #endregion



        #region 应急消息管理
        //任务列表
        public ActionResult AppWarnList()
        {
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }
        public ActionResult LeaderAppWarnList()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }

        //事件列表
        /// <summary>
        /// 事件的id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AppWarnInfoList(int id)
        {

            ViewData["warnId"] = id;
            return View();
        }
        public ActionResult LeaderAppWarnInfoList(int id)
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            ViewData["warnId"] = id;
            return View();
        }
        /// <summary>
        /// 统计阅读人员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult SumWarnRead(string id,bool read,string reciveAdcd)
        {
            ViewData["warnInfoId"] = id;
            ViewData["isReaD"] = read;
            ViewData["reciveAdcd"] = reciveAdcd;
            return View();
        }
        /// <summary>
        /// 统计未阅读人员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult SumWarnNoRead(string id, bool read, string reciveAdcd)
        {
            ViewData["warnInfoId"] = id;
            ViewData["isReaD"] = read;
            ViewData["reciveAdcd"] = reciveAdcd;
            return View();
        }

        public ActionResult SumAppMessagePerson(string id,string countryadcd)
        {
            ViewData["warnEventId"] = id;
            ViewData["adcd"] = countryadcd;
            return View();
        }

        public ActionResult DutyDisplay(string id)
        {
            ViewBag.messageid = id;
            return View();
        }
        #endregion

        #region 注册人数统计
        public ActionResult AppUserList()
        {
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }
        public ActionResult LeaderAppUserList()
        {
            var session = base.SessionAs<Model.Common.UserSession>();
            ViewData["leaderName"] = session.yhRealName;
            ViewData["orgname"] = session.orgname;
            ViewData["headpicture"] = session.headpicture;
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }

        public ActionResult CheckUser()
        {
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }
        public ActionResult AppNoRegister(string adcd)
        {
            ViewData["adcd"] = adcd;
            ViewData["role"] = AdcdHelper.GetByAdcdRole(adcd);
            return View();
        }

        public FileResult DownSumExcel()
        {
            RouteSumAppUser2 model = new RouteSumAppUser2();
            model.adcd = adcd;
            model.adcdName = "浙江省";
            List<SelectSumAppUserList> list = logic.GetSelectSumAppUserList2(model);
            list =list.OrderBy(x => x.adcdId).ToList();
            list = ArrangeHelper.GetList(list);
            List<string> title = new List<string>();
            title.Add("单位名称");
            title.Add("责任人总人数");
            title.Add("已注册人数");
            HSSFWorkbook book=ExcelHelper.CreateExecl(title,list);
            string strdate = "APP注册人员统计"+DateTime.Now.ToString("yyyyMMddhhmmss");//获取当前  
            // 写入到客户端   
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", strdate + ".xls");
        }

        public FileResult DownNoRegisterExcel()
        {
            //查找已注册人员
            List<AppUserPerson> list = logic.GetCountryList2(adcd, false);
            List<string> title = new List<string>();
            title.Add("单位名称");
            title.Add("姓名");
            title.Add("电话");
            HSSFWorkbook book = ExcelHelper.CreateExecl(title, list);
            string strdate = "APP未注册人员名单" + DateTime.Now.ToString("yyyyMMddhhmmss");       
            // 写入到客户端   
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", strdate + ".xls");
        }
        public FileResult DownRegisterExcel()
        {
            //查找已注册人员
            List<AppUserPerson> list = logic.GetCountryList2(adcd, true);
            List<string> title = new List<string>();
            title.Add("单位名称");
            title.Add("姓名");
            title.Add("电话");
            HSSFWorkbook book = ExcelHelper.CreateExecl(title, list);
            string strdate = "APP已注册人员名单" + DateTime.Now.ToString("yyyyMMddhhmmss");
            // 写入到客户端   
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", strdate + ".xls");
        }
        #endregion


        public ActionResult AppWarnMoment()
        {            
            ViewData["Level"] = "";
            ViewData["adcd"] = adcd;
            if ((RoleID == (int)GrassrootsFloodCtrlEnums.RoleEnums.省级用户))
            {
                ViewData["Level"] = "省级用户";
            }
            else if ((RoleID == (int)GrassrootsFloodCtrlEnums.RoleEnums.市级用户))
            {
                ViewData["Level"] = "市级用户";
            }
            return View();
        }

        public ActionResult AppWarnMomentCounty()
        {
            return View();
        }

        public ActionResult BindTownPostedLiableData(int? warninfoid,string adcd)
        {
            ViewBag.warninfoid = warninfoid;
            ViewBag.adcd = adcd;
            return View();
        }

        public ActionResult BindTownReadLiableData(int? warninfoid, string isRead, string adcd)
        {
            ViewBag.warninfoid = warninfoid;
            ViewBag.isRead = isRead;
            ViewBag.adcd = adcd;
            return View();
        }

        public ActionResult BindTownUnReadLiableData(int? warninfoid, string isRead, string adcd)
        {
            ViewBag.warninfoid = warninfoid;
            ViewBag.isRead = isRead;
            ViewBag.adcd = adcd;
            return View();
        }
    }
}