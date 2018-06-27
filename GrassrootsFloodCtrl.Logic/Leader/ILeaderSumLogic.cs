using GrassrootsFloodCtrl.Model.Leader;
using GrassrootsFloodCtrl.ServiceModel.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Logic.Leader
{
    public interface ILeaderSumLogic
    {
        LeaderSumModel GetLeaderModel(RouteLeader requset);

    }
}
