using GrassrootsFloodCtrl.Model.Leader;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/sumLeader/getLeaderSumForm", "Get", Summary = "获取领导的统计报表")]
    [Api("领导统计")]
    public class RouteLeader: IReturn<LeaderSumModel>
    {
        [ApiMember(IsRequired = true, DataType = "string", Description = "adcd")]
        public string adcd { get; set; }
        [ApiMember(IsRequired = false, DataType = "int", Description = "年份")]
        public int?  year { get; set; }
    }
}
