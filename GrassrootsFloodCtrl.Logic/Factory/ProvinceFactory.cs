using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrassrootsFloodCtrl.Model.AppApi;
using GrassrootsFloodCtrl.ServiceModel.AppApi;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack.OrmLite;
using System.Data;
using GrassrootsFloodCtrl.Model.Sys;
using GrassrootsFloodCtrl.Model.SumAppUser;
using GrassrootsFloodCtrl.Logic.Common;

namespace GrassrootsFloodCtrl.Logic.Factory
{
    public class ProvinceFactory : ManagerBase, IGradelevelFactory
    {
        public AppLoginModel GetLoginInfo(RoutePostAppLoginInfo request, AppMobileLogin model,IDbConnection db)
        {
            var userInfoModel = db.Single<UserInfo>(x => x.UserName == request.userName);
            if (userInfoModel != null)
            {
                return new AppLoginModel
                {
                    ActionName = "省级",
                    StatusCode = 1,
                    IsSend = true,
                    Message = "没有登录应急管理权限",
                    Token = request.token,
                    Adcd = model.adcd,
                    ExistUser = true,
                    Postion = null
                };
            }
            return new AppLoginModel
            {
                ActionName = "省级",
                StatusCode = 1,
                IsSend = true,
                Message = "没有登录应急管理权限",
                Token = request.token,
                Adcd = model.adcd,
                ExistUser = false,
                Postion = null
            };


        }

        public List<SumMessagePersonReadModel> GetMessageReadStateListSum(RouteGetMessageReadStateListSum model, IDbConnection db)
        {
            //县级
            List<SumMessagePersonReadTypeModel> countryMessageList = db.SqlList<SumMessagePersonReadTypeModel>("exec SumCountryMessageReadTable '" + model.warnEventId + "'");
            List<SumMessagePersonReadModel> sendList = db.SqlList<SumMessagePersonReadModel>(" select distinct a.Id,parentId,adnm,adcd as ReciveAdcd,b.SendMessage from ADCDInfo a right join AppSendMessage b on a.adcd = b.SendAdcd and b.SendAdcd = b.ReciveAdcd  where adcd like '" + model.adcd.Substring(0, 6) + "%'  and b.AppWarnEventId = '" + model.warnEventId + "'  order by id");
            List<SumMessagePersonReadModel> resList = db.SqlList<SumMessagePersonReadModel>("select Id,parentId,adnm,adcd as reciveAdcd from ADCDInfo where adcd like '" + model.adcd.Substring(0, 6) + "%' order by id");
            //将排序以后的县级的父亲节点id变为0
            if (resList != null)
            {
                resList[0].parentId = 0;
            }
            int i = 0;
            foreach (var item in resList)
            {

                if (AdcdHelper.GetByAdcdRole(item.reciveAdcd) == "镇级")
                {
                    item.parentId = 0;
                }

                item.noReadCount = countryMessageList.FindAll(x => x.reciveAdcd == item.reciveAdcd && x.readtype == 0).Sum(x => x.messageCount);
                item.readCount = countryMessageList.FindAll(x => x.reciveAdcd == item.reciveAdcd && x.readtype == 1).Sum(x => x.messageCount);
                var sendMessage = sendList.Find(x => x.reciveAdcd == item.reciveAdcd);
                if (sendMessage != null)
                {
                    item.sendMessage = countryMessageList.FindLast(x => x.reciveAdcd == item.reciveAdcd).sendMessage;
                }
                else
                    item.sendMessage = "<span style='color:red'>未进行转发</span>";
                i++;
            }
            return resList;
        }
    }
}
