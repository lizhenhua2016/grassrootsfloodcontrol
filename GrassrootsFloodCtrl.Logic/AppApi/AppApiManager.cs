// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppApiManager.cs" company="abc">
//   abc
// </copyright>
// <summary>
//   Defines the AppApiManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.UI.WebControls;

namespace GrassrootsFloodCtrl.Logic.AppApi
{
    using Dy.Common;
    using GrassrootsFloodCtrl.Logic.Common;
    using GrassrootsFloodCtrl.Logic.Factory;
    using GrassrootsFloodCtrl.Model;
    using GrassrootsFloodCtrl.Model.AppApi;
    using GrassrootsFloodCtrl.Model.CountryPerson;
    using GrassrootsFloodCtrl.Model.Sys;
    using GrassrootsFloodCtrl.Model.Town;
    using GrassrootsFloodCtrl.Model.Village;
    using GrassrootsFloodCtrl.Model.ZZTX;
    using GrassrootsFloodCtrl.ServiceModel.AppApi;
    using GrassrootsFloodCtrl.ServiceModel.Common;
    using GrassrootsFloodCtrl.ServiceModel.Route;
    using Model.Common;
    using Model.NewAppApi;
    using ServiceStack;
    using ServiceStack.OrmLite;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AppUserGps = GrassrootsFloodCtrl.ServiceModel.AppApi.AppUserGps;
    using Model.Org;
    using System.Threading;
    using CachHelper;

    /// <summary>
    /// The app api manager.
    /// </summary>
    public class AppApiManager : ManagerBase, IAppApiManager
    {
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppGetLoginVCode(AppGetLoginVCode request)
        {
            BaseResult br = null;
            var _usernname = request.UserName.Trim();
            if (!ValidatorHelper.IsMobile(_usernname))
            {
                br = new BaseResult();
                br.IsSuccess = false;
                br.ErrorMsg = "账号格式错误！请输入标准的手机号";
                return br;
            }
            using (var db = DbFactory.Open())
            {
                br = new BaseResult();
                //原有的存储过程方式
                //var f = db.SqlList<ServiceModel.Common.VillagePerson>("EXEC AppGetUserInfo @handphone,@nums",
                //    new { handphone = _usernname, nums = 1 });
                //if (f == null)
                //{
                //    br.IsSuccess = false;
                //    br.ErrorMsg = "账号不存在！";
                //    return br;
                //}
                var userpositionTB = db.From<UserPosition>();
                userpositionTB.And(x => x.Phone == _usernname);
                var result = db.Select(userpositionTB);
                result = result.AsEnumerable().Where(x => !x.adcd.Contains("000000")).ToList();
                if (result.Count() == 0)
                {
                    br.IsSuccess = false;
                    br.ErrorMsg = "账号不存在！";
                    return br;
                }

                //else if (!f.isEnable) { br.IsSuccess = false; br.ErrorMsg = "账号已经禁用！"; return br; }
                else
                {
                    //发送短信
                    Random rd = new Random();
                    int num = rd.Next(100000, 1000000);
                    //发送短信前验证下，是否已经安装注册过
                    var content = "";
                    var fcode = db.Single<AppLoginVCode>(w => w.userName == request.UserName.Trim());
                    if (fcode == null)
                    {
                        //没有安装注册过
                        content = "您正在登陆《浙江省基层防汛防台体系信息管理系统》APP,验证码为:" + num.ToString() + "。如不是您自己操作，请忽略。";
                    }
                    else
                    {
                        //安装过
                        content = "警告!!!!!您正在重新登陆《浙江省基层防汛防台体系信息管理系统》APP,验证码为:" + num.ToString() +
                                  "。如果是您自己操作，请重新登陆，否则请及时删除，一旦泄露，后果自负！";
                    }
                    var msg = SmsSend.SendSMS(request.UserName, content);

                    if (msg != "" && msg.IndexOf("-") > 0)
                    {
                        AppLoginVCode _model = new AppLoginVCode()
                        {
                            adddtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                            userName = request.UserName,
                            VerificationCode = num.ToString()
                        };
                        if (fcode != null)
                        {
                            br.IsSuccess = db.Update(_model, w => w.userName == request.UserName) > 0 ? true : false;
                        }
                        else
                        {
                            br.IsSuccess = db.Insert(_model) > 0 ? true : false;
                        }
                        if (!br.IsSuccess)
                        {
                            br.ErrorMsg = "系统异常";
                        }
                    }
                }
                return br;
            }
        }

