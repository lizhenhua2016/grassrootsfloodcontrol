using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Logic
{
    public interface IAppRegPersonUpdate
    {
        BaseResult AppRegPersonSaveOne(AppRegPersonSaveOne request);
        BaseResult AppRegPersonSaveMore(AppRegPersonSaveMore request);
        BaseResult AppRegPersonDelOne(AppRegPersonDelOne request);
        BaseResult AppRegPersonDelMore(AppRegPersonDelMore request);
    }
}
