using GrassrootsFloodCtrl.Logic;
using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceInterface
{
   public class AppRegPersonService: ServiceBase
    {
        public IAppRegPersonUpdate _IAppRegPersonUpdate { get; set; }
        public BaseResult POST(AppRegPersonSaveOne request)
        {
            return _IAppRegPersonUpdate.AppRegPersonSaveOne(request);
        }
        public BaseResult POST(AppRegPersonSaveMore request)
        {
            return _IAppRegPersonUpdate.AppRegPersonSaveMore(request);
        }
        public BaseResult POST(AppRegPersonDelOne request)
        {
            return _IAppRegPersonUpdate.AppRegPersonDelOne(request);
        }
        public BaseResult POST(AppRegPersonDelMore request)
        {
            return _IAppRegPersonUpdate.AppRegPersonDelMore(request);
        }
    }
}
