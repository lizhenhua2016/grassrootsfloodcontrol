using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dy.Common;
using GrassrootsFloodCtrl.Logic.ZZTX;
using GrassrootsFloodCtrl.Model;
using GrassrootsFloodCtrl.Model.Common;
using GrassrootsFloodCtrl.Model.Village;
using GrassrootsFloodCtrl.Model.ZZTX;
using GrassrootsFloodCtrl.ServiceModel.Route;
using GrassrootsFloodCtrl.ServiceModel.Village;
using ServiceStack.OrmLite;
using ServiceStack.Redis.Pipeline;
using GrassrootsFloodCtrl.Logic.Common;
using static GrassrootsFloodCtrl.Model.Enums.GrassrootsFloodCtrlEnums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace GrassrootsFloodCtrl.Logic.Village
{
    /// <summary>
    /// 行政村形势图
    /// </summary>
    public class VillagePicManager:ManagerBase,IVillagePicManager
    {
        public IZZTXManager ZZTXManager { get; set; }
        public ILogHelper _ILogHelper { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BsTableDataSource<VillageViewModel> GetVillagePicAdcd(GetVillagePicAdcd request)
        {
            using (var db=DbFactory.Open())
            {
                var year = DateTime.Now.Year;
                if (request.year != 0)
                    year = request.year;
                var builder = db.From<ADCDInfo>();
                builder.LeftJoin<ADCDInfo, VillagePic2>((x, y) => x.adcd == y.Adcd);
                builder.Where(x => x.adcd.StartsWith(adcd.Substring(0, 9)) && x.adcd != adcd);
                
                if (!string.IsNullOrEmpty(request.adnm))
                    builder.And(x => x.adnm.Contains(request.adnm));
                if (!string.IsNullOrEmpty(request.adcd))
                    builder.And(x => x.adcd==request.adcd);
                builder.SelectDistinct(x => new { x.Id, x.adcd, x.adnm });
                if (request.type == 0)//0：未上报，1：已上报
                    builder.And<VillagePic2>(y => y.PicName == string.Empty || y.PicName == null);
                else if (request.type == 1)
                    builder.And<VillagePic2>(y => y.PicName != ""&&y.Year==year);
                else if (request.type == 2)
                {
                }
                else
                    throw new Exception("上报状态不正确");

                var count = db.Select(builder).Count;
                if (!string.IsNullOrEmpty(request.Sort) && !string.IsNullOrEmpty(request.Order) && request.Order == "asc")
                    builder.OrderBy(x => request.Sort);
                else if (!string.IsNullOrEmpty(request.Sort) && !string.IsNullOrEmpty(request.Order) &&
                         request.Order == "desc")
                    builder.OrderByDescending(x => request.Sort);
                else
                    builder.OrderBy(x => x.adcd);

                var rows = request.PageSize == 0 ? 10 : request.PageSize;
                var skip = request.PageIndex == 0 ? 0 : (request.PageIndex - 1) * rows;

                builder.Limit(skip, rows);
                var list = db.Select<VillageViewModel>(builder);
                return new BsTableDataSource<VillageViewModel>() { total = count, rows = list };

            }
        }

        /// <summary>
        /// 保存形势图路径等等
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool SaveVillagePic(SaveVillagePic request)
        {
            using (var db=DbFactory.Open())
            {
                if (string.IsNullOrEmpty(request.adcd))
                    throw new Exception("行政区划编码不能为空");
                if (string.IsNullOrEmpty(request.filePath))
                    throw new Exception("文件路径不能为空");
                if (request.Year==0)
                    throw new Exception("年份异常");
                var log = new operateLog();
                log.userName = RealName;
                log.operateTime = DateTime.Now;
                var logList = new List<operateLog>();
                var list = GetVillagePicAdcd(new GetVillagePicAdcd() {adcd=request.adcd,year = request.Year, type =1}).rows;
                if (list.Count == 1)
                {
                    log.operateMsg = "更新" + ZZTXManager.GetADCDInfoByADCD(request.adcd).adnm + "的防汛防台形势图";
                    logList.Add(log);
                    #region 日志
                    StringBuilder sb = new StringBuilder();
                    sb.Append("在栏目{组织责任/行政村防汛防台形势图}下,更新了形势图{"+ ZZTXManager.GetADCDInfoByADCD(request.adcd).adnm + "}");
                    _ILogHelper.WriteLog(sb.ToString(), OperationTypeEnums.更新);
                    #endregion
                    var _AuditNums = 0;
                    if (AuditNums != null)
                    {
                        _AuditNums = AuditNums.Value + 1;
                    }
                    return db.UpdateOnly(
                        new VillagePic() {path = request.filePath,operateLog = JsonTools.ObjectToJson(logList),AuditNums = _AuditNums },
                        onlyFields: x => new {x.path,x.operateLog},
                        where: x => x.Id == list[0].Id) == 1;
                }
                else
                {
                    var newInfo = new VillagePic2();
                    string[] arryFiles;
                    string picNames = "";
                    if (request.filePath.Contains("|"))
                    {
                        arryFiles = request.filePath.Split('|');
                        foreach (var item in arryFiles)
                        {
                            JObject jo = (JObject)JsonConvert.DeserializeObject(item);
                            picNames += Path.GetFileName(jo["fileSrc"].ToString().Replace("/", @"\")) + ",";
                            
                        }
                        picNames = picNames.Substring(0, picNames.Length - 1);

                    }
                    else
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(request.filePath);
                        picNames = Path.GetFileName(jo["fileSrc"].ToString().Replace("/", @"\"));
                    }
                    newInfo.Adcd = request.adcd;
                    newInfo.Year = request.Year;
                    newInfo.PicName = picNames;
                    newInfo.CreatTime = DateTime.Now;
                    newInfo.id = Guid.NewGuid().ToString();
                    var info = new VillagePic();
                    info.adcd = request.adcd;
                    info.Year = request.Year;
                    info.path = request.filePath;
                    info.CreatTime = DateTime.Now;
                    //写入更新记录
                    if (AuditNums != null)
                    {
                        info.AuditNums = AuditNums.Value + 1;
                        newInfo.AuditNums = AuditNums.Value + 1;
                    }
                    else
                    {
                        newInfo.AuditNums = 0;
                    }

                    log.operateMsg = "新增" + ZZTXManager.GetADCDInfoByADCD(request.adcd).adnm + "的防汛防台形势图";
                    logList.Add(log);
                    info.operateLog = JsonTools.ObjectToJson(logList);
                    newInfo.operateLog= JsonTools.ObjectToJson(logList);
                    #region 日志
                    StringBuilder sb = new StringBuilder();
                    sb.Append("在栏目{组织责任/行政村防汛防台形势图}下,新增了{"+ ZZTXManager.GetADCDInfoByADCD(request.adcd).adnm + "}形势图");
                    _ILogHelper.WriteLog(sb.ToString(), OperationTypeEnums.新增);
                    #endregion
                    long a=db.Insert(info);
                    string insertsql = "INSERT INTO VillagePic2(id, Adcd, PicName, isDelete, Year, operateLog, CreatTime, AuditNums) VALUES('" + newInfo.id + "','" + newInfo.Adcd + "', '" + newInfo.PicName + "', '" + newInfo.isDelete + "', '" + newInfo.Year + "', '" + newInfo.operateLog + "', '" + newInfo.CreatTime + "', '" + newInfo.AuditNums + "')";
                    long b = db.ExecuteSql(insertsql);
                    return b==1;
                    //return db.Insert(info) == 1;
                }
            }
        }


        /// <summary>
        /// 删除行政村防汛防台形势图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool DelVillagePic(DelVillagePic request)
        {
            using (var db = DbFactory.Open())
            {
                if(string.IsNullOrEmpty(request.adcds))
                    throw new Exception("行政编码不能为空");
                if (request.Year==0)
                    throw new Exception("年份异常");
                ArrayList arr = new ArrayList();
                string[] arrs = request.adcds.Split(',');
                for (int i = 0; i < arrs.Length; i++)
                {
                    arr.Add(arrs[i]);
                    #region 日志
                    var _r = db.Single<VillagePic>(w=>w.adcd==arrs[i].ToString());
                    StringBuilder sb = new StringBuilder();
                    sb.Append("在栏目{组织责任/行政村防汛防台形势图}下,删除了形势图"+_r.path+"");
                    _ILogHelper.WriteLog(sb.ToString(), OperationTypeEnums.删除);
                    #endregion
                }

                return db.Delete<VillagePic2>(x => Sql.In(x.Adcd, arr)&&x.Year==request.Year) > 0;
            }
        }


        /// <summary>
        /// 获取已上报的行政村防汛防台形势图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public VillagePic2 GetVillagePicByAdcdAndYear(GetVillagePicByAdcdAndYear request)
        {
            using (var db=DbFactory.Open())
            {
                var builder = db.From<VillagePic2>();
                if (request.year == 0)
                { throw new Exception("年度不正确"); }
                if (string.IsNullOrEmpty(request.adcd) && request.adcd.Length == 15)
                { throw new Exception("行政区划编码为空或不规范"); }
                builder.Where(x=>x.Year== request.year&& x.Adcd == request.adcd);
                if (request.nums != null && request.nums > 1) { builder.Where(x => x.AuditNums == request.nums); }
                var model = db.Single(builder);
                return model;
            }
        }

        /// <summary>
        /// 获取已上报的行政村防汛防台形势图列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BsTableDataSource<VillagePicViewModel> GetVillagePicList(GetVillagePicList request)
        {
            using (var db=DbFactory.Open())
            {
                var builder = db.From<VillagePic2>();
                if (request.year == 0)
                    throw new Exception("年度不正确");
                
                builder.LeftJoin<VillagePic2, ADCDInfo>((x, y) => x.Adcd == y.adcd);
                builder.Where(x => x.Year == request.year&&x.Adcd.StartsWith(adcd.Substring(0,9))&&x.Adcd!=adcd);
                if (!string.IsNullOrEmpty(request.adnm))
                    builder.And<ADCDInfo>(y=>y.adnm.Contains(request.adnm));
                builder.Select<VillagePic2, ADCDInfo>((x, y) => new { Id=x.id, adcd=x.Adcd,adnm=y.adnm, PicName = x.PicName, Year =x.Year, CreatTime =x.CreatTime, operateLog =x.operateLog});
                var count = db.Select(builder).Count;
                if (!string.IsNullOrEmpty(request.Sort) && !string.IsNullOrEmpty(request.Order) && request.Order == "asc")
                    builder.OrderBy(x => request.Sort);
                else if (!string.IsNullOrEmpty(request.Sort) && !string.IsNullOrEmpty(request.Order) &&
                         request.Order == "desc")
                    builder.OrderByDescending(x => request.Sort);
                else
                    builder.OrderBy(x => x.Adcd);

                var rows = request.PageSize == 0 ? 10 : request.PageSize;
                var skip = request.PageIndex == 0 ? 0 : (request.PageIndex - 1) * rows;

                builder.Limit(skip, rows);
                var list = db.Select<VillagePicViewModel>(builder);
                return new BsTableDataSource<VillagePicViewModel>() { total = count, rows = list };
            }
        }
    }
}
