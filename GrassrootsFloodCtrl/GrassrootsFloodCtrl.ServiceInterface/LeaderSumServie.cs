using GrassrootsFloodCtrl.Logic.Leader;
using GrassrootsFloodCtrl.Model.Leader;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceInterface
{
    [Authenticate]
    public class LeaderSumServie : ServiceBase
    {
        public ILeaderSumLogic leader { get; set; }
        public LeaderSumModel Get(RouteLeader requset)
        {
            return leader.GetLeaderModel(requset);
        }
    }
}
