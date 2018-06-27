using GrassrootsFloodCtrl.ServiceModel.Common;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Route
{
    [Route("/AppRegPerson/AppRegPersonSaveOne", "POST", Summary = "更新一条app端注册人员账号")]
    [Api("移动端注册人员管理")]
    public class AppRegPersonSaveOne : AppRegPersonCondition, IReturn<BaseResult>
    {
       
    }
    [Route("/AppRegPerson/AppRegPersonSaveMore", "POST", Summary = "批量更新app端注册人员账号")]
    [Api("移动端注册人员管理")]
    public class AppRegPersonSaveMore : AppRegPersonCondition, IReturn<BaseResult>
    {

    }
    
    [Route("/AppRegPerson/AppRegPersonDelOne", "POST", Summary = "删除一个村/镇/县app端注册人员账号")]
    [Api("移动端注册人员管理")]
    public class AppRegPersonDelOne : AppRegPersonCondition, IReturn<BaseResult>
    {

    }
    [Route("/AppRegPerson/AppRegPersonDelMore", "POST", Summary = "单条或批量删除app端注册人员账号")]
    [Api("移动端注册人员管理")]
    public class AppRegPersonDelMore : AppRegPersonCondition, IReturn<BaseResult>
    {

    }
}
