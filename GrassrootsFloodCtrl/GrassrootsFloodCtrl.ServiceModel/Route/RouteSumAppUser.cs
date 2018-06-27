using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.SumAppUser;
using GrassrootsFloodCtrl.ServiceModel.Common;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/sumAppUser/list", "Get", Summary = "获取APP")]
    [Api("统计APP用户")]
    public class RouteSumAppUser:IReturn<List<SelectSumAppUserList>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
        [ApiMember(IsRequired = false, DataType = "string", Description = "adcdName")]
        public string adcdName { get; set; }
    }
    [Route("/sumAppUser/list2", "Get", Summary = "获取APP")]
    [Api("统计APP用户")]
    public class RouteSumAppUser2 : IReturn<List<SelectSumAppUserList>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
        [ApiMember(IsRequired = false, DataType = "string", Description = "adcdName")]
        public string adcdName { get; set; }
    }
    [Route("/sumAppUser/noInfo", "Get", Summary = "获取未注册人的信息")]
    public class RouteNoAppUser : PageQuery,
        IReturn<BsTableDataSource<Model.SumAppUser.SumAppUser>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
    }

    [Route("/sumAppUser/checkUser", "Get", Summary = "核对人员信息并修改人员")]
    public class RouteCheckAppUser :IReturn<BaseResult>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "phone")]
        public string phone { get; set; }
    }
    [Route("/sumAppUser/checkTown", "Get", Summary = "核对镇数据")]
    public class RouteCheckTown : IReturn<BaseResult>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
    }
    [Route("/sumAppUser/checkTownUser", "Get", Summary = "核对镇数据")]
    public class RouteCheckTownUser : IReturn<BaseResult>
    {
    }
    [Route("/sumAppUser/getMessageReadStateListSum", "Get", Summary = "获取消息状态的列表统计")]
    public class RouteGetMessageReadStateListSum : IReturn<List<SumMessagePersonReadModel>>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
        [ApiMember(IsRequired = true, DataType = "int", Description = "adcd")]
        public int warnEventId { get; set; }
    }

}
