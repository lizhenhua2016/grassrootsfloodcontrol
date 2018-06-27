using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrassrootsFloodCtrl.ServiceModel.Common;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack.OrmLite;
using GrassrootsFloodCtrl.ServiceModel;
using GrassrootsFloodCtrl.Model.ZZTX;
using ServiceStack;
using System.Configuration;
using GrassrootsFloodCtrl.Model.Sys;

namespace GrassrootsFloodCtrl.Logic
{
    public class LAppRegPersonUpdate : ManagerBase, IAppRegPersonUpdate
    {
        /// <summary>
        /// 删除多条
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppRegPersonDelMore(AppRegPersonDelMore request)
        {
            BaseResult br = new BaseResult();
            using (var db = DbFactory.Open())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    //县/镇
                    if (!string.IsNullOrEmpty(request.countyadcd) || !string.IsNullOrEmpty(request.townadcd))
                    {
                         request.AdcdIds.ForEach(w => {
                            sb.Append("DELETE FROM AppGetReg WHERE Mobile='" + w.phone + "' AND UserName='" + w.username + "' AND AdcdId='" + w.adcdId + "';");
                         });
                        br.IsSuccess = db.ExecuteNonQuery(sb.ToString()) > 0 ? true : false;
                    }
                    else
                    {
                        //村
                        var f = db.SqlList<AppAlluserViewNODistinct>(
                            "EXEC AppAlluserViewNODistinct @villageadcd,@year",
                            new { villageadcd = request.villageadcd, year = request.year });
                            request.AdcdIds.Distinct().ToList().ForEach(w => {
                                var fcount = f.Where(x => x.adcd == w.adcd && x.phone == w.phone && x.userName == w.username).Count();
                                if (fcount == 0)
                                {
                                    sb.Append("DELETE FROM AppGetReg WHERE Mobile='" + w.phone + "' AND UserName='" + w.username + "' AND AdcdId='" + w.adcdId + "';");
                                }
                            });
                        if (String.IsNullOrEmpty(sb.ToString())) { br.IsSuccess = false; return br; }
                        br.IsSuccess = db.ExecuteNonQuery(sb.ToString()) > 0 ? true : false;
                    }
                }
                catch (Exception ex)
                {
                    br.IsSuccess = false; br.ErrorMsg = ex.Message;
                }
                return br;
            }
        }

        /// <summary>
        /// 删除一条
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppRegPersonDelOne(AppRegPersonDelOne request)
        {
            BaseResult br = new BaseResult();
            using (var db = DbFactory.Open())
            {
                try
                {
                    br.IsSuccess= db.Delete<AppGetReg>(w => w.AdcdId == request.adcdid) >0 ?true:false;
                }
                catch (Exception ex)
                {
                    br.IsSuccess = false; br.ErrorMsg = ex.Message;
                }
                return br;
            }
        }

        /// <summary>
        /// 批量导入更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppRegPersonSaveMore(AppRegPersonSaveMore request)
        {
            BaseResult br = new BaseResult();
            using (var db = DbFactory.Open())
            {
                try
                {
                    //取出现有数据
                    var reqList = request.AdcdIds.Select(w => w.adcdId).Distinct().ToArray();
                    var regList = db.Select<AppGetReg>(w => Sql.In(w.AdcdId, reqList));
                    //找出新数据
                    List<AdcdItems> _newlist = new List<AdcdItems>();
                    request.AdcdIds.ForEach(w=> {
                        var f = regList.Where(x => x.Mobile == w.phone && UserName==w.username && x.AdcdId == w.adcdId).ToList();
                        if(f == null || f.Count == 0)
                        {
                            AdcdItems _items = new AdcdItems()
                            {
                                adcdId=w.adcdId,
                                phone=w.phone,
                                username=w.username
                            };
                            _newlist.Add(_items);
                        }
                    });
                    //遍历更新
                    if (_newlist.Count > 0)
                    {
                        _newlist.ForEach(w =>
                        {
                            AppRegPersonSaveOne(new ServiceModel.Route.AppRegPersonSaveOne() { hanphone = w.phone, username = w.username, adcdid = w.adcdId });
                        });
                    }
                }
                catch (Exception ex)
                {
                    br.IsSuccess = false; br.ErrorMsg = ex.Message;
                }
                return br;
            }
        }

        /// <summary>
        /// 新增，修改 一条数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult AppRegPersonSaveOne(AppRegPersonSaveOne request)
        {
            if (string.IsNullOrEmpty(request.hanphone)) throw new Exception("用户不能为空");
            BaseResult br = new BaseResult();
            var webapi = ConfigurationManager.AppSettings["appApi"];
            using (var db = DbFactory.Open())
            {
                try
                {
                    //查询是否存在该记录
                    var findAppRegUser = db.Select<AppGetReg>(w => w.Mobile == request.hanphone && w.UserName == request.username && w.AdcdId == request.adcdid);
                    List<AppAlluserView> list = null;
                    if (findAppRegUser == null || findAppRegUser.Count == 0)
                    {
                        //删除
                        db.Delete<AppGetReg>(w => w.AdcdId == request.adcdid);
                        list = db.Select<AppAlluserView>(w => w.adcdId == request.adcdid.ToString());
                        if (list != null && list.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            list.ForEach(w =>
                            {
                                var model = db.Single<AppUser>(x=>x.Phone==w.phone);
                                if (model != null)
                                {
                                    sb.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime,Operate) values('" + w.phone + "','" + w.userName + "','" + w.adcdId + "',GETDATE(),'1');");
                                }
                                //using (var client = new JsonServiceClient(webapi))
                                //{
                                //    var r = client.Get<AppRegUser>("?username=" + w.phone);
                                //    if (r.type == "error")//app端已注册
                                //    {
                                //        sb.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime,Operate) values('" + w.phone + "','" + w.userName + "','" + w.adcdId + "',GETDATE(),'1');");
                                //    }
                                //}
                            });
                            br.IsSuccess = db.ExecuteNonQuery(sb.ToString()) > 0 ? true : false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    br.IsSuccess = false; br.ErrorMsg = ex.Message;
                }
                return br;
            }
        }
    }
}
