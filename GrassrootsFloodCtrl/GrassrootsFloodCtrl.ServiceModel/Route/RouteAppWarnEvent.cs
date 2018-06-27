using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.ServiceModel.AppApi;
using GrassrootsFloodCtrl.ServiceModel.Common;
using ServiceStack;
using System;
using System.Collections.Generic;

namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/AppWarnEvent/AddAppWarnEvent", "POST", Summary = "接口:增加事件信息,默认不启动预警")]
    [Api("预警事件")]
    public class RouteAddAppWarnEvent : IReturn<BaseResult>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户账号")]
        public string userName { get; set; }

        [ApiMember(IsRequired = true, DataType = "string", Description = "事件名称")]
        public string EventName { get; set; }

        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
    }

    [Route("/AppWarnEvent/GetWarnSelect", "Get", Summary = "接口:获取改该县要发送预警的列表")]
    [Api("预警事件")]
    public class RouteGetAppWarnSelect : IReturn<AppWarnViewModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户adcd")]
        public string adcd { get; set; }
    }

    [Route("/AppWarnEvent/GetTownWarnSelect", "Get", Summary = "接口:获取镇级要发送预警的列表")]
    [Api("预警事件")]
    public class RouteTownGetAppWarnSelect : IReturn<AppWarnViewModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户adcd")]
        public string adcd { get; set; }
    }

    [Route("/AppWarnEvent/GetWarnList", "Get", Summary = "接口:获取所有事件的列表")]
    [Api("预警事件")]
    public class RouteGetAppWarnList : IReturn<AppWarnViewModel>
    {
        [ApiMember(IsRequired = false, DataType = "string", Description = "用户adcd")]
        public string adcd { get; set; }

        [ApiMember(IsRequired = false, DataType = "string", Description = "事件名称")]
        public string EventName { get; set; }
    }

    [Route("/AppWarnEvent/SearchWarnEventName", "Get", Summary = "模糊查询任务名称")]
    [Api("预警事件")]
    public class RouteSearchWarnEventName : IReturn<AppWarnEventMoel>
    {
        [ApiMember(IsRequired = false, DataType = "string", Description = "任务名称")]
        public string WarnEventName { get; set; }
    }

    [Route("/AppWarnEvent/SearchWarnInfoName", "Get", Summary = "模糊查事件名称")]
    [Api("预警事件")]
    public class RouteSearchWarnInfoName : IReturn<AppWarnEventMoel>
    {
        [ApiMember(IsRequired = false, DataType = "string", Description = "事件名称")]
        public string WarnInfoName { get; set; }
    }

    [Route("/AppWarnEvent/GetMessageNoReadCount", "Get", Summary = "查询消息未读的数量")]
    [Api("预警事件")]
    public class RouteGetMessageNoReadCount : IReturn<AppMessageRemind>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "手机号")]
        public string userName { get; set; }
    }

    [Route("/AppWarnEvent/RouteGetWarnListByWarnId", "post", Summary = "根据事件ID获取全部的预警等级信息")]
    [Api("预警事件")]
    public class RouteGetWarnListByWarnId : IReturn<List<Model.AppApi.AppWarnInfo>>
    {
        [ApiMember(IsRequired = false, DataType = "int", Description = "事件的ID")]
        public int WarnId { get; set; }

        [ApiMember(IsRequired = false, DataType = "string", Description = "发布事件的Adcd")]
        public string UserAdcd { get; set; }
    }

    [Route("/AppWarnEvent/RouteGetWarnListByVillageAdcd", "post", Summary = "点击村弹出应急管理大事件")]
    [Api("预警事件")]
    public class RouteGetWarnListByVillageAdcd : IReturn<AppWarnEventMoel2>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "村adcd")]
        public string adcd { get; set; }
    }

    [Route("/AppWarnEvent/GetMessageCountByMobileAndSinceDate", "Get", Summary = "登录时候获取未读消息的数量")]
    [Api("预警事件")]
    public class RouteGetMessageCountByMobileAndSinceDate : IReturn<string>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "登录手机")]
        public string Mobile { get; set; }
    }

    [Route("/AppWarnEvent/GetMobileByVillageAdcd", "Get", Summary = "通过村Adcd获取村所有的手机号码")]
    [Api("预警事件")]
    public class RouteGetMobileByVillageAdcd : IReturn<List<AppGetMobileByVillageAdcd>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "Adcd")]
        public string AdcdCode { get; set; }
    }


    public class RouteGetAllEmergency : PageQuery, IReturn<BsTableDataSource<AppGetAllEmergency>>
    {
    }

    [Route("/AppWarnEvent/RouteUserPush","Post", Summary = "四个平台用户推送")]
    public class RouteUserPush : IReturn<string>
    {
        //"zhangsan@commnetSoft.com","":"1","":"10E162D07EAA501C8A8A941ED03F76D5","":"1","":"浙江","":"1","":"100688","":"13675838888","":"4"
        public int sex { get; set; }
        public string orgcoding { get; set; }
        public string idtype { get; set; }
        public string hobby { get; set; }
        public int usertype { get; set; }
        public int officialtype { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string loginname { get; set; }
        public string username { get; set; }
        public bool isCommunist { get; set; }
        public string email { get; set; }
        public bool useable { get; set; }
        public string loginpwd { get; set; }
        public string age { get; set; }
        public string province { get; set; }
        public string official { get; set; }
        public string orderby { get; set; }
        public string telephone { get; set; }
        public string encryptiontype { get; set; }
    }

    [Route("/AppWarnEvent/RouteUserSSO","Post",Summary = "sso单点登录")]
    public class RouteUserSSO : IReturn<AppUserSSO>
    {
        
    }
}