        /// <summary>
        /// 登陆：第一次和免登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppLogin(AppLogin request)
        {
            BaseResult br = new BaseResult();
            using (var db = DbFactory.Open())
            {
                var _usernname = request.UserName;
                var vCode = request.VCode;
                if (string.IsNullOrEmpty(_usernname))
                    br.ErrorMsg = "用户名不能为空";
                if (string.IsNullOrEmpty(vCode))
                    br.ErrorMsg = "验证码不能为空";

                //用旧的验证码登录验证
                var fold = db.Single<AppLoginVCode>(w =>
                    w.userName == _usernname && w.VerificationCode == vCode && w.token != "");
                if (fold != null)
                {
                    br.IsSuccess = false;
                    br.token = "您的验证码已经无效!";
                    return br;
                }
                var list = db.SqlList<ServiceModel.Common.VillagePerson>("EXEC AppUserLogin @handphone,@code",
                    //new {handphone = _usernname, code = vCode});
                    new { handphone = _usernname, code = vCode });
                if (list != null && list.Count > 0)
                {
                    var _token = Guid.NewGuid().ToString("N");
                    //生成token
                    //var i = db.UpdateOnly(() => new AppLoginVCode {token = _token}, w => w.userName == _usernname) > 0
                    var i = db.UpdateOnly(() => new AppLoginVCode { token = _token }, w => w.userName == _usernname) > 0
                        ? true
                        : false;
                    if (!i)
                    {
                        br.IsSuccess = false;
                        br.ErrorMsg = "token账号生成失败！";
                        db.Delete<AppLoginVCode>(w => w.userName == _usernname);
                        return br;
                    }
                    else
                    {
                        br.IsSuccess = true;
                        br.token = _token;
                    }
                }
                else
                {
                    br.IsSuccess = false;
                    br.ErrorMsg = "账号密码错误!";
                }
                return br;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AppUserInfoViewModel AppGetUserInfo(AppGetUserInfo request)
        {
            AppUserInfoViewModel _model = null;
            if (string.IsNullOrEmpty(request.token)) return _model;
            var _token = request.token;

            using (var db = this.DbFactory.Open())
            {
                var fcode = db.Single<AppLoginVCode>(w => w.token == _token);
                if (fcode == null) return _model;
                _model = new AppUserInfoViewModel();

                var f = db.SqlList<ServiceModel.Common.VillagePerson>(
                    "EXEC AppGetUserInfoAndVCode @handphone,@token,@nums",
                    //new {handphone = fcode.userName, token = fcode.token, nums = 0});
                    new { handphone = fcode.userName, token = fcode.token, nums = 0 });
                if (f == null) return _model;

                // 获取用户信息
                _model.adcd = f.FirstOrDefault().adcd;
                _model.RealName = f.FirstOrDefault().name;
                _model.UserName = f.FirstOrDefault().HandPhone;

                // 需要转移的人员总数
                var totalnum = db.Select<VillageTransferPerson>(w => w.adcd == f.FirstNonDefault().adcd)
                    .Sum(w => w.HouseholderNum);
                List<postInfo> _posts = new List<postInfo>();
                postInfo pi = null;
                var vgroup = ConfigurationManager.AppSettings["村级工作组"].Split(',');

                var village = ConfigurationManager.AppSettings["村级网格"].Split(',');
                f.ForEach(
                    w =>
                    {
                        //pi = new postInfo {postCode = w.Post};
                        pi = new postInfo { postCode = w.Post };

                        // pi.gridType = w.gridname;
                        if (vgroup.Contains(w.Post)) pi.postTypecode = "村级工作组";
                        if (village.Contains(w.Post)) pi.postTypecode = "村级网格";
                        if (_posts.Count(x => x.postCode == pi.postCode && x.postTypecode == pi.postTypecode) == 0)
                        {
                            pi.transferNum = pi.postCode == "人员转移组" ? totalnum : 0;

                            _posts.Add(pi);
                        }
                    });
                _model.Posts = _posts;
            }

            return _model;
        }

        /// <summary>
        /// 问题提交
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppPostSignAndFile(AppPostSignAndFile request)
        {
            BaseResult br = new BaseResult();

            using (var db = this.DbFactory.Open())
            {
                var eventModel = db.Single<AppWarnEvent>(
                    "select b.EndTime, b.EventName, b.Id from AppSendMessage a left  join AppWarnEvent b on a.AppWarnEventId = b.id  where a.Id = '" +
                    request.messageId + "'");
                AppVillagePostRecord model = new AppVillagePostRecord();
                model.Id = Guid.NewGuid().ToString();
                model.MessageId = request.messageId;
                model.adcd = request.adcd;
                model.username = request.username;
                model.postTypecode = request.postTypecode;
                model.postCode = request.postCode;
                model.gridType = request.gridType;
                model.step = request.step;
                model.values = request.values;
                model.location = request.location;
                model.stepItem = request.stepItem;
                model.valuesItem = request.valuesItem;
                model.IsResumption = true;
                var _files = System.Web.HttpContext.Current.Request.Files;
                if (_files.Count > 0 && _files[0].ContentLength > 0)
                {
                    var file = _files[0];
                    var fileinfo = new FileInfo(_files[0].FileName);
                    string fileExt = fileinfo.Extension;
                    // 只能上传图片，过滤不可上传的文件类型
                    if (fileExt != ".zip")
                    {
                        br.IsSuccess = false;
                        br.ErrorMsg = "上传文件格式错误！";
                        return br;
                    }
                    else
                    {
                        var userinfo =
                            db.Single<ADCDInfo>(w => w.adcd == request.adcd.Substring(0, 9) + "000000");
                        Random r = new Random();

                        var filepahtname = userinfo != null
                            ? userinfo.adnm + "-" + userinfo.adcd
                            : DateTime.Now.ToString("yyyyMMddHHmmssfff")
                              + r.Next(100, 999).ToString();
                        var fileFolde =
                            System.Web.HttpContext.Current.Server.MapPath("/UploadApp/" + filepahtname);
                        if (!Directory.Exists(fileFolde))
                        {
                            Directory.CreateDirectory(fileFolde);
                        }

                        string guidFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                                              + r.Next(100, 999).ToString() + fileExt.ToLower();
                        file.SaveAs(fileFolde + "\\" + guidFileName);
                        UnZipFloClass uzfc = new UnZipFloClass();
                        model.fileName = request.fileName;

                        model.file = uzfc.unZipFile(
                            fileFolde + "\\" + guidFileName,
                            fileFolde,
                            "/UploadApp/" + filepahtname);
                    }
                }
                //事件没有关闭的情况
                if (eventModel == null)
                {
                    br.IsSuccess = false;
                    br.ErrorMsg = "没有该消息Id";
                    return br;
                }
                if (eventModel.EndTime == null)
                {
                    model.ifFillIn = false;
                    model.addtime = DateTime.Now;
                    br.IsSuccess = db.Insert<AppVillagePostRecord>(model) == 1 ? true : false;
                    var singleModel = db.Single<AppVillagePostRecord>(
                        "select * from dbo.AppVillagePostRecord where MessageId='" + request.messageId +
                        "'and addtime = (select MAX(addtime) from dbo.AppVillagePostRecord where MessageId = '" +
                        request.messageId + "')");
                    if (singleModel != null)
                    {
                        br.QusetionEndTime = singleModel.addtime.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    if (!br.IsSuccess)
                    {
                        br.ErrorMsg = "问题提交失败";
                    }
                }
                else
                {
                    model.addtime = DateTime.Now;
                    model.ifFillIn = true;
                    br.IsSuccess = db.Insert<AppVillagePostRecord>(model) == 1 ? true : false;
                    var singleModel = db.Single<AppVillagePostRecord>(
                        "select * from dbo.AppVillagePostRecord where MessageId='" + request.messageId +
                        "'and addtime = (select MAX(addtime) from dbo.AppVillagePostRecord where MessageId = '" +
                        request.messageId + "')");
                    if (singleModel != null)
                    {
                        br.QusetionEndTime = singleModel.addtime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (!br.IsSuccess)
                    {
                        br.ErrorMsg = "问题提交失败";
                    }

                }
                return br;
            }
        }

        /// <summary>
        /// 村级主要负责人发送消息的时候插入消息列表当中
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AddLoactionAndRecord(RouteVillageLoaction request)
        {
            using (var db = DbFactory.Open())
            {
                var messgeModel = db.Single<AppSendMessage>(x => x.Id == request.messageId);

                AppVillagePostRecord model = new AppVillagePostRecord();
                BaseResult result = new BaseResult();
                if (messgeModel != null)
                {
                    //List<AppSendMessage> list = db.SqlList<AppSendMessage>("select * from AppSendMessage  where AppWarnInfoID='" + messgeModel.AppWarnInfoID + "' and Position like '%村级主要负责人%' ");
                    //foreach (AppSendMessage appSend in list)
                    //{
                    //    db.Delete<AppVillagePostRecord>(x => x.MessageId == appSend.Id && x.postCode == "村级主要负责人");
                    //}
                    model.adcd = messgeModel.SendAdcd;
                    model.Id = Guid.NewGuid().ToString();
                    model.MessageId = request.messageId;
                    model.postCode = "村级主要负责人";
                    model.step = "1";
                    model.values = "已履职";
                    model.location = request.location;
                    model.addtime = DateTime.Now;
                    model.ifFillIn = false;
                    model.username = messgeModel.SendMessagePhone;
                    model.IsResumption = true;
                    result.IsSuccess = db.Insert<AppVillagePostRecord>(model) == 1 ? true : false;
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMsg = "没有该信息";
                }
                return result;
            }
        }

        /// <summary>
        /// 问题补充
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppPostFillIn(AppPostFillIn request)
        {
            BaseResult br = new BaseResult();
            using (var db = DbFactory.Open())
            {
                try
                {
                    if (request.qusetionId == null)
                    {
                        br.IsSuccess = false;
                        br.ErrorMsg = "消息id无效！";
                        return br;
                    }
                    AppVillagePostRecord ar = new AppVillagePostRecord()
                    {
                        addtime = DateTime.Now,
                        postTime = request.postTime,
                        location = request.location,
                        values = request.values,
                        ifFillIn = true,
                        IsResumption = true
                    };
                    var _files = System.Web.HttpContext.Current.Request.Files;
                    if (_files.Count > 0 && _files[0].ContentLength > 0)
                    {
                        var file = _files[0];
                        var fileinfo = new FileInfo(_files[0].FileName);
                        string fileExt = fileinfo.Extension;

                        // 只能上传图片，过滤不可上传的文件类型
                        if (fileExt != ".zip")
                        {
                            br.IsSuccess = false;
                            br.ErrorMsg = "上传文件格式错误！";
                            return br;
                        }
                        else
                        {
                            var userinfo =
                                db.Single<ADCDInfo>(w => w.adcd == request.adcd.Substring(0, 9) + "000000");
                            Random r = new Random();

                            var filepahtname = userinfo != null
                                ? userinfo.adnm + "-" + userinfo.adcd.Substring(0, 9)
                                : DateTime.Now.ToString("yyyyMMddHHmmssfff")
                                  + r.Next(100, 999).ToString();
                            var fileFolde = System.Web.HttpContext.Current.Server.MapPath("/UploadApp/" + filepahtname);
                            if (!Directory.Exists(fileFolde))
                            {
                                Directory.CreateDirectory(fileFolde);
                            }

                            string guidFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                                                  + r.Next(100, 999).ToString() + fileExt.ToLower();
                            file.SaveAs(fileFolde + "\\" + guidFileName);
                            UnZipFloClass uzfc = new UnZipFloClass();
                            ar.fileName = request.fileName;

                            ar.file = uzfc.unZipFile(
                                fileFolde + "\\" + guidFileName,
                                fileFolde,
                                "/UploadApp/" + filepahtname);
                        }
                    }

                    br.IsSuccess = db.UpdateNonDefaults(ar,
                                       w => w.adcd == request.adcd && w.username == request.username &&
                                            w.postCode == request.postCode && w.Id == request.qusetionId) > 0
                        ? true
                        : false;
                    if (!br.IsSuccess)
                    {
                        br.ErrorMsg = "问题提交失败";
                    }
                }
                catch (Exception ex)
                {
                    br.IsSuccess = false;
                    br.ErrorMsg = "系统异常:" + ex.Message;
                }

                return br;
            }
        }

        /// <summary>
        /// 通讯录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<MailListViewModel> AppGetMailList(AppGetMailList request)
        {
            List<MailListViewModel> _mlv = null;
            if (string.IsNullOrEmpty(request.token)) return _mlv;
            var _token = request.token;

            using (var db = DbFactory.Open())
            {
                var fcode = db.SqlList<ServiceModel.Common.VillagePerson>(
                    "EXEC AppGetUserInfoAndVCodeByToken @token",
                    //new {token = _token});
                    new { token = _token });
                if (fcode == null) return _mlv;
                _mlv = new List<MailListViewModel>();

                //var f = db.SqlList<ServiceModel.Common.VillagePerson>("EXEC AppGetUserInfoByAdcd @adcd", new {adcd = fcode[0].adcd});
                var f = db.SqlList<ServiceModel.Common.VillagePerson>("EXEC AppGetUserInfoByAdcd @adcd", new { adcd = fcode[0].adcd });
                MailListViewModel mv = null;
                List<PeerUser> luser = null;
                PeerUser u = null;

                var postnamelist = f.Where(w => w.Post != string.Empty).Select(w => w.Post).Distinct().ToList();
                postnamelist.ForEach(
                    w =>
                    {
                        mv = new MailListViewModel();
                        mv.postname = w;
                        var personlist = f.Where(x => x.Post == w).ToList();
                        luser = new List<PeerUser>();
                        personlist.ForEach(
                            x =>
                            {
                                u = new PeerUser();
                                u.adcd = x.adcd;
                                u.RealName = x.name;
                                u.Mobile = x.HandPhone;
                                if (luser.Count(
                                        y => y.adcd == x.adcd && y.RealName == x.name
                                             && y.Mobile == x.HandPhone) == 0)
                                {
                                    luser.Add(u);
                                }
                            });
                        mv.userList = luser;
                        _mlv.Add(mv);
                    });
            }

            return _mlv;
        }

        /// <summary>
        /// </summary>
        /// <param name="code_type">
        /// </param>
        /// <param name="code">
        /// </param>
        /// <returns>
        /// </returns>
        public string DecodeBase64(string code_type, string code)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }

            return decode;
        }

        /// <summary>
        /// 获取提交过的问题
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BsTableDataSource<AppRecordViewModel> GetAppRecord(GetAppRecord request)
        {
            if (string.IsNullOrEmpty(request.token) || string.IsNullOrEmpty(request.postTypecode)
                || string.IsNullOrEmpty(request.postCode))
            {
                throw new Exception("有参数为空！");
            }

            List<AppRecordViewModel> _list = new List<AppRecordViewModel>();
            using (var db = this.DbFactory.Open())
            {
                var bulider = db.From<AppRecord>();

                bulider.Where(
                    w => w.token == request.token && w.postTypecode == request.postTypecode
                         && w.postCode == request.postCode);
                bulider.OrderByDescending(o => o.addtime);
                var count = db.Count(bulider);
                var PageSize = request.PageSize == 0 ? 15 : request.PageSize;

                var PageIndex = request.PageIndex == 0 ? 0 : request.PageIndex * PageSize;
                bulider.Limit(PageIndex, PageSize);
                var list = db.Select<AppRecordViewModel>(bulider);
                //return new BsTableDataSource<AppRecordViewModel> {rows = list, total = count};
                return new BsTableDataSource<AppRecordViewModel> { rows = list, total = count };
            }
        }

        /// <summary>
        /// 村级责任人，是否接到上级防汛通知，选项
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<VillagePersonItemViewModel> GetVillagePersonItem(GetVillagePersonItem request)
        {
            List<VillagePersonItemViewModel> list = new List<VillagePersonItemViewModel>();
            VillagePersonItemViewModel _model = null;
            string[] Names = Enum.GetNames(typeof(Model.Enums.GrassrootsFloodCtrlEnums.AppVillagePersonItem));
            foreach (string Name in Names)
            {
                _model = new VillagePersonItemViewModel();

                //_model.itemId = ((Model.Enums.GrassrootsFloodCtrlEnums.AppVillagePersonItem) Enum.Parse(
                _model.itemId = ((Model.Enums.GrassrootsFloodCtrlEnums.AppVillagePersonItem)Enum.Parse(
                    typeof(Model.Enums.GrassrootsFloodCtrlEnums.AppVillagePersonItem),
                    Name)).ToString("d");
                _model.itemName = Name;
                list.Add(_model);
            }

            return list;
        }

        /// <summary>
        /// 获取事件下拉框列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AppWarnViewModel GetWarnSelectList(RouteGetAppWarnSelect request)
        {
            using (var db = this.DbFactory.Open())
            {
                var whereLamda = db.From<AppWarnEvent>()


                    .Where<AppWarnEvent>(x => x.EndTime == null).And<UserInfo>(y => y.adcd == request.adcd)
                    //.Select<AppWarnEvent>(x => new {x.Id, x.EventName})
                    .Select<AppWarnEvent>(x => new { x.Id, x.EventName })
                    .OrderByDescending<AppWarnEvent>(x => x.StartTime);
                var model = new AppWarnViewModel();
                var list = db.Select<AppWarnEvent>(whereLamda);
                model.WarnList = list;
                model.StateCode = 1;
                model.Message = "返回信息成功！";
                return model;
            }
        }

        //获取镇级发送的列表
        public AppWarnViewModel GetTownGetAppWarnSelect(RouteTownGetAppWarnSelect request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnEvent>(
                    "select distinct b.Id,b.EventName,b.StartTime from AppWarnInfo a left join AppWarnEvent b on a.AppWarnEventId = b.Id left join UserInfo c on c.UserName = b.UserPhone where b.adcd='" +
                    request.adcd + "' and b.EndTime is null order by b.StartTime desc");
                var model = new AppWarnViewModel();
                model.WarnList = list;
                model.StateCode = 1;
                model.Message = "返回信息成功！";
                return model;
            }
        }

        /// <summary>
        /// 获取事件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AppWarnViewModel GetWarntList(RouteGetAppWarnList request)
        {
            using (var db = this.DbFactory.Open())
            {
                string sql = "select a.* from AppWarnEvent a left join UserInfo b on a.UserPhone=b.UserName where 1=1";
                if (request.EventName != null)
                {
                    sql += "  and a.EventName like '%" + request.EventName + "%'";
                }
                if (request.adcd != null)
                {
                    sql += "  and b.adcd like '%" + request.adcd + "%'";
                }
                sql += " order by id desc";
                var list = db.SqlList<AppWarnEvent>(sql);
                var model = new AppWarnViewModel();
                model.WarnList = list;
                model.StateCode = 1;
                model.Message = "返回信息成功！";
                return model;
            }
        }


        /// <summary>
        /// 增加预警信息默认不发送预警信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AddAppWarnEvent(RouteAddAppWarnEvent request)
        {
            using (var db = this.DbFactory.Open())
            {
                AppWarnEvent model = new AppWarnEvent();
                model.EventName = request.EventName;
                model.UserPhone = request.userName;
                model.UserName = db.Single<UserPostionList>(x => x.UserMobile == request.userName).UserName;//db.Single<UserInfo>(x => x.UserName == request.userName).UserRealName;
                model.StartTime = DateTime.Now;
                model.adcd = request.adcd;
                long count = db.Insert<AppWarnEvent>(model);
                BaseResult ret = new BaseResult();

                ret.IsSuccess = count == 1 ? true : false;
                return ret;
            }
        }

        /// <summary>
        /// 发送预警消息和增加预警的事件
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public BaseResult AddAppWarnInfoAndAppSendMessage(RouteAppWarnInfo requset)
        {
            using (var db = this.DbFactory.Open())
            {
                string jgpushUrl = ConfigurationManager.AppSettings["jgpushUrl"].ToString();
                string jgpushtoken = ConfigurationManager.AppSettings["jgpushtoken"].ToString();
                string jgpushmod = ConfigurationManager.AppSettings["jgpushmod"].ToString();
                BaseResult baseReult = new BaseResult();
                var appWarnInfoModel = new AppWarnInfo
                {
                    AppWarnEventId = requset.AppWarnEventId,
                    WarnLevel = requset.WarnLevel != -1
                        ? requset.WarnLevel
                        : (db.Single<AppWarnInfo>(
                            "select WarnLevel from AppWarnInfo where Id in(select MAX(Id) from AppWarnInfo where AppWarnEventId='" +
                            requset.AppWarnEventId + "')")).WarnLevel,
                    WarnMessage = requset.WarnMessage,
                    Remark = requset.Remark
                };
                AppSendMessage appSendMessageModel = new AppSendMessage { AppWarnEventId = requset.AppWarnEventId };
                if (this.AddAppWarnInfo(appWarnInfoModel, requset.adcd))
                {
                    if (requset.WarnLevel == -1)
                    {
                        appSendMessageModel.Warninglevel =
                        (db.Single<AppWarnInfo>(
                            "select WarnLevel from AppWarnInfo where Id in(select MAX(Id) from AppWarnInfo where AppWarnEventId='" +
                            requset.AppWarnEventId + "')")).WarnLevel;
                    }
                    else
                    {
                        appSendMessageModel.Warninglevel = requset.WarnLevel;
                    }

                    appSendMessageModel.AppWarnInfoID = this.GetByAppWarnInfoId(appWarnInfoModel);
                    appSendMessageModel.SendAdcd = requset.adcd;
                    appSendMessageModel.SendMessage = requset.WarnMessage;
                    appSendMessageModel.Remark = requset.Remark;
                    appSendMessageModel.VillageMessage = requset.VillageMessage;

                    string sendPhone = requset.userName;

                    string userName = string.Empty;
                    var userInfoModel = db.Single<UserInfo>(x => x.UserName == requset.userName);
                    if (userInfoModel != null)
                    {
                        userName = userInfoModel.UserRealName;
                    }
                    else
                    {
                        //获取发送者的名字
                        if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                        {
                            userName = db.Single<CountryPerson>(x => x.Phone == requset.userName).UserName;
                        }
                        if (AdcdHelper.GetByAdcdRole(requset.adcd) == "镇级")
                        {
                            userName = db.Single<TownPersonLiable>(x => x.Mobile == requset.userName).Name;
                        }
                        if (AdcdHelper.GetByAdcdRole(requset.adcd) == "村级")
                        {
                            userName = db.Single<VillageWorkingGroup>(x => x.HandPhone == requset.userName)
                                .PersonLiable;
                        }
                    }
                    int count = 0;
                    if (AdcdHelper.GetByAdcdRole(requset.adcd) != "村级")
                    {
                        var userList = this.GetByRoleIdUserList(requset);
                        StringBuilder jgushStr = new StringBuilder();
                        foreach (var item in userList)
                        {
                            jgushStr.Append(item.ReceiveUserPhone + ",");
                        }
                        string mobileStr = jgushStr.ToString().Substring(0, jgushStr.ToString().Length - 1);



                        //推送
                        Dictionary<string, string> postparam = new Dictionary<string, string>();
                        postparam.Add("token", jgpushtoken);
                        postparam.Add("members", mobileStr);
                        postparam.Add("title", "应急消息");
                        postparam.Add("content", appSendMessageModel.SendMessage);
                        postparam.Add("mod", jgpushmod);
                        postparam.Add("param", "");
                        postparam.Add("production", "true");
                        string a = ImitateHttp.PostHttpResponseJson(jgpushUrl, null, postparam);
                        string sql = GetAppSendMessageInserSql(appSendMessageModel, userList, sendPhone, userName);
                        count = this.AddAppSendMessage(sql);
                    }
                    else
                    {
                        var userList = this.GetVillageList(requset);
                        StringBuilder jgushStr = new StringBuilder();
                        foreach (var item in userList)
                        {
                            jgushStr.Append(item.ReceiveUserPhone + ",");
                        }
                        string mobileStr = jgushStr.ToString().Substring(0, jgushStr.ToString().Length - 1);

                        //推送
                        Dictionary<string, string> postparam = new Dictionary<string, string>();
                        postparam.Add("token", jgpushtoken);
                        postparam.Add("members", mobileStr);
                        postparam.Add("title", "应急消息");
                        postparam.Add("content", appSendMessageModel.SendMessage);
                        postparam.Add("mod", jgpushmod);
                        postparam.Add("param", "");
                        postparam.Add("production", "true");
                        string a = ImitateHttp.PostHttpResponseJson(jgpushUrl, null, postparam);

                        string sql =
                            GetValiageAppSendMessageInserSql(appSendMessageModel, userList, sendPhone, userName);
                        count = this.AddAppSendMessage(sql);
                    }

                    if (count > 0)
            
                    {
                        baseReult.IsSuccess = true;
                        baseReult.ErrorMsg = "发送成功";
                    }
                    else
                    {
                        baseReult.IsSuccess = false;
                        baseReult.ErrorMsg = "发送失败,消息表插入失败";
                    }
                }
                else
                {
                    baseReult.IsSuccess = false;
                    baseReult.ErrorMsg = "发送失败,事件插入失败";
                }

                return baseReult;
            }
        }

        public string GetValiageAppSendMessageInserSql(AppSendMessage model,
            List<AppSendMessageAndPostModel> byRoleIdUserList, string userPhone, string userName)
        {
            StringBuilder inserStr = new StringBuilder();
            if (byRoleIdUserList != null)
            {
                foreach (var item in byRoleIdUserList)
                {
                    Guid id = Guid.NewGuid();
                    inserStr.Append(
                        "insert into AppSendMessage(Id,SendMessagePhone, SendMessageByUserName, AppWarnEventId,Warninglevel, SendMessage, ReceiveUserPhone, ReceiveUserName, IsReaded,AppWarnInfoID, ReceiveDateTime, IsClosed, SendAdcd, ReciveAdcd, Position,Remark,VillageMessage)values('" +
                        id.ToString() + "','" + userPhone + "','" + userName + "','" + model.AppWarnEventId + "','" +
                        model.Warninglevel + "','" + model.SendMessage + "','" + item.ReceiveUserPhone + "','" +
                        item.ReceiveUserName + "',0,'" + model.AppWarnInfoID + "','" + DateTime.Now + "',0,'" +
                        model.SendAdcd + "','" + item.ReciveAdcd + "','" + item.Position + "','" + model.Remark +
                        "','" + model.VillageMessage + "');");
                    //if (item.Posts != null)
                    //{
                    //    foreach (string str in item.Posts)
                    //    {
                    //        inserStr.Append(
                    //            "insert into AppVillagePostRecord(MessageId,username,postCode,adcd) values('" +
                    //            id.ToString() + "','" + item.ReceiveUserPhone + "','" + str + "','" + item.ReciveAdcd +
                    //            "');");
                    //    }
                    //}
                }
            }
            return inserStr.ToString();
        }

        /// <summary>
        /// 增加发送预警消息
        /// </summary>
        /// <param name="model">
        /// </param>
        /// <param name="userAdcd">
        /// The user Adcd.
        /// </param>
        /// <returns>
        /// </returns>
        public bool AddAppWarnInfo(AppWarnInfo model, string userAdcd)
        {
            using (var db = this.DbFactory.Open())
            {
                int count = db.UpdateNonDefaults<AppWarnEvent>(
                    //new AppWarnEvent {IsStartWarning = true},
                    new AppWarnEvent { IsStartWarning = true },
                    x => x.Id == model.AppWarnEventId);
                if (count == 1)
                {
                    if (AdcdHelper.GetByAdcdRole(userAdcd) == "县级" || AdcdHelper.GetByAdcdRole(userAdcd) == "镇级" ||
                        AdcdHelper.GetByAdcdRole(userAdcd) == "村级")
                    {
                        return db.Insert<AppWarnInfo>(model) == 1 ? true : false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 根据AppWarnInfo获取插入的ID值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetByAppWarnInfoId(AppWarnInfo model)
        {
            using (var db = this.DbFactory.Open())
            {
                return db.Single<AppWarnInfo>(
                    "select Id from AppWarnInfo where Id = (select MAX(Id) from AppWarnInfo where AppWarnEventId ='"
                    + model.AppWarnEventId + "' and WarnLevel ='" + model.WarnLevel + "' )").Id;
            }
        }

        /// <summary>
        /// 添加到消息表
        /// </summary>
        /// <param name="addList"></param>
        /// <returns></returns>
        public int AddAppSendMessage(string sql)
        {
            using (var db = this.DbFactory.Open())
            {
                try
                {
                    int count = db.ExecuteSql(sql);
                    return count;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 根据UserId
        /// </summary>
        /// <param name="model">
        /// </param>
        /// <param name="byRoleIdUserList">
        /// The by Role Id User List.
        /// </param>
        /// <param name="userPhone">
        /// The user Phone.
        /// </param>
        /// <param name="userName">
        /// The user Name.
        /// </param>
        /// <returns>
        /// </returns>
        public string GetAppSendMessageInserSql(AppSendMessage model, List<AppSendMessage> byRoleIdUserList,
            string userPhone, string userName)
        {
            StringBuilder inserStr = new StringBuilder();
            if (byRoleIdUserList != null)
            {
                foreach (var item in byRoleIdUserList)
                {
                    inserStr.Append(
                        "insert into AppSendMessage(SendMessagePhone, SendMessageByUserName, AppWarnEventId,Warninglevel, SendMessage, ReceiveUserPhone, ReceiveUserName, IsReaded,AppWarnInfoID, ReceiveDateTime, IsClosed, SendAdcd, ReciveAdcd, Position,Remark)values('" +
                        userPhone + "','" + userName + "','" + model.AppWarnEventId + "','" + model.Warninglevel +
                        "','" + model.SendMessage + "','" + item.ReceiveUserPhone + "','" + item.ReceiveUserName +
                        "',0,'" + model.AppWarnInfoID + "','" + DateTime.Now + "',0,'" + model.SendAdcd + "','" +
                        item.ReciveAdcd + "','" + item.Position + "','" + model.Remark + "');");
                }
            }
            return inserStr.ToString();
        }

        /// <summary>
        /// 根据RoleId获取发送消息的人员
        /// </summary>
        /// <param name="requset">
        /// The requset.
        /// </param>
        /// <returns>
        /// </returns>
        public List<AppSendMessage> GetByRoleIdUserList(RouteAppWarnInfo requset)
        {
            using (var db = this.DbFactory.Open())
            {
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                {
                    var userInfo =
                        db.Single<CountryPerson>("select * from CountryPerson where Phone='" + requset.userName + "'");
                    var list = db.SqlList<AppSendMessage>(
                        "select Phone as ReceiveUserPhone, UserName as ReceiveUserName,adcd as ReciveAdcd,Position from CountryPerson where Phone in (select  Phone  from CountryPerson where adcd = '" +
                        requset.adcd + "' and Phone!='' and  Year = '" + DateTime.Now.Year +
                        "' group by Phone having COUNT(1) = 1) group by Phone,UserName,adcd,Position");
                    var sameCountryList = db.SqlList<AppSendMessage>(
                        "select Phone as ReceiveUserPhone, UserName as ReceiveUserName,adcd as ReciveAdcd,Position from CountryPerson where Phone in (select  Phone  from CountryPerson where adcd = '" +
                        requset.adcd + "' and  Year = '" + DateTime.Now.Year +
                        "' and Phone!='' group by Phone having COUNT(1) > 1) group by Phone,UserName,adcd,Position");
                    list.AddRange(MergePositionList(sameCountryList));
                    List<AppSendMessage> townList = new List<AppSendMessage>();
                    if (requset.sendUser == null)
                    {
                        townList = db.SqlList<AppSendMessage>(
                            "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd like '" +
                            requset.adcd.Substring(0, 6) + "%' and Year = '" + DateTime.Now.Year +
                            "' and Mobile!='' group by Mobile having COUNT(1) = 1) group by Mobile,Name,adcd,Position ");
                        var sameTownList = db.SqlList<AppSendMessage>(
                            "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd like '" +
                            requset.adcd.Substring(0, 6) + "%' and Year = '" + DateTime.Now.Year +
                            "' and Mobile!='' group by Mobile having COUNT(1) > 1) group by Mobile,Name,adcd,Position ");
                        list.AddRange(MergePositionList(sameTownList));
                    }
                    else
                    {
                        StringBuilder adcdBuilder = new StringBuilder();
                        foreach (var item in requset.sendUser)
                        {
                            adcdBuilder.Append("'" + item.adcd + "',");
                        }
                        //string  adcdStr= adcdBuilder.ToString().Substring(0, adcdBuilder.Length - 1);
                        string adcdStr = adcdBuilder.ToString().Substring(0, adcdBuilder.Length - 1);
                        townList = db.SqlList<AppSendMessage>(
                            "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd in ( " +
                            adcdStr + ") and Year = '" + DateTime.Now.Year +
                            "' and Mobile!='' group by Mobile having COUNT(1) = 1) group by Mobile,Name,adcd,Position ");
                        var sameTownList = db.SqlList<AppSendMessage>(
                            "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd in (" +
                            adcdStr + ") and Year = '" + DateTime.Now.Year +
                            "' and Mobile!='' group by Mobile having COUNT(1) > 1) group by Mobile,Name,adcd,Position ");
                        list.AddRange(MergePositionList(sameTownList));
                    }
                    if (userInfo == null)
                    {
                        var userModel = db.Single<UserInfo>(x => x.UserName == requset.userName);
                        if (userModel != null)
                        {
                            list.Add(new AppSendMessage
                            {
                                ReceiveUserPhone = userModel.UserName,
                                ReceiveUserName = userModel.UserRealName,
                                ReciveAdcd = userModel.adcd,
                                Position = ""
                            });
                        }
                    }

                    list.AddRange(townList);
                    return list;
                }
                else if (AdcdHelper.GetByAdcdRole(requset.adcd) == "镇级")
                {
                    List<AppSendMessage> list = new List<AppSendMessage>();
                    if (requset.sendUser == null)
                    {
                        list = db.SqlList<AppSendMessage>(
                         "select distinct HandPhone as ReceiveUserPhone, PersonLiable as ReceiveUserName,VillageADCD as ReciveAdcd,Post as Position   from VillageWorkingGroup where VillageADCD like '" +
                         requset.adcd.Substring(0, 9) + "%' and Year ='" + DateTime.Now.Year +
                         "' and  HandPhone!='' and (Post='村级主要负责人' or Post='驻村干部' or Post='乡级包片领导') ");
                    }
                    else
                    {
                        StringBuilder adcdBuilder = new StringBuilder();
                        foreach (var item in requset.sendUser)
                        {
                            //adcdBuilder.Append("'"+item.adcd + "',");
                            adcdBuilder.Append("'" + item.adcd + "',");
                        }

                        string adcdStr = adcdBuilder.ToString().Substring(0, adcdBuilder.Length - 1);
                        list = db.SqlList<AppSendMessage>(
                        // "select distinct HandPhone as ReceiveUserPhone, PersonLiable as ReceiveUserName,VillageADCD as ReciveAdcd,Post as Position   from VillageWorkingGroup where VillageADCD  in  (" +adcdStr + ") and Year ='" + DateTime.Now.Year +
                         "select distinct HandPhone as ReceiveUserPhone, PersonLiable as ReceiveUserName,VillageADCD as ReciveAdcd,Post as Position   from VillageWorkingGroup where VillageADCD  in  (" + adcdStr + ") and Year ='" + DateTime.Now.Year +
                         "' and  HandPhone!='' and (Post='村级主要负责人' or Post='驻村干部' or Post='乡级包片领导') ");
                    }
                    var sendName = db.Single<TownPersonLiable>(x => x.Mobile == requset.userName);
                    var townList = db.SqlList<AppSendMessage>(
                        "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd= '" +
                        //requset.adcd+ "' and Year = '" + DateTime.Now.Year +
                        requset.adcd + "' and Year = '" + DateTime.Now.Year +
                        "' and Mobile!='' group by Mobile having COUNT(1) = 1) group by Mobile,Name,adcd,Position ");
                    var sameTownList = db.SqlList<AppSendMessage>(
                        "select Mobile as ReceiveUserPhone, Name as ReceiveUserName,adcd as ReciveAdcd,Position from TownPersonLiable where Mobile in (select  Mobile  from TownPersonLiable  where adcd = '" +
                        requset.adcd + "' and Year = '" + DateTime.Now.Year +
                        "' and Mobile!='' group by Mobile having COUNT(1) > 1) group by Mobile,Name,adcd,Position ");
                    list.AddRange(MergePositionList(sameTownList));
                    list.AddRange(townList);
                    ////发送的时候自己在发送给自己一条但是不显示//镇级的时候上面以及选择了自己所以不必须增加
                    //list.Add(new AppSendMessage
                    //{
                    //    ReceiveUserPhone = requset.userName,
                    //    ReceiveUserName = sendName.Name,
                    //    Position = "",
                    //    ReciveAdcd = requset.adcd
                    //});

                    return list;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 村级的用户列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public List<AppSendMessageAndPostModel> GetVillageList(RouteAppWarnInfo requset)
        {
            using (var db = DbFactory.Open())
            {
                AppSendMessageAndPostModel _model = null;
                var f = db.SqlList<ServiceModel.Common.VillagePerson>(
                    "EXEC AppGetUserInfoByAdcd @adcd",
                    //new {adcd = requset.adcd});
                    new { adcd = requset.adcd });
                //var f = db.SqlList<ServiceModel.Common.VillagePerson>(
                //    "EXEC AppGetUserInfoByAdcd @adcd",
                //    new {adcd = requset.adcd});

                //var userpositions= db.Select<UserPosition>(x=>x.adcd==requset.adcd && x.Position!=null&&x.Position!="" && x.Position != "驻村干部" && x.Position != "乡级包片领导");
                var userpositions = db.Select<UserPosition>(x => x.adcd == requset.adcd && x.Position != null && x.Position != "" && x.Position != "驻村干部" && x.Position != "乡级包片领导");

                List<AppSendMessageAndPostModel> _list = new List<AppSendMessageAndPostModel>();
                if (requset.sendUser != null)
                {
                    foreach (var item in requset.sendUser)
                    {
                        _model = new AppSendMessageAndPostModel();
                        //var listModel = userpositions.Find(x=>x.Position==item.adnm&&x.Phone==item.adcd);
                        var listModel = userpositions.Find(x => x.Position == item.adnm && x.Phone == item.adcd);
                        _model.ReceiveUserPhone = listModel.Phone;
                        _model.ReceiveUserName = listModel.UserName;
                        _model.ReciveAdcd = listModel.adcd;
                        _model.Position = listModel.Position;
                        _model.Posts = f.FindAll(x => x.Post == item.adnm && x.HandPhone == item.adcd).Select(x => x.Post).Distinct().ToList();
                        _model.Position = listModel.Position;
                        _model.Posts = userpositions.FindAll(x => x.Position == item.adnm && x.Phone == item.adcd).Select(x => x.Position).Distinct().ToList();
                        _list.Add(_model);
                    }
                    return _list;
                }
                if (f == null) throw new Exception("adcd无效，暂无任何村民信息！");
                if (userpositions == null) throw new Exception("adcd无效，暂无任何村民信息！");


                var find_phone_list = userpositions.Select(w => w.Phone).Distinct().ToList();
                find_phone_list.ForEach(w =>
                {
                    _model = new AppSendMessageAndPostModel();
                    var fitems = userpositions.Where(x => x.Phone == w).ToList();
                    _model.ReceiveUserPhone = fitems.FirstOrDefault().Phone;
                    _model.ReceiveUserName = fitems.FirstOrDefault().UserName;
                    _model.ReciveAdcd = fitems.FirstOrDefault().adcd;
                    var postions = fitems.Where(x => x.Position != "").Select(x => x.Position).Distinct().ToArray();
                    _model.Position = postions.Count() > 0 ? string.Join(",", postions) : "";
                    var posts = fitems.Where(x => x.Position != "").Select(x => x.Position).Distinct().ToList();
                    _model.Posts = posts;
                    _list.Add(_model);
                });
                return _list;
            }
        }


        /// <summary>
        /// 合并
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<AppSendMessage> MergePositionList(List<AppSendMessage> sameList)
        {
            var list = new List<AppSendMessage>();
            string postionStr = "";
            for (int i = 0; i < sameList.Count; i++)
            {
                if (i != sameList.Count - 1)
                {
                    if (sameList[i].ReceiveUserPhone == sameList[i + 1].ReceiveUserPhone)
                    {
                        if (i == 0)
                        {
                            postionStr = sameList[i].Position;
                        }
                        postionStr += "," + sameList[i + 1].Position;
                    }
                    else
                    {
                        list.Add(new AppSendMessage
                        {
                            ReceiveUserPhone = sameList[i].ReceiveUserPhone,
                            ReceiveUserName = sameList[i].ReceiveUserName,
                            Position = postionStr,
                            ReciveAdcd = sameList[i].ReciveAdcd
                        });
                        postionStr = sameList[i + 1].Position;
                    }
                }
                else
                {
                    list.Add(new AppSendMessage
                    {
                        ReceiveUserPhone = sameList[i].ReceiveUserPhone,
                        ReceiveUserName = sameList[i].ReceiveUserName,
                        Position = postionStr,
                        ReciveAdcd = sameList[i].ReciveAdcd
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// 获取发送的消息列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppSendMessageModel GetSendCountyAppSendMessage(RouteGetSendCountyAppSendMessage requset)
        {
            using (var db = this.DbFactory.Open())
            {
                List<AppMessageChangeWarnLevelModel> sendList = new List<AppMessageChangeWarnLevelModel>();
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                {
                    // 发送的列表
                    sendList = db
                        .SqlList<AppMessageChangeWarnLevelModel>(
                            "select  a.Id,a.Warninglevel as WarninglevelId,a.AppWarnEventId,b.Warninglevel,a.SendMessage,a.IsReaded,d.adnm+'防指' as SendMessageByUserName,a.AppWarnInfoID,c.EventName as AppWarnEventName,a.ReceiveDateTime,a.IsClosed,a.Remark,d.adnm,SendAdcd from AppSendMessage a left join AppwarnLevel b on a.Warninglevel=b.Id left join AppWarnEvent c on a.AppWarnEventId=c.Id left join ADCDInfo d on a.SendAdcd=d.adcd where  a.SendMessagePhone = a.ReceiveUserPhone and a.SendMessagePhone='"
                            + requset.userName + "' and a.SendAdcd='" + requset.adcd +
                            "' order by a.ReceiveDateTime desc  ").Skip(requset.pageSize * (requset.pageIndex - 1))
                        .Take(requset.pageSize).ToList();
                }
                else
                {
                    // 发送的列表
                    sendList = db
                        .SqlList<AppMessageChangeWarnLevelModel>(
                            "select  a.Id,a.Warninglevel as WarninglevelId,a.AppWarnEventId,b.Warninglevel,a.SendMessage,a.IsReaded,a.SendMessageByUserName+'('+d.adnm+')' as SendMessageByUserName,a.AppWarnInfoID,c.EventName as AppWarnEventName,a.ReceiveDateTime,a.IsClosed,a.Remark,d.adnm,a.SendAdcd from AppSendMessage a left join AppwarnLevel b on a.Warninglevel=b.Id left join AppWarnEvent c on a.AppWarnEventId=c.Id left join ADCDInfo d on a.SendAdcd=d.adcd where  a.SendMessagePhone = a.ReceiveUserPhone and a.SendMessagePhone='"
                            + requset.userName + "' and a.SendAdcd='" + requset.adcd +
                            "' order by a.ReceiveDateTime desc  ").Skip(requset.pageSize * (requset.pageIndex - 1))
                        .Take(requset.pageSize).ToList();

                }


                // 拼装统计的列表
                var sumList = new List<AppMessageModel>();
                foreach (var item in sendList)
                {
                    var model = new AppMessageModel
                    {
                        //SendCount = db.SqlList<AppSendMessage>("select b.* from AppMobileLogin a left join AppSendMessage b on a.userName = b.ReceiveUserPhone  where b.SendAdcd = '"+requset.adcd+"' and b.AppWarnInfoID ="+item.AppWarnInfoID+" ").Count(),
                        SendCount = db.SqlList<AppSendMessage>("select b.* from AppMobileLogin a left join AppSendMessage b on a.userName = b.ReceiveUserPhone  where b.SendAdcd = '" + requset.adcd + "' and b.AppWarnInfoID =" + item.AppWarnInfoID + " ").Count(),
                        NoReadCount = db.SqlList<AppSendMessage>("select b.* from AppMobileLogin a left join AppSendMessage b on a.userName = b.ReceiveUserPhone  where b.SendAdcd = '" + requset.adcd + "' and b.AppWarnInfoID =" + item.AppWarnInfoID + "  and b.IsReaded=0 ").Count(),
                        SendMessage = item.SendMessage,
                        SendMessageByUserName = item.SendMessageByUserName,
                        Warninglevel = item.Warninglevel,
                        Time = item.ReceiveDateTime,
                        Id = item.Id,
                        IsReaded = item.IsReaded,
                        IsClosed = item.IsClosed,
                        WarninglevelId = item.WarninglevelId.ToString(),
                        AppWarnEventId = item.AppWarnInfoID,
                        AppWarnEventName = item.AppWarnEventName,
                        Remark = item.Remark
                    };
                    sumList.Add(model);
                }

                var returnModel =
                    //new AppSendMessageModel {Total = sumList.Count, StateCode = 1, Rows = sumList, Message = "发送成功"};
                    new AppSendMessageModel { Total = sumList.Count, StateCode = 1, Rows = sumList, Message = "发送成功" };
                return returnModel;
            }
        }

        public int UserExistCount(List<AppSendMessage> list)
        {
            using (var db = DbFactory.Open())
            {
                int count = 0;
                var userList = db.SqlList<AppMobileLogin>("select * from AppMobileLogin");

                foreach (var item in list)
                {
                    if (userList.Exists(x => x.userName == item.ReceiveUserPhone))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// 获取接收消息的列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppSendMessageModel GetReceiveCountyAppSendMessage(RouteGetReceiveCountyAppSendMessage requset)
        {
            using (var db = this.DbFactory.Open())
            {
                string sql = string.Empty;
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                {
                    sql =
"select a.Id,b.Warninglevel,a.Warninglevel as WarninglevelId,a.AppWarnEventId,a.SendMessage,a.SendAdcd,a.IsReaded,d.adnm+'防指' as SendMessageByUserName,a.AppWarnInfoID,a.ReceiveDateTime as Time,a.IsClosed,c.EventName as AppWarnEventName,a.Remark,d.adnm from AppSendMessage a left join AppwarnLevel b on a.Warninglevel=b.Id left join AppWarnEvent c on a.AppWarnEventId=c.Id left join ADCDInfo d on a.SendAdcd=d.adcd where a.ReceiveUserPhone='"
+ requset.userName + "' order by a.ReceiveDateTime desc";
                }
                else
                {
                    sql =
"select a.Id,b.Warninglevel,a.Warninglevel as WarninglevelId,a.AppWarnEventId,a.SendMessage,a.SendAdcd,a.IsReaded,a.SendMessageByUserName+'('+d.adnm+')' as SendMessageByUserName,a.AppWarnInfoID,a.ReceiveDateTime as Time,a.IsClosed,c.EventName as AppWarnEventName,a.Remark,d.adnm from AppSendMessage a left join AppwarnLevel b on a.Warninglevel=b.Id left join AppWarnEvent c on a.AppWarnEventId=c.Id left join ADCDInfo d on a.SendAdcd=d.adcd where a.ReceiveUserPhone='"
+ requset.userName + "' order by a.ReceiveDateTime desc";
                }

                var totalCount = db.SqlList<AppMessageModel>(sql).Count();
                var pageList = db.SqlList<AppMessageModel>(sql).Skip(requset.pageSize * (requset.pageIndex - 1))
                    .Take(requset.pageSize).ToList();
                foreach (AppMessageModel item in pageList)
                {
                    if (AdcdHelper.GetByAdcdRole(item.SendAdcd) == "县级")
                    {
                        item.SendMessageByUserName = item.adnm + "防指";
                    }

                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "村级")
                {
                    foreach (var item in pageList)
                    {
                        int sum = 0;
                        List<string> sumList = new List<string>();
                        var messageMode = db.Single<AppSendMessage>(x => x.Id == item.Id);
                        if (messageMode.Position != "" || messageMode.Position != null)
                        {
                            sum = messageMode.Position.Split(',').Length;
                            for (int i = 0; i < messageMode.Position.Split(',').Length; i++)
                            {
                                //if (messageMode.Position.Split(',')[i] == "村级主要负责人")
                                //{
                                //    sum = messageMode.Position.Split(',').Length - 1;
                                //}
                                //if (messageMode.Position.Split(',')[i] == "管理员"|| messageMode.Position.Split(',')[i]=="联络员"|| messageMode.Position.Split(',')[i]=="抢险救援组"|| messageMode.Position.Split(',')[i]=="人员转移组"|| messageMode.Position.Split(',')[i]=="巡查员"|| messageMode.Position.Split(',')[i]=="预警员") {
                                if (messageMode.Position.Split(',')[i] == "管理员" || messageMode.Position.Split(',')[i] == "联络员" || messageMode.Position.Split(',')[i] == "抢险救援组" || messageMode.Position.Split(',')[i] == "人员转移组" || messageMode.Position.Split(',')[i] == "巡查员" || messageMode.Position.Split(',')[i] == "预警员")
                                {
                                    sumList.Add(messageMode.Position.Split(',')[i]);
                                }
                            }
                        }
                        sum = sumList.Count();
                        List<AppVillagePostRecord> list = new List<AppVillagePostRecord>();
                        list = db.SqlList<AppVillagePostRecord>(
                            "select distinct(postCode) from AppVillagePostRecord where MessageId = '" + item.Id +
                            "' and postCode!='村级主要负责人'  ");
                        int endCount = list.Count;
                        //int sum = db
                        //    .SqlList<AppVillagePostRecord>("select * from AppVillagePostRecord where MessageId = '" +
                        //                                   item.Id + "' and postCode!='村级主要负责人' ").Count();
                        //int endCount =
                        //    db.SqlList<AppVillagePostRecord>("select * from AppVillagePostRecord where MessageId = '" +
                        //                                     item.Id + "' and IsResumption=1 and postCode!='村级主要负责人' ").Count();
                        item.Percent = endCount + "/" + sum;
                    }
                }
                var model = new AppSendMessageModel();
                model.Total = totalCount;
                model.Rows = pageList;
                model.StateCode = 1;
                model.Message = "发送成功";
                model.NoReadSumCount = db.Select<AppSendMessage>(x =>
                    x.ReceiveUserPhone == requset.userName && x.IsReaded == false && x.IsClosed == false).Count();
                return model;
            }
        }

        /// <summary>
        /// 根据消息的Id更状态
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public BaseResult UpdateAppSendMessage(RouteUpdateAppSendMessage requset)
        {
            using (var db = DbFactory.Open())
            {
                BaseResult baseResult = new BaseResult();
                baseResult.IsSuccess = true;
                var checkModel = db.Single<AppSendMessage>(x => x.Id == requset.id);
                if (checkModel != null)
                {
                    if (checkModel.ReceiveUserPhone != requset.userName)
                    {
                        baseResult.IsSuccess = false;
                        baseResult.ErrorMsg = "接收人和接收人Id不一致不能更改消息状态!";
                        return baseResult;
                    }
                }
                else
                {
                    baseResult.IsSuccess = false;
                    baseResult.ErrorMsg = "消息不存在";
                    return baseResult;
                }
                int count = db.UpdateNonDefaults<AppSendMessage>(                    
                    new AppSendMessage { IsReaded = true, UserReadTime = DateTime.Now },
                    x => x.Id == requset.id && x.ReceiveUserPhone == requset.userName);
                baseResult.IsSuccess = count == 1 ? true : false;
                if (count == 1) baseResult.ErrorMsg = "更改成功!";
                else baseResult.ErrorMsg = "接收人和接收人Id不一致不能更改消息状态!";

                return baseResult;
            }
        }

        /// <summary>
        /// 根据ID查询发送消息的详情
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppSendMessage GetByIdAppSendMessageDetails(RouteByIdAppSendMessageDetails requset)
        {
            using (var db = DbFactory.Open())
            {
                var model = db.Single<AppSendMessage>(x => x.Id == requset.id);
                return model;
            }
        }

        public AppPersonModel GetByWarnInfoIdPerson(RouteGetByWarnInfoIdPerson requset)
        {
            using (var db = DbFactory.Open())
            {
                //var noReadList = db.SqlList<AppPersonListModel>(
                //    "select ReceiveUserName,Position,ReceiveUserPhone from AppSendMessage where AppWarnInfoID = '" +
                //    requset.AppWarnInfoID
                //    + "' and IsReaded = 0 ");
                //var readList = db.SqlList<AppPersonListModel>(
                //    "select ReceiveUserName,Position,ReceiveUserPhone  from AppSendMessage where AppWarnInfoID = '" +
                //    requset.AppWarnInfoID
                //    + "' and IsReaded = 1   ");
                //returnModel.ReadPerson = UserExist(readList);
                //returnModel.NoReadPerson = UserExist(noReadList);
                var noReadList = db.SqlList<AppPersonListModel>(
                    "select a.ReceiveUserName,a.Position,a.ReceiveUserPhone,a.IsReaded from AppSendMessage a join AppMobileLogin b on a.ReceiveUserPhone=b.userName where a.AppWarnInfoID = '" +
                    requset.AppWarnInfoID
                    + "' and a.IsReaded =0 ");
                var readList = db.SqlList<AppPersonListModel>(
                    "select a.ReceiveUserName,a.Position,a.ReceiveUserPhone,a.IsReaded  from AppSendMessage a join AppMobileLogin b on a.ReceiveUserPhone=b.userName where a.AppWarnInfoID = '" +
                    requset.AppWarnInfoID
                    + "' and a.IsReaded =1   ");
                var returnModel = new AppPersonModel();
                returnModel.ReadPerson = readList;
                returnModel.NoReadPerson = noReadList;
                returnModel.StateCode = 1;
                returnModel.Message = "成功";
                return returnModel;
            }
        }

        public List<AppPersonListModel> UserExist(List<AppPersonListModel> list)
        {
            using (var db = DbFactory.Open())
            {
                var existList = new List<AppPersonListModel>();
                var userList = db.SqlList<AppMobileLogin>("select * from AppMobileLogin");

                foreach (var item in list)
                {
                    if (userList.Exists(x => x.userName == item.ReceiveUserPhone))
                    {
                        existList.Add(item);
                    }
                }

                return existList;
            }
        }

        /// <summary>
        /// 关闭整个任务列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public BaseResult GetByWarnInfoIdCloseEvent(RouteGetByWarnInfoIdCloseEvent requset)
        {
            using (var db = DbFactory.Open())
            {
                BaseResult baseReturn = new BaseResult();
                int eventId = db.Single<AppWarnInfo>(x => x.Id == requset.AppWarnInfoID).AppWarnEventId;
                var eventModel = db.Single<AppWarnEvent>(x => x.Id == eventId);
                if (eventModel != null)
                {
                    var userNameStr = eventModel.UserPhone;
                    if (userNameStr != requset.userName)
                    {
                        baseReturn.IsSuccess = false;
                        baseReturn.ErrorMsg = "没有权限!";
                        return baseReturn;
                    }
                }

                if (eventId > 0)
                {
                    int updateCount = db.ExecuteSql(
                        "update AppSendMessage set IsClosed = 1 where AppWarnEventId ='" + eventId + "'");
                    if (updateCount > 0)
                    {
                        //int count = db.UpdateNonDefaults<AppWarnEvent>(x => x.Id == eventId);
                        int count = db.UpdateNonDefaults<AppWarnEvent>(
                            new AppWarnEvent {EndTime = DateTime.Now, IsStartWarning = true},
                            x => x.Id == eventId);
                        if (count == 1)
                        {
                            baseReturn.IsSuccess = true;
                            baseReturn.ErrorMsg = "关闭成功";
                        }
                    }
                }
                else
                {
                    baseReturn.IsSuccess = false;
                    baseReturn.ErrorMsg = "没有找见ID";
                }

                return baseReturn;
            }
        }

        /// <summary>
        /// 获取所有事件的等级
        /// </summary>
        /// <param name="requset">
        /// The requset.
        /// </param>
        /// <returns>
        /// </returns>
        public AppWarnLevelListModel GetAllWarnLevel(RouteGetEventLevel requset)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.Select<AppWarnLevel>(x => x.Id > 0);
                var model = new AppWarnLevelListModel();
                model.StateCode = 1;
                model.Message = "成功";
                model.WarnLevelList = list;
                return model;
            }
        }

        /// <summary>
        /// 核对用户
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppLoginModel CheckUser(RouteCheckUser requset)
        {
            using (var db = DbFactory.Open())
            {
                AppLoginModel returnModel = new AppLoginModel();
                var checkUser = db.Single<AppMobileLogin>(x => x.userName == requset.userName);
                var getUser = db.SqlList<ServiceModel.Common.VillagePerson>("EXEC AppLoginApp @handphone", new{handphone=requset.userName});
                    
                var userInfo = db.Single<UserInfo>(x => x.UserName == requset.userName);
                if (checkUser == null)
                {
                    if (getUser.Count == 0 && userInfo == null)
                    {
                        returnModel.StatusCode = 2;
                        returnModel.Message = "该用户没有权限登录系统";
                        returnModel.ExistUser = false;
                    }
                    else if (getUser.Count == 0 && userInfo != null)
                    {
                        if (AdcdHelper.GetByAdcdRole(userInfo.adcd) == "县级")
                        {
                            long count =
                                db.Insert<AppMobileLogin>(
                                    new AppMobileLogin
                                    {
                                        adddtime = DateTime.Now,
                                        token = requset.token,
                                        userName = requset.userName,
                                        adcd = userInfo.adcd
                                    });
                            returnModel.ExistUser = true;
                            returnModel.StatusCode = 1;
                            returnModel.Message = "登录成功";
                            returnModel.Token = requset.token;
                            returnModel.ActionName = "县级";
                            returnModel.IsSend = true;
                            returnModel.Adcd = userInfo.adcd;
                        }
                        else
                        {
                            returnModel.StatusCode = 2;
                            returnModel.Message = "该用户没有权限登录系统";
                            returnModel.ExistUser = false;
                        }
                    }
                    else
                    {
                        long count =
                            db.Insert<AppMobileLogin>(
                                new AppMobileLogin
                                {
                                    adddtime = DateTime.Now,
                                    token = requset.token,
                                    userName = requset.userName,
                                    adcd = getUser[0].adcd
                                });
                        returnModel.ExistUser = true;
                        returnModel.StatusCode = 1;
                        returnModel.Message = "登录成功";
                        returnModel.Token = requset.token;
                        if (userInfo != null && AdcdHelper.GetByAdcdRole(userInfo.adcd) == "县级")
                        {
                            returnModel.ActionName = "县级";
                            returnModel.IsSend = true;
                            returnModel.Adcd = userInfo.adcd;
                        }
                        else
                        {
                            returnModel.ActionName = getUser[0].Post;
                            if (returnModel.ActionName == "镇级")
                            {
                                if (getUser[0].Position == "指挥")
                                {
                                    returnModel.IsSend = true;
                                }
                            }
                            else
                            {
                                returnModel.IsSend = false;
                            }
                            returnModel.Adcd = getUser[0].adcd;
                        }
                    }
                }
                else
                {
                    returnModel.ExistUser = true;
                    returnModel.StatusCode = 1;
                    returnModel.Message = "登录成功";
                    returnModel.Token = checkUser.token;
                    if (userInfo != null && AdcdHelper.GetByAdcdRole(userInfo.adcd) == "县级")
                    {
                        returnModel.ActionName = "县级";
                        returnModel.IsSend = true;
                        returnModel.Adcd = userInfo.adcd;
                    }
                    else
                    {
                        returnModel.ActionName = getUser[0].Post;
                        if (returnModel.ActionName == "镇级")
                        {
                            if (getUser[0].Position == "指挥")
                            {
                                returnModel.IsSend = true;
                            }
                        }
                        else
                        {
                            returnModel.IsSend = false;
                        }
                        returnModel.Adcd = getUser[0].adcd;
                    }
                }
                return returnModel;
            }
        }

        /// 上传GPS定位
        ///  <summary>
        /// The create gps record.
        /// </summary>
        /// <param name="appgps">
        /// The appgps.
        /// </param>
        /// <returns>
        /// The<see cref="bool"/>.
        /// </returns>
        public GrassrootsFloodCtrl.ServiceModel.AppApi.AppUserGps CreateGpsRecord(CreateGpsRecord appgps)
        {
            using (var db = this.DbFactory.Open())
            {
                var model = db.SqlList<ServiceModel.AppApi.AppUserGps>(
                    "EXEC CreateGpsRecord @UserName,@Lng,@Lat,@UserAdcd",
                    new {UserName = appgps.UserName, Lng = appgps.Lng, Lat = appgps.Lat, UserAdcd = appgps.UserAdcd});
                return model.FirstNonDefault();
            }
        }

        /// <summary>
        /// The get gps list.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="AppUserGps"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public BsTableDataSource<AppUserGps> GetGpsList(GetGpsList request)
        {
            using (var db = this.DbFactory.Open())
            {
                var model = db.SqlList<ServiceModel.AppApi.AppUserGps>(
                    "EXEC GetGpsList @UserAdcd",
                    //new {UserAdcd = request.AppWarnEventId});
                //return new BsTableDataSource<AppUserGps> {rows = model, total = model.Count};
                    new { UserAdcd = request.AppWarnEventId });
                return new BsTableDataSource<AppUserGps> { rows = model, total = model.Count };
            }
        }

        /// <summary>
        /// The get by id app send message detail.
        /// </summary>
        /// <param name="requset">
        /// The requset.
        /// </param>
        /// <returns>
        /// The <see cref="AppSendDetailsModel"/>.
        /// </returns>
        public AppSendDetailsModel GetByIdAppSendMessageDetail(RouteByIdAppSendMessageDetails requset)
        {
            using (var db = this.DbFactory.Open())
            {
                var model = db.SqlList<AppSendDetailsModel>(
                    "select a.Id, a.SendMessagePhone,a.VillageMessage, a.SendMessageByUserName, b.EventName as AppWarnEventName, c.Warninglevel, a.SendMessage, a.ReceiveUserPhone, a.ReceiveUserName, a.IsReaded, a.UserReadTime, a.AppWarnInfoID, a.ReceiveDateTime, a.IsClosed, a.Remark,a.SendAdcd from AppSendMessage a left join AppWarnEvent b on a.AppWarnEventId = b.Id left join AppWarnLevel c on a.Warninglevel=c.Id where a.Id='"
                    + requset.id + "' ");
                if (AdcdHelper.GetByAdcdRole(model[0].SendAdcd) == "县级")
                {                  
                    var adcdModel = db.Single<ADCDInfo>("select *from ADCDInfo where adcd='" + model[0].SendAdcd + "'");
                    model[0].SendMessageByUserName = adcdModel.adnm + "防指";

                }
                return model?[0];
            }
        }

        public List<AppUserGps> GetGpsListByUserName(GetGpsListByUserName request)
        {
            using (var db = this.DbFactory.Open())
            {
                var model = db.SqlList<ServiceModel.AppApi.AppUserGps>(
                    "EXEC GetGpsListByUserName @UserName",
                    //new {UserName = request.UserName});
                    new { UserName = request.UserName });
                return model;
            }
        }

        /// <summary>
        /// 接收的消息
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppSumSendMessage GetPCReceiveCountyAppSendMessage(RouteGetPCReceiveCountyAppSendMessage requset)
        {
            using (var db = DbFactory.Open())
            {
                var sql =
                    "select a.Id,d.Warninglevel,a.Position,a.SendMessage,a.IsReaded,a.SendMessageByUserName as SendMessageByUserName,a.AppWarnInfoID,a.ReceiveDateTime as Time,a.IsClosed,c.EventName as AppWarnEventName,a.ReceiveUserName,a.Position,a.ReceiveUserPhone from AppSendMessage a left join  ADCDInfo b on a.SendAdcd = b.adcd left join AppWarnEvent c  on a.AppWarnEventId = c.Id left join AppWarnLevel d on a.Warninglevel = d.Id where a.SendMessagePhone != a.ReceiveUserPhone";
                if (requset.AppWarnEventName != null)
                {
                    sql += " and c.EventName like '%" + requset.AppWarnEventName + "%' ";
                }
                if (requset.SendMessage != null)
                {
                    sql += " and a.SendMessage like '%" + requset.SendMessage + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 6) + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "市级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 4) + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "省级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 2) + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "镇级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 9) + "%' ";
                }
                if (requset.startDateTime != null)
                {
                    sql += "  and a.ReceiveDateTime >= '" + requset.startDateTime + "' ";
                }
                if (requset.endDateTime != null)
                {
                    sql += "  and a.ReceiveDateTime <= '" + requset.endDateTime + "' ";
                }
                sql += " order by a.ReceiveDateTime desc ";
                var total = db.SqlList<AppSumSendContent>(sql).Count();
                var pageList = db.SqlList<AppSumSendContent>(sql).Skip(requset.pageSize * (requset.pageIndex - 1))
                    .Take(requset.pageSize).ToList();
                var model = new AppSumSendMessage();
                model.Total = total;
                model.StateCode = 1;
                model.Message = "成功";
                model.Rows = pageList;
                return model;
            }
        }

        /// <summary>
        /// 发出去的消息
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppSumSendMessage GetPCSendCountyAppSendMessage(RouteGetPCSendCountyAppSendMessage requset)
        {
            using (var db = DbFactory.Open())
            {
                var sql =
                    "select a.Id,d.Warninglevel,a.Position,a.SendMessage,a.IsReaded,a.SendMessageByUserName as SendMessageByUserName,a.AppWarnInfoID,a.ReceiveDateTime as Time,a.IsClosed,c.EventName as AppWarnEventName,a.ReceiveUserName,a.Position from AppSendMessage a left join  ADCDInfo b on a.SendAdcd = b.adcd left join AppWarnEvent c  on a.AppWarnEventId = c.Id left join AppWarnLevel d on a.Warninglevel = d.Id where a.SendMessagePhone = a.ReceiveUserPhone";
                if (requset.AppWarnEventName != null)
                {
                    sql += " and c.EventName like '%" + requset.AppWarnEventName + "%' ";
                }
                if (requset.SendMessage != null)
                {
                    sql += " and a.SendMessage like '%" + requset.SendMessage + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "县级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 6) + "%'";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "市级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 4) + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "省级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 2) + "%' ";
                }
                if (AdcdHelper.GetByAdcdRole(requset.adcd) == "镇级")
                {
                    sql += "  and a.SendAdcd like '" + requset.adcd.Substring(0, 9) + "%' ";
                }
                if (requset.startDateTime != null)
                {
                    sql += "  and a.ReceiveDateTime >= '" + requset.startDateTime + "' ";
                }
                if (requset.endDateTime != null)
                {
                    sql += "  and a.ReceiveDateTime <= '" + requset.endDateTime + "' ";
                }
                sql += " order by a.ReceiveDateTime desc ";
                var total = db.SqlList<AppSumSendContent>(sql).Count();
                var model = new AppSumSendMessage();
                model.Total = total;
                model.StateCode = 1;
                model.Message = "成功";
                model.Rows = db.SqlList<AppSumSendContent>(sql).Skip(requset.pageSize * (requset.pageIndex - 1))
                    .Take(requset.pageSize).ToList();
                return model;
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppWarnEventMoel GetSearchWarnEventName(RouteSearchWarnEventName requset)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnEvent>("select * from AppWarnEvent where EventName like '%" +
                                                    requset.WarnEventName + "%'  ");
                var model = new AppWarnEventMoel();
                model.Rows = list;
                model.Total = list.Count();
                model.StateCode = 1;
                model.Message = "成功";
                return model;
            }
        }

        /// <summary>
        /// 模糊查询事件名称
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppWarnEventMoel GetSearchWarnInfoName(RouteSearchWarnInfoName requset)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnInfo>("select * from AppWarnInfo where WarnMessage like '%" +
                                                   requset.WarnInfoName + "%' ");
                var model = new AppWarnEventMoel();
                model.Rows = list;
                model.Total = list.Count();
                model.StateCode = 1;
                model.Message = "成功";
                return model;
            }
        }

        /// <summary>
        /// 获取消息未读的条数
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppMessageRemind GetMessageNoReadCount(RouteGetMessageNoReadCount requset)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppSendMessage>("select * from AppSendMessage where ReceiveUserPhone='" +
                                                      requset.userName + "' and IsClosed!=1 and IsReaded!=1 ");
                AppMessageRemind model = new AppMessageRemind();
                model.StateCode = 1;
                model.Message = "成功";
                model.MessageCount = list.Count();
                return model;
            }
        }

        /// <summary>
        /// 获取正在执行任务的列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppGetByUserExecutingModel GetByUserExecutingList(RouteGetByUserExecutingList requset)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppMessageModel>(
                    "select a.Id,b.Warninglevel,a.SendMessage,a.IsReaded,a.SendMessageByUserName+'('+d.adnm+')' as SendMessageByUserName,a.AppWarnInfoID,a.ReceiveDateTime as Time,a.IsClosed,c.EventName as AppWarnEventName,b.Warninglevel as WarninglevelId,a.AppWarnEventId from AppSendMessage a left join AppwarnLevel b on a.Warninglevel=b.Id left join AppWarnEvent c on a.AppWarnEventId=c.Id left join ADCDInfo d on a.SendAdcd=d.adcd where  a.AppWarnEventId in  (select AppWarnEventId from AppSendMessage where ReciveAdcd = '" +
                    requset.adcd + "' )  and a.ReceiveUserPhone ='" + requset.userName +
                    "' and a.IsClosed!=1  order by a.ReceiveDateTime desc  ");
                var modelList = new List<AppMessageModel>();
                var noReadList = new List<AppMessageModel>();
                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        if (item.IsReaded != false)
                        {
                            modelList.Add(item);
                        }
                        else
                        {
                            noReadList.Add(item);
                        }
                    }
                }
                var model = new AppGetByUserExecutingModel();
                model.ExecutingList = modelList;
                model.NoReadList = noReadList;
                model.Message = "成功";
                model.StateCode = 1;
                return model;
            }
        }

        public List<AppWarnInfo> GetAllWarn(RouteGetWarnListByWarnId reqeust)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnInfo>("exec AppGetUserInfoByWarnId @WarnId",
                   // new {WarnId = reqeust.WarnId});
                    new { WarnId = reqeust.WarnId });
                return list;
            }
        }

        /// <summary>
        /// b/s人员列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppBsPesonModel GetBsByWarnInfoIdPerson(RouteBsGetByWarnInfoIdPerson requset)
        {
            using (var db = DbFactory.Open())
            {
                var count = db.SqlList<AppPersonListModel>(
                    "select ReceiveUserName,Position,ReceiveUserPhone,IsReaded from AppSendMessage where AppWarnInfoID = '" +
                    requset.AppWarnInfoID
                    + "' ").Count;
                var pageList = db.SqlList<AppPersonListModel>(
                    "select ReceiveUserName,Position,ReceiveUserPhone,IsReaded from AppSendMessage where AppWarnInfoID = '" +
                    requset.AppWarnInfoID
                    + "' ").Skip(requset.pageSize * (requset.pageIndex - 1)).Take(requset.pageSize).ToList();
                var returnModel = new AppBsPesonModel();
                returnModel.Rows = pageList;
                returnModel.StateCode = 1;
                returnModel.Total = count;
                returnModel.Message = "成功";
                return returnModel;
            }
        }

        /// <summary>
        /// 镇、村级是否显示转发按钮
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult GetForwardExist(RouteGetForwardExist request)
        {
            using (var db = DbFactory.Open())
            {
                var result = new BaseResult();
                var modle = db.Single<AppSendMessage>(x => x.Id == request.Id);

                // 判断是否已经结束了事件

                var warnevent = db.Single<AppWarnEvent>(x => x.Id == modle.AppWarnEventId);

                //if (warnevent.EndTime != null || warnevent.EndTime.ToString() != "") {
                if (warnevent.EndTime != null || warnevent.EndTime.ToString() != "")
                {
                    result.IsSuccess = false;
                    return result;
                }


                if (request.userName != modle.SendMessagePhone)
                {
                    string adcd = modle.ReciveAdcd;
                    string recivePhone = modle.ReceiveUserPhone;
                    if (AdcdHelper.GetByAdcdRole(adcd) == "镇级")
                    {
                        //var townModle = db.Single<TownPersonLiable>(x => x.Mobile == recivePhone);
                        var townList = db.SqlList<TownPersonLiable>("select * from TownPersonLiable where Mobile='" + recivePhone + "'");
                        bool townSend = false;
                        foreach (var item in townList)
                        {
                            if (item.Position == "指挥")
                            {
                                townSend = true;
                            }
                        }
                        if (townSend)
                        {
                            if (request.actionId > 1)
                            {
                                if (modle.IsForward != true)
                                {
                                   // db.UpdateNonDefaults<AppSendMessage>(new AppSendMessage {IsForward = true},
                                    db.UpdateNonDefaults<AppSendMessage>(new AppSendMessage { IsForward = true },
                                        x => x.Id == request.Id);
                                    result.IsSuccess = true;
                                    return result;
                                }
                            }
                            else
                            {
                                if (modle.IsForward != true)
                                {
                                    result.IsSuccess = true;
                                    return result;
                                }
                            }
                        }
                    }
                    else if (AdcdHelper.GetByAdcdRole(adcd) == "村级")
                    {
                        //var list = db.SqlList<AppSendMessage>("select * from AppSendMessage where AppWarnInfoID='" +modle.AppWarnInfoID + "' and ReciveAdcd='" + modle.ReciveAdcd + "'  and IsForward=0 and Position like '村级主要负责人%'");
                        //这里应该是大于0
                        //if (list.Count <=0)
                        {
                           // var valigeList= db.SqlList<VillageWorkingGroup>("select * from VillageWorkingGroup where HandPhone='"+request.userName+"' ");
                            var valigeList = db.SqlList<VillageWorkingGroup>("select * from VillageWorkingGroup where HandPhone='" + request.userName + "' ");
                            bool sendState = false;
                            foreach (var model in valigeList)
                            {
                                if (model.Post.Trim() == "村级主要负责人")
                                {
                                    sendState = true;
                                }
                            }
                            if (sendState)
                            {
                                //if (AdcdHelper.GetByAdcdRole(modle.SendAdcd) == "村级")
                                //{
                                //    result.IsSuccess = true;
                                //    return result;
                                //}

                                if (request.actionId > 1)
                                {
                                    db.UpdateNonDefaults<AppSendMessage>(new AppSendMessage { IsForward = true },//IsForward是否已经转发
                                        x => x.Id == request.Id);
                                    result.IsSuccess = false;
                                    return result;
                                }
                                else
                                {
                                    if (modle.IsForward == false)//IsForward是是否转发，如转发了则为true，未转发则显示转发标识
                                    {
                                        result.IsSuccess = true;
                                        return result;
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        return result;
                                    }
                                }
                            }
                        }
                    }
                }
                result.IsSuccess = false;
                return result;
            }
        }

        /// <summary>
        /// 新-镇村级获取是否有转发权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult GetForwardPermissions(RouteGetForwardPermissions request)
        {
            using (var db = DbFactory.Open())
            {
                var result = new BaseResult();
                var modle = db.Single<AppSendMessage>(x => x.Id == request.Id);
                if (request.userName != modle.SendMessagePhone)
                {
                    string adcd = modle.ReciveAdcd;
                    string recivePhone = modle.ReceiveUserPhone;
                    if (AdcdHelper.GetByAdcdRole(adcd) == "镇级")
                    {
                        //var townModle = db.Single<TownPersonLiable>(x => x.Mobile == recivePhone);
                        var townList = db.SqlList<TownPersonLiable>("select * from TownPersonLiable where Mobile='" + recivePhone + "'");
                        bool townSend = false;
                        foreach (var item in townList)
                        {
                            if (item.Position == "指挥")
                            {
                                townSend = true;
                            }
                        }
                        if (townSend)
                        {
                            if (modle.IsForward != true)
                            {
                                result.IsSuccess = true;
                                return result;
                            }
                        }
                    }
                    else if (AdcdHelper.GetByAdcdRole(adcd) == "村级")
                    {
                        var list = db.SqlList<AppSendMessage>("select * from AppSendMessage where AppWarnInfoID='" + modle.AppWarnInfoID + "' and ReciveAdcd='" + modle.ReciveAdcd + "'  and IsForward=1 and Position like '村级主要负责人%'");

                        if (list.Count <= 0)
                        {
                            var valigeList = db.SqlList<VillageWorkingGroup>("select * from VillageWorkingGroup where HandPhone='" + request.userName + "' ");
                            bool sendState = false;
                            foreach (var model in valigeList)
                            {
                                if (model.Post.Trim() == "村级主要负责人")
                                {
                                    sendState = true;
                                }
                            }
                            if (sendState)
                            {
                                if (AdcdHelper.GetByAdcdRole(modle.SendAdcd) == "村级")
                                {
                                    result.IsSuccess = false;
                                    return result;
                                }
                                if (modle.IsForward == false)//IsForward是是否转发，如转发了则为true，未转发则显示转发标识
                                {
                                    result.IsSuccess = true;
                                    return result;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    return result;
                                }
                            }
                        }
                    }
                }
                result.IsSuccess = false;
                return result;
            }
        }


        ///用户登录APP返回信息update2
        public AppLoginModel GetAppLoginInfo(RoutePostAppLoginInfo request)
        {
            AppMobileLogin loginModel = null;
            using (var db = DbFactory.Open())
            {
                //AppMobileLogin allUserInfo = CheckUserExist(request);
                List<UserPostionList> allUserInfo = (List<UserPostionList>)CacheHelper.GetCache("UserPostionList");
                if (allUserInfo == null)
                {
                    
                    allUserInfo = db.Select<UserPostionList>(x => x.UserMobile == request.userName).OrderBy(x => x.Adcd).ToList();
                }
                else
                {
                    if (db.Select<UserPostionList>(x => x.UserMobile == request.userName).OrderBy(x => x.Adcd).ToList().Count > 0)
                    {
                        allUserInfo = allUserInfo.Where(x => x.UserMobile == request.userName).OrderBy(x => x.Adcd).ToList();
                    }
                    else
                    {
                        allUserInfo = db.Select<UserPostionList>(x => x.UserMobile == request.userName).OrderBy(x => x.Adcd).ToList();
                    }
                }

                //查看是否在所有用户表中             
                if (allUserInfo.Count<1)
                {
                    return new AppLoginModel
                    {
                        ActionName = "",
                        StatusCode = 2,
                        IsSend = false,
                        Message = "该用户无权限登录系统",
                        Token = request.token,
                        Adcd = "",
                        ExistUser = false,
                        Postion = null
                    };
                }
                else
                {
                    //增加post岗位list
                    List<postInfo> postinfomodel = new List<postInfo>();
                    
                    bool aha = false;
                   
                    foreach (var item in allUserInfo)
                    {                       
                        if (item.IsSend)
                        {
                            aha = true;
                        }
                        var ss = item.UserPestion.Split(',');
                        if (ss != null)
                        {
                            foreach (var abc in ss)
                            {
                                var a = new postInfo { adcd = item.Adcd, adnm = "", gridType = "", postCode = abc, postTypecode = abc.ToString(), transferNum = item.ToatalNum };
                                postinfomodel.Add(a);
                            }
                        }
                        
                        else
                        {

                        }

                    }
                    //判断等级
                    string roleName = AdcdHelper.GetByAdcdRole(allUserInfo.FirstOrDefault().Adcd);
                    //创建返回的model
                    var returnmodel = new AppLoginModel
                    {
                        StatusCode = 1,//1返回成功，2无权限登录
                        ActionName = roleName,//登录等级 县，乡，村
                        Adcd = allUserInfo.FirstOrDefault().Adcd,//地区编码
                        ExistUser = true,//是否存在
                        IsSend = aha,// allUserInfo.FirstOrDefault().IsSend,//是否有权限发送信息
                        Message = "登录成功",//登录成功信息
                        Postion = postinfomodel,//这里是岗位信息
                        Token = request.token//登录过来的token

                    };
                    return returnmodel;
                    // IGradelevelFactory factory = CreateGradelevelFactory(allUserInfo.FirstNonDefault().adcd);
                    // return factory.GetLoginInfo(request, abc, db);


                }

                ////第一次登录
                ////更改原来登录方式由第一次登录以后返回是否注册然后在进行更新
                //if (CheckUserFirstLoginExist(request.userName))
                //{
                //    if (!InserUserInApp(allUserInfo))
                //    {
                //        return new AppLoginModel { StatusCode = 2, Message = "插入信息异常" };
                //    }
                //    loginModel = allUserInfo;
                //}
                //else
                //{
                //    //获取用户信息
                //    loginModel = db.Single<AppMobileLogin>(x => x.userName == request.userName);
                //    //判断AdcdId是否变动如果发生变动就更改
                //    //同步已经更改的数据库信息
                //    if (loginModel.adcdId != allUserInfo.adcdId)
                //    {
                //        //首先删除已注册表人员
                //       bool delState=db.Delete<AppMobileLogin>(x => x.userName == request.userName)==1?true:false;
                //        if (delState)
                //        {
                //            //然后插入已注册表
                //            if (!InserUserInApp(allUserInfo))
                //            {
                //                return new AppLoginModel { StatusCode = 2, Message = "插入信息异常" };
                //            }
                //            loginModel = allUserInfo;
                //        }
                //        else
                //        {
                //            return new AppLoginModel { StatusCode = 2, Message = "删除信息异常" };
                //        }
                //    }
                //}
                //IGradelevelFactory factory = CreateGradelevelFactory(loginModel.adcd);
                //return factory.GetLoginInfo(request, loginModel, db);
            }
        }


        //用户是否第一次登录APP
        public bool CheckUserFirstLoginExist(string userName)
        {
            using (var db = DbFactory.Open())
            {
                var user = db.Single<AppMobileLogin>(x => x.userName == userName);
                return user == null ? true : false;
            }
        }

        //第一次登录APP插入用户
        public bool InserUserInApp(AppMobileLogin model)
        {
            using (var db = DbFactory.Open())
            {
                long count = db.Insert<AppMobileLogin>(model);
                return count == 1 ? true : false;
            }
        }

        //第一次登录判断该用户是否在表里面,权限按照最大往下排列
        public AppMobileLogin CheckUserExist(RoutePostAppLoginInfo request)
        {
            using (var db = DbFactory.Open())
            {
                ////新版本
                //List<ResponsibleAdcdDo> list = db.SqlList<ResponsibleAdcdDo>("select a.AdcdId, b.AdcdCode, a.ResponsiblePersonnelName, a.IsLeader, a.ResponsiblePersonnelMobile from responsiblepersonnel a LEFT JOIN adcdinfo b  on  a.AdcdId = b.AdcdId where a.ResponsiblePersonnelMobile = '" + request.userName + "'");
                //if (list != null)
                //{
                //    //查看是否是拥有转发权限
                //    var leaderList = list.FindAll(x => x.IsLeader == true);
                //    if (leaderList == null)
                //    {
                //        return new AppMobileLogin()
                //        {
                //            adcd = list[0].AdcdCode,
                //            adddtime = DateTime.Now,
                //            token = request.token = request.token,
                //            userName = request.userName,
                //            VerificationCode = "123456",
                //            realName = list[0].ResponsiblePersonnelName,
                //            adcdId = list[0].AdcdId
                //        };
                //    }
                //    else
                //    {
                //        return new AppMobileLogin()
                //        {
                //            adcd = leaderList[0].AdcdCode,
                //            adddtime = DateTime.Now,
                //            token = request.token = request.token,
                //            userName = request.userName,
                //            VerificationCode = "123456",
                //            realName = leaderList[0].ResponsiblePersonnelName,
                //            adcdId = leaderList[0].AdcdId
                //        };
                //    }
                //}
                //else
                //{
                //    return null;
                //}
                //原来版本;
                //var list = db.SqlList<ServiceModel.Common.VillagePerson>(
                //"EXEC AllUserLoginApp @handphone",
                //new { handphone = request.userName });


                var list = db.Select<UserPosition>(x => x.Phone == request.userName).OrderBy(x => x.adcd).ToList();
                if (list.Count() > 0)
                {
                    //int adcdId = 0;
                    ////增加是否是镇级指挥和村级主要负责人的判断
                    //var townModel = db.Single<TownPersonLiable>(x => x.Mobile == request.userName);
                    //var villageModel = db.Single<VillageWorkingGroup>(x => x.HandPhone == request.userName);
                    //var adcdModel = db.Single<ADCDInfo>(x => x.adcd == list[0].adcd);
                    ////镇级指挥
                    //string adcd = "";
                    //if (townModel != null && townModel.Position == "指挥")
                    //{
                    //    adcd = townModel.adcd;
                    //    adcdId = db.Single<ADCDInfo>(x => x.adcd == townModel.adcd).Id;
                    //}
                    //else if (villageModel != null && villageModel.Post == "村级主要负责人")
                    //{
                    //    adcd = villageModel.VillageADCD;
                    //    adcdId = db.Single<ADCDInfo>(x => x.adcd == villageModel.VillageADCD).Id;
                    //}
                    //else
                    //{
                    //    adcdId = adcdModel.Id;
                    //    adcd = list[0].adcd;
                    //}


                    //判断是否县级管理员、指挥、村级主要负责人
                    //foreach (var item in list) {
                    //    if (item.Position == "县级管理员") { }
                    //    if (item.Position == "指挥") { }
                    //    if (item.Position == "村级主要负责人") { }
                    //}



                    return new AppMobileLogin()
                    {
                        adcd = list.FirstOrDefault().adcd,
                        adddtime = DateTime.Now,
                        token = request.token = request.token,
                        userName = request.userName,
                        VerificationCode = "123456",
                        realName = list.FirstOrDefault().UserName,
                        adcdId = 0 //adcdId 这边是原版的
                    };
                }
                else
                    return null;
            }
        }

        public IGradelevelFactory CreateGradelevelFactory(string adcd)
        {
            string roleName = AdcdHelper.GetByAdcdRole(adcd);
            switch (roleName)
            {
                case "省级":
                    return new ProvinceFactory();

                case "市级":
                    return new CityFactory();

                case "县级":
                    return new CountryFactory();

                case "镇级":
                    return new TownFactory();

                case "村级":
                    return new VillageFactory();

                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// 问题列表
        /// </summary>
        /// <param name="requset"></param>
        /// <returns></returns>
        public AppQusetionModel GetQusertionList(RouteGetQuestionList requset)
        {
            using (var db = DbFactory.Open())
            {
                var model = db.Single<AppSendMessage>(x => x.Id == requset.id);

                List<AppQuestionDescription> list = new List<AppQuestionDescription>();
                if (model != null)
                {
                    if (AdcdHelper.GetByAdcdRole(model.ReciveAdcd) == "村级")
                    {
                        string postion = model.Position;
                        if (postion != "" || postion != null)
                        {
                            string[] arry = postion.Split(',');
                            for (int i = 0; i < arry.Length; i++)
                            {
                                //if (arry[i] == "管理员" || arry[i] == "联络员" || arry[i] == "抢险救援组" || arry[i] == "人员转移组"|| arry[i] == "巡查员"|| arry[i] == "预警员")
                                if (arry[i] == "管理员" || arry[i] == "联络员" || arry[i] == "抢险救援组" || arry[i] == "人员转移组" || arry[i] == "巡查员" || arry[i] == "预警员")
                                {
                                    AppQuestionDescription item = new AppQuestionDescription();
                                    item.messageId = model.Id;
                                    item.GroupName = arry[i];
                                    //var singleModel = db.Single<AppVillagePostRecord>("select * from dbo.AppVillagePostRecord where MessageId='" + requset.id + "' and  postCode='"+arry[i]+"' and addtime = (select MAX(addtime) from dbo.AppVillagePostRecord where MessageId = '" + requset.id + "' and  postCode='" + arry[i] + "')");
                                    var singleModel = db.Single<AppVillagePostRecord>("select * from dbo.AppVillagePostRecord where MessageId='" + requset.id + "' and  postCode='" + arry[i] + "' and addtime = (select MAX(addtime) from dbo.AppVillagePostRecord where MessageId = '" + requset.id + "' and  postCode='" + arry[i] + "')");
                                    if (singleModel != null)
                                    {
                                        item.EndTime = singleModel.addtime;
                                        item.IsResumption = singleModel.IsResumption;
                                    }
                                    else
                                    {
                                        item.EndTime = null;
                                        item.IsResumption = false;
                                    }

                                    list.Add(item);
                                }

                            }
                        }
                    }

                }
                return new AppQusetionModel { StateCode = 1, Message = "返回信息成功", QusetionList = list };
            }

        }

        public AppUserGpsViewModel AppNewGpsGuiJi(AppNewGpsGuiJi request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppUserGpsViewModel>("exec AppNewGpsGuiJi @username",
                    //new {username = request.UserName});
                    new { username = request.UserName });
                return list[0];
            }
        }

        public List<AppXiaoshijian> AppGetWarnEventNext(RouteGetWarnEventNext request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppXiaoshijian>("exec AppGetWarnInfoByWarnEventId @warneventid,@villageadcd",
                    //new {warneventid = request.WarnEventId, villageadcd = request.villageadcd});
                    new { warneventid = request.WarnEventId, villageadcd = request.villageadcd });
                return list;
            }
        }

        //通过adcd获取注册人数
        public int AppGetRegCount(AppGetRegCount request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnEventNextModel>("exec AppGetRegCount @useradcd,@grade",
                    //new {useradcd = request.adcd, grade = request.grad});
                    new { useradcd = request.adcd, grade = request.grad });
                return list.Count;
            }
        }

        //通过县级adcd获取app的下面镇级注册人数
        public int AppGetRegCountByCountyAdcdForTown(AppGetRegCountByCountyAdcdForTown request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppGetRegCountByCountyAdcdForTown>(
                    "exec AppGetRegCountByCountyadcdForTown @useradcd",
                    //new {useradcd = request.adcd});
                    new { useradcd = request.adcd });
                return list.Count;
            }
        }

        //通过县级adcd获取app的下面村级注册人数
        public int AppGetRegCountByCountyAdcdForVillage(AppGetRegCountByCountyAdcdForVillage request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppGetRegCountByCountyAdcdForVillage>(
                    "exec AppGetRegCountByCountyadcdForVillage @useradcd",
                   // new {useradcd = request.adcd});
                    new { useradcd = request.adcd });
                return list.Count;
            }
        }

        public List<AppGpsVillageDisplay> AppGpsVillageDisplay(AppGetGpsLocation request)
        {
            List<AppGpsVillageDisplay> list2 = new List<ServiceModel.AppApi.AppGpsVillageDisplay>();
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppGpsVillageDisplay>("exec AppGetGpsLocation @adcd,@warninfoid",
                    //new {adcd = request.villageadcd, warninfoid = request.warninfoid});
                    new { adcd = request.villageadcd, warninfoid = request.warninfoid });
                foreach (var item in list)
                {
                }

                return list;
            }
        }

