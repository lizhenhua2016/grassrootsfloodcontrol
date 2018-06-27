using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.Supervise;
using GrassrootsFloodCtrl.Model.ZZTX;
using GrassrootsFloodCtrl.ServiceModel;
using GrassrootsFloodCtrl.ServiceModel.CApp;
using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using GrassrootsFloodCtrl.ServiceModel.Town;

using GrassrootsFloodCtrl.ServiceModel.Supervise;
using GrassrootsFloodCtrl.ServiceModel.ZZTX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Logic.Supervise
{
    public interface ISuperviseManager
    {
        SuperviseModel PersonLiable(SPersonLiable request);
        BsTableDataSource<ADCDDisasterViewModel> GetPersonLiabelList(GetPersonLiabelList request);
        SuperviseModel PersonLiable1(SPersonLiable1 request);
        BsTableDataSource<StatiscPerson> CCKHVillage(CCKHVillage request);
        BaseResult SetCCKH(SetCCKH request);
        BsTableDataSource<SpotCheckModel> GetCCJLTable(GetCCJLTable request);
        BsTableDataSource<SpotCheckOne> GetCCJLTableOne(GetCCJLTableOne request);

        List<AppAreaViewModel> GetCityAppInPostList(GetCityAppInPostList request);

        List<CountyMessageInfo> CountyMessageInfo(RouteCountyMessageInfo request);

        BsTableDataSource<TownLiableInfoResponse> TownLiableList(RouteTownLiableInfo request);


        #region 应急管理

        /// <summary>
        /// 全省应急响应启动市列表.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<EmergencyStartCityListViewModel> EmergencyStartCityList(EmergencyStartCityList request);

        /// <summary>
        /// 县级应急消息列表.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<CountyEmergencyMessageListViewModel> CountyEmergencyMessageList(CountyEmergencyMessageList request);

        /// <summary>
        /// 获取单条应急事件信息.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        GetSingleEmergencyInfoViewModel GetSingleEmergencyInfo(GetSingleEmergencyInfo request);


        /// <summary>
        /// 根据消息id获取各乡镇履职情况.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<GetDutyInfoByWarnInfoIdViewModel> GetDutyInfoByWarnInfoId(GetDutyInfoByWarnInfoId request);

        /// <summary>
        /// 根据消息id获取各村履职情况.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<GetDutyInfoByWarnInfoIdViewModel> GetVillageDutyInfoByWarnInfoId(GetVillageDutyInfoByWarnInfoId request);

        #endregion
    }
}
