using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.ServiceModel.Town;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/Supervise/TownLiableInfo", "GET", Summary = "接口：获取镇责任人信息")]
    [Api("镇责任人")]
    public class RouteTownLiableInfo:PageQuery,IReturn<BsTableDataSource<TownLiableInfoResponse>>
    {
        [ApiMember(IsRequired = false, DataType = "string", Description = "adcd编码")]
        public string ReceiveAdcd { get; set; }

        [ApiMember(IsRequired = false, DataType = "string", Description = "消息id")]
        public int? WarnInfoId { get; set; }
    }
}