        public List<AppWarnEventMoel2> GetWarnListByVillageAdcd(RouteGetWarnListByVillageAdcd request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppWarnEventMoel2>("exec AppGetWarnListByVillageAdcd @adcd",
                    //new {adcd = request.adcd});
                    new { adcd = request.adcd });
                return list;
            }
        }

        //获取岗位和履职信息
        public List<AppLvZhi> GetPostByUserName(AppGetPostByUserName request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppLvZhi>("exec AppGetPostByUserName @username,@xiaoshijian,@dashijian",
                    new
                    {
                        username = request.username,
                        xiaoshijian = request.xiaoshijian,
                        dashijian = request.dashijian
                    });
                return list;
            }
        }

        //县级增加信息
        public int CountySendMessage(AppCountySendMessage request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<int>("exec AppCountySendMessage @AppWarnEventId,@WarnLevel,@WarnMessage,@Remark",
                    new
                    {
                        AppWarnEventId = request.AppWarnEventId,
                        WarnLevel = request.WarnLevel,
                        WarnMessage = request.WarnMessage,
                        Remark = request.Remark
                    });
                return list.Count();
            }
        }

        //履职地图上显示岗位信息
        public List<AppPostcode> GetAppPostcodeOnLvZhi(AppGetPostcode request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppPostcode>("exec AppLvzhiMapGetPost @username,@warninfoid",
                    //new {username = request.username, warninfoid = request.warninfoid});
                    new { username = request.username, warninfoid = request.warninfoid });
                return list;
            }
        }

        //履职在地图上显示坐标点
        public List<AppLocation> GetLocationOnLvZhi(AppGetLocationOnLvZhi request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppLocation>("exec AppGetLvZhiPostionByUserName @username,@warninfoid,@postcode",
                    //new {username = request.username, warninfoid = request.warninfoid, postcode= request.postcode});
                    new { username = request.username, warninfoid = request.warninfoid, postcode = request.postcode });
                return list;
            }
        }

        //履职记录
        public List<AppTaskRecord> GetTaskRecordOnLvZhi(AppGetTaskRecordOnLvZhi request)
        {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<AppTaskRecord>("exec AppGetTaskRecord @username,@warninfoid,@postcode",
                    //new {username = request.username, warninfoid = request.warninfoid, postcode = request.postcode});
                    new { username = request.username, warninfoid = request.warninfoid, postcode = request.postcode });
                return list;
            }
        }
        /// <summary>
        /// 根据消息id获取镇级列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<TownSelect> GetByReceiveMessageIdTownList(RouteGetByReceiveMessageIdTownList request)
        {
            using (var db = DbFactory.Open())
            {
                //var list = db.SqlList<TownSelect>("select  adcd, adnm from ADCDInfo where parentId = (select Id from ADCDInfo where adcd = ( select ReciveAdcd from  AppSendMessage where Id = '"+request.ReceiveMessageId+"')); ");
                var list = db.SqlList<TownSelect>("select  adcd, adnm from ADCDInfo where parentId = (select Id from ADCDInfo where adcd = ( select ReciveAdcd from  AppSendMessage where Id = '" + request.ReceiveMessageId + "')); ");
                return list;
            }
        }
        /// <summary>
        /// 根据消息id获取村级列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public VillageGroupSelect GetByReceiveMessageIdVillageList(RouteGetByReceiveMessageIdVillageList request)
        {
            using (var db = DbFactory.Open())
            {
                VillageGroupSelect model = new VillageGroupSelect();
                //var receiveMessageModel = db.Single<AppSendMessage>(x=>x.Id==request.ReceiveMessageId);
                var receiveMessageModel = db.Single<AppSendMessage>(x => x.Id == request.ReceiveMessageId);
                if (receiveMessageModel == null)
                    return null;
                //var list = db.SqlList<Model.Org.VillagePerson>(" exec AppGetUserInfoByAdcd '"+receiveMessageModel.ReciveAdcd+"'");
                var list = db.Select<UserPosition>(x => x.adcd == receiveMessageModel.ReciveAdcd);
                //model.administrator = list.FindAll(x=>x.Position=="管理员");
                //model.connector = list.FindAll(x=>x.Position == "联络员");
                //model.rescueAndRescueTeam = list.FindAll(x=>x.Position == "抢险救援组");
                //model.personnelTransferGroup = list.FindAll(x=>x.Position == "人员转移组");
                //model.inspector = list.FindAll(x=>x.Position == "巡查员");
                //model.warntor = list.FindAll(x=>x.Position == "预警员");
                model.administrator = list.FindAll(x => x.Position == "管理员");
                model.connector = list.FindAll(x => x.Position == "联络员");
                model.rescueAndRescueTeam = list.FindAll(x => x.Position == "抢险救援组");
                model.personnelTransferGroup = list.FindAll(x => x.Position == "人员转移组");
                model.inspector = list.FindAll(x => x.Position == "巡查员");
                model.warntor = list.FindAll(x => x.Position == "预警员");
                return model;
            }
        }

        public VillagePostModel GetByReceiveMessageIdVillagePostList(RouteGetByReceiveMessageIdVillagePostList request)
        {
            //using (var db=DbFactory.Open())
            using (var db = DbFactory.Open())
            {
                VillagePostModel model = new VillagePostModel();
                //var list= db.SqlList<PostVillageUserModel>(" select a.postCode,a.addtime,c.adnm,b.ReceiveUserName,a.addtime,a.location,b.ReceiveUserPhone,a.[file],a.step,a.[values] from dbo.AppVillagePostRecord a left join AppSendMessage b   on a.MessageId = b.Id left join ADCDInfo c on c.adcd = b.ReciveAdcd where b.Id = '"+request.ReceiveMessageId+"' and a.location is not null; ");
                var list = db.SqlList<PostVillageUserModel>(" select a.postCode,a.addtime,c.adnm,b.ReceiveUserName,a.addtime,a.location,b.ReceiveUserPhone,a.[file],a.step,a.[values] from dbo.AppVillagePostRecord a left join AppSendMessage b   on a.MessageId = b.Id left join ADCDInfo c on c.adcd = b.ReciveAdcd where b.Id = '" + request.ReceiveMessageId + "' and a.location is not null; ");
                model.administrator = list.FindAll(x => x.postCode == "管理员");
                model.connector = list.FindAll(x => x.postCode == "联络员");
                model.rescueAndRescueTeam = list.FindAll(x => x.postCode == "抢险救援组");
                model.personnelTransferGroup = list.FindAll(x => x.postCode == "人员转移组");
                model.inspector = list.FindAll(x => x.postCode == "巡查员");
                model.warntor = list.FindAll(x => x.postCode == "预警员");
                return model;

            }
        }

        public List<VillagePostUserModel> GetByWarnInfoIdPostList(RouteGetByWarnInfoIdPostList request)
        {
            using (var db = DbFactory.Open())
            {
                //List<VillagePostUserModel> list = db.SqlList<VillagePostUserModel>(" select Id,ReceiveUserName,ReceiveUserPhone,Position from AppSendMessage  where AppWarnInfoID='"+request.warnInfoId+ "'  and  SendAdcd='"+request.sendAdcd+"';");
                List<VillagePostUserModel> list = db.SqlList<VillagePostUserModel>(" select Id,ReceiveUserName,ReceiveUserPhone,Position from AppSendMessage  where AppWarnInfoID='" + request.warnInfoId + "'  and  SendAdcd='" + request.sendAdcd + "';");
                foreach (var item in list)
                {
                    int sum = 0;
                    List<string> sumList = new List<string>();
                    StringBuilder sumStr = new StringBuilder();
                    if (item.position != "" || item.position != null)
                    {
                        sum = item.position.Split(',').Length;
                        for (int i = 0; i < item.position.Split(',').Length; i++)
                        {
                            if (item.position.Split(',')[i] == "管理员" || item.position.Split(',')[i] == "联络员" || item.position.Split(',')[i] == "抢险救援组" || item.position.Split(',')[i] == "人员转移组" || item.position.Split(',')[i] == "巡查员" || item.position.Split(',')[i] == "预警员")
                            {
                                sumList.Add(item.position.Split(',')[i]);
                                //sumStr.Append(item.position.Split(',')[i]+",");
                                sumStr.Append(item.position.Split(',')[i] + ",");
                            }
                        }
                    }

                    sum = sumList.Count();
                    List<AppVillagePostRecord> villageList = new List<AppVillagePostRecord>();
                    villageList = db.SqlList<AppVillagePostRecord>(
                        "select distinct(postCode) from AppVillagePostRecord where MessageId = '" + item.id +
                        "' and postCode!='村级主要负责人'  ");
                    int endCount = sumList.Count;
                    //item.percent= endCount + "/" + sum;
                    //item.position = sumStr.ToString().Substring(0,sumStr.ToString().Length - 1);
                    item.percent = endCount + "/" + sum;
                    item.position = sumStr.ToString().Substring(0, sumStr.ToString().Length - 1);
                }
                return list;
            }
        }

        public List<TownSelect> GetByAdcdTownOrVillageList(RouteGetByAdcdTownOrVillageList request)
        {
            using (var db = DbFactory.Open())
            {
                //var list = db.SqlList<TownSelect>("select adcd,adnm from ADCDInfo where parentId=(select Id from ADCDInfo where adcd='"+request.adcd+"'); ");
                var list = db.SqlList<TownSelect>("select adcd,adnm from ADCDInfo where parentId=(select Id from ADCDInfo where adcd='" + request.adcd + "'); ");
                return list;
            }
        }

        public string GetMessageCountByMobileAndSinceDate(RouteGetMessageCountByMobileAndSinceDate request)
        {
            //using (var db = DbFactory.Open()) {
            using (var db = DbFactory.Open())
            {
                var list = db.SqlList<string>("select count(1) as messagecount from AppSendMessage where ReceiveUserPhone='" + request.Mobile + "' and IsReaded=0");
                if (list != null)
                {
                    return list.FirstOrDefault()[0].ToString();
                }
                //else {
                else
                {
                    return "0";
                }
            }

        }
        //获取村里人员的手机号码
        public List<AppGetMobileByVillageAdcd> GetMobileByVillageAdcd(RouteGetMobileByVillageAdcd request)
        {
            //using (var db=DbFactory.Open())
            using (var db = DbFactory.Open())
            {
                //var list = db.SqlList<AppGetMobileByVillageAdcd>("select DISTINCT username as username,adcd,phone as mobile from AppAllUserPostion where adcd='" + request.AdcdCode+"'");
                var list = db.SqlList<AppGetMobileByVillageAdcd>("select DISTINCT username as username,adcd,phone as mobile from AppAllUserPostion where adcd='" + request.AdcdCode + "'");

                return list;
            }
        }

        public BsTableDataSource<AppGetAllEmergency> GetAllEmergency(RouteGetAllEmergency request)
        {
            return null;
        }

        public string AddUser(RouteUserPush request)
        {
           using (var db = this.DbFactory.Open())
           {
               var list = db.SqlList<UserSelect>("exec UserPushForProvince @sex,@orgcoding,@idtype,@hobby,@usertype,@officialtype,@city,@country,@loginname,@username,@isCommunist,@email,@useable,@loginpwd,@age,@province,@official,@orderby,@telephone,@encryptiontype",
                               new { sex =request.sex,
                                   orgcoding =request.orgcoding,
                                   idtype =request.idtype,
                                   hobby =request.hobby,
                                   usertype =request.usertype,
                                   officialtype =request.official,
                                   city =request.city,
                                   country =request.country,
                                   loginname =request.loginname,
                                   username =request.username,
                                   isCommunist =request.isCommunist,
                                   email =request.email,
                                   useable =request.useable,
                                   loginpwd =request,
                                   age =request.age,
                                   province =request.province,
                                   official =request.official,
                                   orderby =request.orderby,
                                   telephone =request.telephone,
                                   encryptiontype =request.encryptiontype});
               return "true";
           }
        }
    }
}