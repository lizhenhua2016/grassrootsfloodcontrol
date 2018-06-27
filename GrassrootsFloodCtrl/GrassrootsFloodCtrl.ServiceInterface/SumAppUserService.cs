using GrassrootsFloodCtrl.Logic.SumAppUser;
using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.SumAppUser;
using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceInterface
{

    public class SumAppUserService: ServiceBase
    {
        public ISunAppUserLogic logic { get; set; }
        public List<SelectSumAppUserList> Get(RouteSumAppUser request)
        {
            return logic.GetSelectSumAppUserList(request);
        }
        public List<SelectSumAppUserList> Get(RouteSumAppUser2 request)
        {
            return logic.GetSelectSumAppUserList2(request);
        }
        public BsTableDataSource<Model.SumAppUser.SumAppUser> Get(RouteNoAppUser request)
        {
            return logic.GetSumAppUserList(request);
        }

        public BaseResult Get(RouteCheckAppUser request)
        {
            return logic.CheckUser(request);
        }

        public BaseResult Get(RouteCheckTown request)
        {
            return logic.CheckTown(request);
        }

        public BaseResult Get(RouteCheckTownUser request)
        {
            return logic.CheckTownUser(request);
        }

        public List<SumMessagePersonReadModel> Get(RouteGetMessageReadStateListSum request)
        {
            return logic.GetMessageReadStateListSum(request);
        }
    }
}
