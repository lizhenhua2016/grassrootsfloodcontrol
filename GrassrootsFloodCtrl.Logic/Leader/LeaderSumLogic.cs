using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrassrootsFloodCtrl.Model.Leader;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack.OrmLite;
using GrassrootsFloodCtrl.Logic.Supervise;
using GrassrootsFloodCtrl.Logic.StatisAnalysis;

namespace GrassrootsFloodCtrl.Logic.Leader
{
    public class LeaderSumLogic : ManagerBase,ILeaderSumLogic
    {
        public ISuperviseManager supervise { get; set; }
        public IStatisAnalysisManager statisAnalysis { get; set; }
        //领导页面的数据获取
        public LeaderSumModel GetLeaderModel(RouteLeader requset)
        {
            LeaderSumModel result = new LeaderSumModel();
            try
            {
                //行政区划
                Task<AdministrativeDivision> adtask = Task.Factory.StartNew<AdministrativeDivision>(() => { return GetAdministrativeDivision(requset); });
                //基层防汛责任人
                Task<PersionLiableCount> spTask = Task.Factory.StartNew<PersionLiableCount>(() => { return GetPersionLiable(requset); } );
                //危险点统计
                Task<HiddenRiskPoint> pointTask = Task.Factory.StartNew<HiddenRiskPoint>(() => { return GetHiddenRiskPoint(requset); });
                //其他统计
                Task<LeaderSumModel> otherTask = Task.Factory.StartNew<LeaderSumModel>(() => { return GetOtherCount(); } );
                //获取市信息、按照市级统计责任人数、近30天注册人数
                Task<MessageRegistrationInfo> messageTask = Task.Factory.StartNew<MessageRegistrationInfo>(() => { return GetMessageRegistrationInfo(); });
                Task.WaitAll(adtask, spTask, pointTask, otherTask);
                result = otherTask.Result;
                result.administrativeDivision = adtask.Result;
                result.hiddenRiskPoint = pointTask.Result;
                result.persionLiableCount = spTask.Result;
                result.messageRegistration = messageTask.Result;
            }
            catch (Exception ex)
            {
                result.code = 500;
                result.success = false;
                result.msg = ex.ToString();
                return result;
            }
            if (result != null)
            {
                result.code = 200;
                result.success = true;
                result.msg = "返回数据成功";
            }
            return result;
        }
        //行政区划
        private AdministrativeDivision GetAdministrativeDivision(RouteLeader requset)
        {
            SPersonLiable model = new SPersonLiable();
            model.adcd = requset.adcd;
            model.year = requset.year;
            model.grid = 0;
            var admodel = supervise.PersonLiable(model);
            return new AdministrativeDivision() { countryCount = admodel.CountyAll, townCount = admodel.TownAll, villageCount = admodel.VillageAll };
        }
        //基层防汛责任人
        private PersionLiableCount GetPersionLiable(RouteLeader requset)
        {
            StatisticsByPost model = new StatisticsByPost();
            model.year = requset.year;
            model.adcd = requset.adcd;
            var spmodel = statisAnalysis.StatisticsByPost(model);
            if (spmodel != null)
            {
                return new PersionLiableCount() {countryCount= spmodel[0].CountyPLNums,townCount = spmodel[0].TownPLNums,villageCount= spmodel[0].VillagePLNums };
            }
            return null;
            
        }
        //危险点统计
        private HiddenRiskPoint GetHiddenRiskPoint(RouteLeader requset)
        {
            Statistics model = new Statistics();
            HiddenRiskPoint result = new HiddenRiskPoint();
            model.year = requset.year;
            model.adcd = requset.adcd.Substring(0,4);
            model.adcdtype = 5;
            var point = statisAnalysis.Statics(model);
            if (point != null)
            {
                var list = point[0].DisasterPoint.FindAll(x => x.typeid == 1).OrderBy(x => x.nums);
                foreach (var item in list)
                {
                    if (item.typename == "危房")
                        result.dangerousCount = item.nums;
                    if (item.typename == "山洪灾害危险区")
                        result.torrentialFloodCount = item.nums;
                    if (item.typename == "地质灾害点")
                        result.geologyCount = item.nums;
                    if (item.typename == "低洼易涝区")
                        result.lowLyingCount = item.nums;
                    if (item.typename == "屋顶山塘")
                        result.poolCount = item.nums;
                    if (item.typename == "堤防险段")
                        result.dikeCount = item.nums;
                    if (item.typename == "海塘险段")
                        result.seawallCount = item.nums;
                    if (item.typename == "其它")
                        result.otherCount = item.nums;
                }
                result.pointPersonCount = point[0].TransferPersonNums;
                return result;
            }
            return null;    
        }
        //获取形势图、防汛任务、APP安装人数
        private LeaderSumModel GetOtherCount()
        {
            using (var db = DbFactory.Open())
            {
                var dateStr = DateTime.Now.Year+"-1-1";
                List<int> floodCount=db.SqlList<int>("select COUNT(*)  from ADCDDisasterInfo where FXFTRW like '%重%'");
                List<int> picCount = db.SqlList<int>("select COUNT(*) from VillagePic2");
                List<int> appCount = db.SqlList<int>("select  COUNT(*) from AppGetReg");
                List<int> countryCount = db.SqlList<int>("select COUNT(*) from AppWarnEvent where StartTime>'" + dateStr+"' and IsStartWarning=0 ");
                List <int> managementCount = db.SqlList<int>("select COUNT(*) from AppSendMessage where ReceiveDateTime>'" + dateStr + "'");
                List<int> dutyCount = db.SqlList<int>("select COUNT(*) from AppSendMessage where ReceiveDateTime>'" + dateStr + "' and IsReaded=1");
                ManagementCount manageCount = new ManagementCount();
                manageCount.countryCount = countryCount[0];
                manageCount.dutyCount = dutyCount[0];
                manageCount.managementCount = managementCount[0];
                return new LeaderSumModel()
                {
                    appUserCount = appCount[0],
                    villagePicCount = picCount[0],
                    floodCount = floodCount[0],
                    management = manageCount
                };
            }
        }
        //获取市信息、按照市级统计责任人数、近30天注册人数
        private MessageRegistrationInfo GetMessageRegistrationInfo()
        {
            using (var db=DbFactory.Open())
            {
                var startTime = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                var endTime = DateTime.Now.ToString("yyyy-MM-dd");
                var cityList = db.SqlList<string>("select adnm  from ADCDInfo  where grade=1 order by adcd");
                var alluserList = db.SqlList<int>("select COUNT(*) as sumCount from AppAlluserView where adcd is not null group by SUBSTRING(adcd, 0, 5) order by SUBSTRING(adcd, 0, 5)");
                var personList = db.SqlList<int>("select COUNT(*) as personCount  from AppGetReg a left join ADCDInfo b on a.AdcdId = b.Id where a.CreateTime >= '"+startTime+"' and a.CreateTime <= '"+endTime+"' and b.adcd is not null group by SUBSTRING(b.adcd, 0, 5) order by SUBSTRING(b.adcd, 0, 5)");
                return new MessageRegistrationInfo() { cityList = cityList, allList = alluserList, regList = personList };
            }
        }
    }
}
