using GrassrootsFloodCtrl.Logic.Supervise;
using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.Supervise;
using GrassrootsFloodCtrl.ServiceModel;
using GrassrootsFloodCtrl.ServiceModel.CApp;
using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using GrassrootsFloodCtrl.ServiceModel.Town;
using GrassrootsFloodCtrl.ServiceModel.Supervise;
using GrassrootsFloodCtrl.ServiceModel.ZZTX;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceInterface
{
    [Authenticate]
    public class SuperviseService:ServiceBase
    {
        public ISuperviseManager SuperviseManager { get; set;}
        public SuperviseModel Post(SPersonLiable request)
        {
            return SuperviseManager.PersonLiable(request);
        }
        public SuperviseModel Post(SPersonLiable1 request)
        {
            return SuperviseManager.PersonLiable1(request);
        }
        public BsTableDataSource<ADCDDisasterViewModel> Post(GetPersonLiabelList request)
        {
            return SuperviseManager.GetPersonLiabelList(request);
        }
        public BsTableDataSource<StatiscPerson> Get(CCKHVillage request)
        {
            return SuperviseManager.CCKHVillage(request);
        }
        public BaseResult Post(SetCCKH request)
        {
            return SuperviseManager.SetCCKH(request);
        }

        public BsTableDataSource<SpotCheckModel> Post(GetCCJLTable request)
        {
            return SuperviseManager.GetCCJLTable(request);
        }

        public BsTableDataSource<SpotCheckOne> Post(GetCCJLTableOne request)
        {
            return SuperviseManager.GetCCJLTableOne(request);
        }
        public List<AppAreaViewModel> POST(GetCityAppInPostList request)
        {
            return SuperviseManager.GetCityAppInPostList(request);
        }

        public List<CountyMessageInfo> Get(RouteCountyMessageInfo request)
        {
            return SuperviseManager.CountyMessageInfo(request);
        }

        public BsTableDataSource<TownLiableInfoResponse> Get(RouteTownLiableInfo request)
        {
            return SuperviseManager.TownLiableList(request);
        }

        #region 应急管理

        // 全省应急响应启动市列表

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<EmergencyStartCityListViewModel> Get(EmergencyStartCityList request)
        {
            return SuperviseManager.EmergencyStartCityList(request);
        }

        // 县级应急消息列表

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<CountyEmergencyMessageListViewModel> Get(CountyEmergencyMessageList request)
        {
            return SuperviseManager.CountyEmergencyMessageList(request);
        }

        // 获取单条应急事件信息

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public GetSingleEmergencyInfoViewModel Get(GetSingleEmergencyInfo request)
        {
            return SuperviseManager.GetSingleEmergencyInfo(request);
        }

        // 根据消息id获取各乡镇履职情况

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<GetDutyInfoByWarnInfoIdViewModel> Get(GetDutyInfoByWarnInfoId request)
        {
            return SuperviseManager.GetDutyInfoByWarnInfoId(request);
        }

        // 根据消息id获取各村履职情况

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<GetDutyInfoByWarnInfoIdViewModel> Get(GetVillageDutyInfoByWarnInfoId request)
        {
            return SuperviseManager.GetVillageDutyInfoByWarnInfoId(request);
        }

        #endregion
    }
}
