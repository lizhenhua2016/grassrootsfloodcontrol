using GrassrootsFloodCtrl.Model.Org;
using GrassrootsFloodCtrl.ServiceModel.AppApi;
using GrassrootsFloodCtrl.ServiceModel.Common;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/AppWarnEvent/AddAppWarnInfo", "POST", Summary = "接口:发送预警信息,发送时候同时更改事件表里的状态另外同时在消息表和记录表里面同时插入数据")]
    [Api("发送预警信息")]
    public class RouteAppWarnInfo: IReturn<BaseResult>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户账号")]
        public string userName { get; set; }
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户adcd")]
        public string adcd { get; set; }
        [ApiMember(IsRequired = true, DataType = "int", Description = "事件Id")]
        public int AppWarnEventId { get; set; }
        [ApiMember(IsRequired = false, DataType = "int", Description = "事件等级")]
        public int? WarnLevel { get; set; }
        [ApiMember(IsRequired = true, DataType = "string", Description = "消息内容")]
        public string WarnMessage { get; set; }
        [ApiMember(IsRequired = false, DataType = "string", Description = "消息备注")]
        public string Remark { get; set; }
        [ApiMember(IsRequired = false, DataType = "string", Description = "村级消息")]
        public string VillageMessage { get; set; }
        [ApiMember(IsRequired = false, DataType = "string", Description = "需要发送的村或者镇或者人")]
        public List<TownSelect> sendUser  { get; set; }

    }
    [Route("/AppWarnEvent/CheckUser", "Post", Summary = "移动对接版登录")]
    [Api("发送预警信息")]
    public class RouteCheckUser : IReturn<AppLoginModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户账号")]
        public string userName { get; set; }
        [ApiMember(IsRequired = true, DataType = "string", Description = "token")]
        public string token { get; set; }
    }
    [Route("/AppWarnEvent/AppCheckAndGetUserLoginInfo", "Get", Summary = "移动对接版登录U2")]
    [Api("发送预警信息")]
    public class RoutePostAppLoginInfo : IReturn<AppLoginModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "用户账号")]
        public string userName { get; set; }
        [ApiMember(IsRequired = true, DataType = "string", Description = "token")]
        public string token { get; set; }
    }

    [Route("/AppWarnEvent/GetQuestionList", "Get", Summary = "问题列表")]
    [Api("发送预警信息")]
    public class RouteGetQuestionList : IReturn<AppLoginModel>
    {
        [ApiMember(IsRequired = true, DataType = "GuId", Description = "消息的ID")]
        public string id { get; set; }
    }

    [Route("/AppWarnEvent/RouteGetWarnEventNext", "post", Summary = "获取小事件列表根据大事件Id")]
    [Api("发送预警信息")]
    public class RouteGetWarnEventNext : IReturn<AppXiaoshijian> {
        [ApiMember(IsRequired =true,DataType ="int",Description ="事件名称")]
        public int WarnEventId { get; set; }
        [ApiMember(IsRequired =true,DataType ="string",Description ="村级adcd")]
        public string villageadcd { get; set; }

    }

    [Route("/AppWarnEvent/GetByReceiveMessageIdTownList", "Get", Summary = "根据收到消息id查询镇级列表")]
    [Api("获取镇级列表")]
    public class RouteGetByReceiveMessageIdTownList : IReturn<List<TownSelect>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "消息id")]
        public string ReceiveMessageId { get; set; }

    }

    [Route("/AppWarnEvent/GetByReceiveMessageIdVillageList", "Get", Summary = "根据收到消息id查询村级列表")]
    [Api("获取村级列表")]
    public class RouteGetByReceiveMessageIdVillageList : IReturn<VillageGroupSelect>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "消息id")]
        public string ReceiveMessageId { get; set; }

    }

    [Route("/AppWarnEvent/GetByReceiveMessageIdVillagePostList", "Get", Summary = "根据收到消息获取履职列表")]
    [Api("获取村级列表")]
    public class RouteGetByReceiveMessageIdVillagePostList : IReturn<VillagePostModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "消息id")]
        public string ReceiveMessageId { get; set; }
    }

    [Route("/AppWarnEvent/GetByWarnInfoIdPostList", "Get", Summary = "根据warnInfoId获取预警事件的列表")]
    [Api("根据warnInfoId获取预警事件的列表")]
    public class RouteGetByWarnInfoIdPostList : IReturn<List<VillagePostUserModel>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "根据warnInfoId获取预警事件的列表")]
        public string warnInfoId { get; set; }
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string sendAdcd { get; set; }

    }
    [Route("/AppWarnEvent/GetByAdcdTownOrVillageList", "Get", Summary = "根据adcd获取镇级列表或者村级列表")]
    [Api("获取镇级列表和村级列表")]
    public class RouteGetByAdcdTownOrVillageList : IReturn<List<TownSelect>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
    }

    [Route("/HelloWorld/HelloWorld", "Get", Summary = "HelloWorld")]
    [Api("HelloWorld")]
    public class HelloWorld : IReturn<string>
    {
    }
}
