using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrassrootsFloodCtrl.Model.SumAppUser;
using GrassrootsFloodCtrl.ServiceModel.Route;
using ServiceStack.OrmLite;
using GrassrootsFloodCtrl.Logic.Common;
using GrassrootsFloodCtrl.Model;
using System.Data;
using GrassrootsFloodCtrl.Model.ZZTX;
using System.Diagnostics;
using System.Collections.Concurrent;
using GrassrootsFloodCtrl.ServiceModel.Common;
using System.Configuration;
using ServiceStack;
using GrassrootsFloodCtrl.ServiceModel;
using System.Runtime.CompilerServices;
using GrassrootsFloodCtrl.Logic.Factory;
using GrassrootsFloodCtrl.Model.Sys;

namespace GrassrootsFloodCtrl.Logic.SumAppUser
{
    public class SunAppUserLogic : ManagerBase, ISunAppUserLogic
    {
        ConcurrentBag<SelectSumAppUserList> concurrentList = new ConcurrentBag<SelectSumAppUserList>();
        ConcurrentBag<SelectSumAppUserList> concurrentCityList = new ConcurrentBag<SelectSumAppUserList>();
       
        /// <summary>
        /// 获取注册情况统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<SelectSumAppUserList> GetSelectSumAppUserList(RouteSumAppUser request)
        {
            using (var db = DbFactory.Open())
            {
                string userSql = "select COUNT(*) as userCount,a.adcd,a.adcdId,b.adnm,b.parentId from AppAllUserView a left join ADCDInfo b on a.adcdId=b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' ";
                string appSql = "select COUNT(*) as appcount,b.adcd,a.adcdId,b.adnm,b.parentId from AppGetReg a left join ADCDInfo b on  a.adcdId=b.Id where a.adcdId is not null  and RIGHT(b.adcd, 11)!= '00000000000' ";
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "省级")
                {
                    return GetProvinceList(request.adcd, request.adcdName, db);
                    //userSql += "  and a.adcd like '33%'";
                    //appSql += "  and b.adcd like '33%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "市级")
                {
                    return GetCityList(request.adcd,request.adcdName,db);

                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "县级")
                {
                    return GetCountryList(request.adcd, request.adcdName, db);
                    //userSql += "  and a.adcd like '" + request.adcd.Substring(0, 6) + "%'";
                    //appSql += "  and b.adcd like '" + request.adcd.Substring(0, 6) + "%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "镇级")
                {
                    return GetTownList(request.adcd, request.adcdName, db);
                    //userSql += "  and a.adcd like '" + request.adcd.Substring(0, 9) + "%'";
                    //appSql += "  and b.adcd like '" + request.adcd.Substring(0, 9) + "%'";
                }
                if (request.adcdName != null && request.adcdName != "")
                {
                    userSql += "  and b.adnm like '%"+request.adcdName+"%'";
                    appSql += "  and b.adnm like  '%" + request.adcdName + "%'";
                }
                userSql += "  group by a.adcd,a.adcdId,b.adnm,b.parentId order by a.adcdId";
                appSql += "  group by b.adcd,a.adcdId,b.adnm,b.parentId order by a.adcdId";
                List <SelectSumAppUserList> allList = db.SqlList<SelectSumAppUserList>(userSql);
                List<SelectSumAppUserList> appList = db.SqlList<SelectSumAppUserList>(appSql);
                return GetSumAppUserList(allList,appList,request.adcd);
            }
        }

        public List<SelectSumAppUserList> GetSelectSumAppUserList2(RouteSumAppUser2 request)
        {
            if (AdcdHelper.GetByAdcdRole(request.adcd) == "省级")
            {
                return GetProvinceList2(request.adcd, request.adcdName);
                //userSql += "  and a.adcd like '33%'";
                //appSql += "  and b.adcd like '33%'";
            }
            if (AdcdHelper.GetByAdcdRole(request.adcd) == "市级")
            {
                return GetCityList2(request.adcd, request.adcdName);
            }
            return null;
        }
        public List<AppUserPerson> GetCountryList2(string adcd,bool isRegister)
        {
            switch (AdcdHelper.GetByAdcdRole(adcd))
            {
                case "省级":
                     adcd = adcd.Substring(0, 2);
                    break;
                case "市级":
                     adcd = adcd.Substring(0, 4);
                    break;
                case "县级":
                    adcd = adcd.Substring(0, 6);
                    break;
                case "镇级":
                    adcd = adcd.Substring(0, 9);
                    break;
                default:
                    return null;
            }
            string sql = "select a.phone,a.userName,b.adnm,a.adcd from AppAlluserView a left join ADCDInfo b on a.adcdId = b.Id  where a.adcd like '" + adcd + "%' group by a.phone,a.userName,b.adnm,a.adcd order by a.adcd";
            string userSql = "select a.Mobile as phone,b.adnm,b.adcd,a.UserName from  AppGetReg a left join ADCDInfo b on a.AdcdId=b.Id where b.adcd like  '" + adcd + "%' group by a.Mobile,b.adnm,b.adcd,a.UserName order by b.adcd";
            using (var db = DbFactory.Open())
            {
                
                List<AppUserPerson> userList = db.SqlList<AppUserPerson>(userSql);
                if (isRegister)
                {
                    return userList;
                }
                List<AppUserPerson> allList = db.SqlList<AppUserPerson>(sql);
                //移除已注册名单
                foreach (var item in userList)
                {
                    allList.RemoveAll(x => x.phone == item.phone && x.adcd == item.adcd);
                }
                return allList;
            }
        }
        
        private List<SelectSumAppUserList> GetProvinceList2(string adcd, string adcdName)
        {
            List<SelectSumAppUserList> list = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> adcdList = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> uList = new List<SelectSumAppUserList>();
            using (var db = DbFactory.Open())
            {
                if (AdcdHelper.GetByAdcdRole(adcd) == "省级")
                {
                    //市长级按照市级用户分类统计
                    string sql = "select COUNT(*) as userCount,left(a.adcd, 4) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 2) + "%' ";
                    sql += "group by left(a.adcd, 4) order by left(a.adcd, 4)";
                    string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd + "') order by adcd ";
                    string userSql = "select COUNT(*) as appcount,left(b.adcd, 4) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 2) + "%' group by left(b.adcd, 4) order by left(b.adcd, 4)";
                    list = db.SqlList<SelectSumAppUserList>(sql);
                    adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
                    uList = db.SqlList<SelectSumAppUserList>(userSql);
                }

            }
            //所有县级的list
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //启动12个线程分别计算县级的
            Task task1 = new Task(() => { InsertConcurrentList(0, 1, list, uList, adcdList); });
            Task task2 = new Task(() => { InsertConcurrentList(1, 2, list, uList, adcdList); });
            Task task3 = new Task(() => { InsertConcurrentList(2, 3, list, uList, adcdList); });
            Task task4 = new Task(() => { InsertConcurrentList(3, 4, list, uList, adcdList); });
            Task task5 = new Task(() => { InsertConcurrentList(4, 5, list, uList, adcdList); });
            Task task6 = new Task(() => { InsertConcurrentList(5, 6, list, uList, adcdList); });
            Task task7 = new Task(() => { InsertConcurrentList(6, 7, list, uList, adcdList); });
            Task task8 = new Task(() => { InsertConcurrentList(7, 8, list, uList, adcdList); });
            Task task9 = new Task(() => { InsertConcurrentList(8, 9, list, uList, adcdList); });
            Task task10 = new Task(() => { InsertConcurrentList(9, 10, list, uList, adcdList); });
            Task task11 = new Task(() => { InsertConcurrentList(10, 11, list, uList, adcdList); });
            task1.Start();
            task2.Start();
            task3.Start();
            task4.Start();
            task5.Start();
            task6.Start();
            task7.Start();
            task8.Start();
            task9.Start();
            task10.Start();
            task11.Start();
            Task.WaitAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            //将所有的县级加入市级列表
            list.AddRange(concurrentList);
            return list;
        }
        private List<SelectSumAppUserList> GetCityList2(string adcd, string adcdName)
        {
            List<SelectSumAppUserList> list = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> adcdList = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> uList = new List<SelectSumAppUserList>();
            using (var db = DbFactory.Open())
            {
                if (AdcdHelper.GetByAdcdRole(adcd) == "市级")
                {
                    //市长级按照市级用户分类统计
                    string sql = "select COUNT(*) as userCount,left(a.adcd, 4) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 4) + "%' ";
                    sql += "group by left(a.adcd, 4) order by left(a.adcd, 4)";
                    string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where adcd='"+adcd+"' order by adcd ";
                    string userSql = "select COUNT(*) as appcount,left(b.adcd, 4) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 4) + "%' group by left(b.adcd, 4) order by left(b.adcd, 4)";
                    list = db.SqlList<SelectSumAppUserList>(sql);
                    adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
                    uList = db.SqlList<SelectSumAppUserList>(userSql);
                }
                InsertCityConcurrentList(0, 1, list, uList, adcdList);
                list.AddRange(concurrentCityList);
                return list;
            }
            //将所有的县级加入市级列表
            //list.AddRange(concurrentList);
            //return list;
        }
        public void InsertCityConcurrentList(int start, int end, List<SelectSumAppUserList> list, List<SelectSumAppUserList> uList, List<SelectSumAppUserList> adcdList)
        {
            List<SelectSumAppUserList> countrylist = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> adcdCountryList = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> uCountryList = new List<SelectSumAppUserList>();
            using (var db = DbFactory.Open())
            {
                //注册人员的统计放入list
                for (int i = start; i < end; i++)
                {
                    //注册人员的统计放入list
                    var userItem = uList.Find(x => x.adcd == list[i].adcd);
                    if (userItem != null)
                    {
                        list[i].appcount = userItem.appcount;
                    }
                    else
                    {
                        list[i].appcount = 0;
                    }
                    //查找市级按照县级分类的统计放入countryList
                    string countrySql = "select COUNT(*) as userCount,left(a.adcd, 6) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '" + list[i].adcd + "%' group by left(a.adcd, 6) order by left(a.adcd, 6)";
                    string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcdList[i].adcd + "') order by adcd ";
                    string countryUList = "select COUNT(*) as appcount,left(b.adcd, 6) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + list[i].adcd + "%' group by left(b.adcd, 6) order by left(b.adcd, 6)";
                    countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                    adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                    uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                    for (int j = 0; j < countrylist.Count; j++)
                    {
                        var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                        if (countryItem != null)
                        {
                            countrylist[j].appcount = countryItem.appcount;
                        }
                        else
                        {
                            countrylist[j].appcount = 0;
                        }
                        countrylist[j].adcd = adcdCountryList[j].adcd;
                        countrylist[j].adnm = adcdCountryList[j].adnm;
                        countrylist[j].adcdId = adcdCountryList[j].adcdId + 20000 * (j + 1);
                        countrylist[j].parentId = i + 1;
                        concurrentCityList.Add(countrylist[j]);
                        InsertCountryConcurrentList(countrylist[j].adcd, db, countrylist[j].adcdId);
                    }
                    list[i].adcd = adcdList[i].adcd;
                    list[i].adcdId = i + 1;
                    list[i].adnm = adcdList[i].adnm;
                    list[i].parentId = 0;
                }
            }
        }
        public void InsertConcurrentList(int start, int end, List<SelectSumAppUserList> list, List<SelectSumAppUserList> uList, List<SelectSumAppUserList> adcdList)
        {
            List<SelectSumAppUserList> countrylist = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> adcdCountryList = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> uCountryList = new List<SelectSumAppUserList>();
            using (var db = DbFactory.Open())
            {
                //注册人员的统计放入list
                for (int i = start; i < end; i++)
                {
                    //注册人员的统计放入list
                    var userItem = uList.Find(x => x.adcd == list[i].adcd);
                    if (userItem != null)
                    {
                        list[i].appcount = userItem.appcount;
                    }
                    else
                    {
                        list[i].appcount = 0;
                    }
                    //查找市级按照县级分类的统计放入countryList
                    string countrySql = "select COUNT(*) as userCount,left(a.adcd, 6) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '" + list[i].adcd + "%' group by left(a.adcd, 6) order by left(a.adcd, 6)";
                    string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcdList[i].adcd + "') order by adcd ";
                    string countryUList = "select COUNT(*) as appcount,left(b.adcd, 6) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + list[i].adcd + "%' group by left(b.adcd, 6) order by left(b.adcd, 6)";
                    countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                    adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                    uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                    for (int j = 0; j < countrylist.Count; j++)
                    {
                        var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                        if (countryItem != null)
                        {
                            countrylist[j].appcount = countryItem.appcount;
                        }
                        else
                        {
                            countrylist[j].appcount = 0;
                        }
                        countrylist[j].adcd = adcdCountryList[j].adcd;
                        countrylist[j].adnm = adcdCountryList[j].adnm;
                        countrylist[j].adcdId = adcdCountryList[j].adcdId+20000*(j+1);
                        countrylist[j].parentId = i + 1;
                        concurrentList.Add(countrylist[j]);
                        InsertCityConcurrentList(countrylist[j].adcd,db, countrylist[j].adcdId);
                    }
                    list[i].adcd = adcdList[i].adcd;
                    list[i].adcdId = i + 1;
                    list[i].adnm = adcdList[i].adnm;
                    list[i].parentId = 0;
                }
            }
        }
        public void InsertCountryConcurrentList(string adcd, IDbConnection db, int parentId)
        {
            string sql = "select COUNT(*) as userCount,left(a.adcd, 9) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 6) + "%' ";
            sql += "group by left(a.adcd, 9) order by left(a.adcd, 9)";
            string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd + "') order by adcd ";
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 9) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 6) + "%' group by left(b.adcd, 9) order by left(b.adcd, 9)";
            List<SelectSumAppUserList> countryList = db.SqlList<SelectSumAppUserList>(sql);
            List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(userSql);
            for (int i = 1; i < countryList.Count; i++)
            {
                //注册人员的统计放入list
                var userItem = uCountryList.Find(x => x.adcd == countryList[i].adcd);
                if (userItem != null)
                {
                    countryList[i].appcount = userItem.appcount;
                }
                else
                {
                    countryList[i].appcount = 0;
                }
                countryList[i].adcd = adcdCountryList[i - 1].adcd;
                countryList[i].adcdId = adcdCountryList[i - 1].adcdId + 200000 * (i + 1);
                countryList[i].adnm = adcdCountryList[i - 1].adnm;
                countryList[i].parentId = parentId;
                concurrentCityList.Add(countryList[i]);
            }
            if (countryList.Count > 0)
            {
                countryList[0].adnm = "县本级";
                countryList[0].adcd = adcd;
                countryList[0].adcdId = countryList[0].adcdId + 20000;
                if (uCountryList.Count > 0)
                {
                    var userItem = uCountryList.Find(x => x.adcd == adcd.Substring(0, 9));
                    if (userItem != null)
                    {
                        countryList[0].appcount = userItem.appcount;
                    }
                }
                countryList[0].parentId = parentId;
                concurrentCityList.Add(countryList[0]);
            }
        }
        public void InsertCityConcurrentList(string adcd,IDbConnection db,int parentId)
        {
            string sql = "select COUNT(*) as userCount,left(a.adcd, 9) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 6) + "%' ";
            sql += "group by left(a.adcd, 9) order by left(a.adcd, 9)";
            string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd + "') order by adcd ";
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 9) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 6) + "%' group by left(b.adcd, 9) order by left(b.adcd, 9)";
            List<SelectSumAppUserList> countryList = db.SqlList<SelectSumAppUserList>(sql);
            List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(userSql);
            for (int i = 1; i < countryList.Count; i++)
            {
                //注册人员的统计放入list
                var userItem = uCountryList.Find(x => x.adcd == countryList[i].adcd);
                if (userItem != null)
                {
                    countryList[i].appcount = userItem.appcount;
                }
                else
                {
                    countryList[i].appcount = 0;
                }
                countryList[i].adcd = adcdCountryList[i - 1].adcd;
                countryList[i].adcdId = adcdCountryList[i - 1].adcdId+ 200000*(i+1);
                countryList[i].adnm = adcdCountryList[i - 1].adnm;
                countryList[i].parentId = parentId;
                concurrentList.Add(countryList[i]);
            }
            if (countryList.Count > 0)
            {
                countryList[0].adnm = "县本级";
                countryList[0].adcd = adcd;
                countryList[0].adcdId = countryList[0].adcdId+20000;
                if (uCountryList.Count > 0)
                {
                    var userItem = uCountryList.Find(x => x.adcd == adcd.Substring(0, 9));
                    if (userItem != null)
                    {
                        countryList[0].appcount = userItem.appcount;
                    }
                }
                countryList[0].parentId = parentId;
                concurrentList.Add(countryList[0]);
            }
        }
        /// <summary>
        /// 省级统计情况
        /// </summary>
        /// <param name="adcd"></param>
        /// <param name="adcdName"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private List<SelectSumAppUserList> GetProvinceList(string adcd, string adcdName, IDbConnection db)
        {
            //县级按照市级用户分类统计
            string sql = "select COUNT(*) as userCount,left(a.adcd, 4) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 2) + "%' ";
            sql += "group by left(a.adcd, 4) order by left(a.adcd, 4)";
            string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd + "') order by adcd ";
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 4) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 2) + "%' group by left(b.adcd, 4) order by left(b.adcd, 4)";
            List<SelectSumAppUserList> list = db.SqlList<SelectSumAppUserList>(sql);
            List<SelectSumAppUserList> adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uList = db.SqlList<SelectSumAppUserList>(userSql);
            //所有县级的list
            //计划按照并行操作每个市级开线程访问
            List<SelectSumAppUserList> countryAllList = new List<SelectSumAppUserList>();
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            for (int i = 0; i < list.Count; i++)
            {
                //注册人员的统计放入list
                var userItem = uList.Find(x => x.adcd == list[i].adcd);
                if (userItem != null)
                {
                    list[i].appcount = userItem.appcount;
                }
                else
                {
                    list[i].appcount = 0;
                }
                //查找市级按照县级分类的统计放入countryList
                string countrySql = "select COUNT(*) as userCount,left(a.adcd, 6) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '" + list[i].adcd + "%' group by left(a.adcd, 6) order by left(a.adcd, 6)";
                string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcdList[i].adcd + "') order by adcd ";
                string countryUList = "select COUNT(*) as appcount,left(b.adcd, 6) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + list[i].adcd + "%' group by left(b.adcd, 6) order by left(b.adcd, 6)";
                List<SelectSumAppUserList> countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                for (int j = 0; j < countrylist.Count; j++)
                {
                    var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                    if (countryItem != null)
                    {
                        countrylist[j].appcount = countryItem.appcount;
                    }
                    else
                    {
                        countrylist[j].appcount = 0;
                    }
                    countrylist[j].adcd = adcdCountryList[j].adcd;
                    countrylist[j].adnm = adcdCountryList[j].adnm;
                    countrylist[j].adcdId = adcdCountryList[j].adcdId;
                    countrylist[j].parentId = i*100000 + 1;
                }
                countryAllList.AddRange(countrylist);
                list[i].adcd = adcdList[i].adcd;
                list[i].adcdId = i* 100000 + 1;
                list[i].adnm = adcdList[i].adnm;
                list[i].parentId = 0;
            }
            sw2.Stop();
            TimeSpan ts2 = sw2.Elapsed;
            //将所有的县级加入市级列表
            list.AddRange(countryAllList);
            if (adcdName != null)
            {
                list = list.FindAll(x=>x.adnm.Contains(adcdName));
            }
            return list;
        }
        public void InsertConcurrentList(int start,int end, List<SelectSumAppUserList> list, List<SelectSumAppUserList> uList, List<SelectSumAppUserList> adcdList, IDbConnection db)
        {
            //注册人员的统计放入list
            for (int i = start; i < end; i++)
            {
                //注册人员的统计放入list
                var userItem = uList.Find(x => x.adcd == list[i].adcd);
                if (userItem != null)
                {
                    list[i].appcount = userItem.appcount;
                }
                else
                {
                    list[i].appcount = 0;
                }
                //查找市级按照县级分类的统计放入countryList
                string countrySql = "select COUNT(*) as userCount,left(a.adcd, 6) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '" + list[i].adcd + "%' group by left(a.adcd, 6) order by left(a.adcd, 6)";
                string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcdList[i].adcd + "') order by id ";
                string countryUList = "select COUNT(*) as appcount,left(b.adcd, 6) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + list[i].adcd + "%' group by left(b.adcd, 6) order by left(b.adcd, 6)";
                List<SelectSumAppUserList> countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                for (int j = 0; j < countrylist.Count; j++)
                {
                    var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                    if (countryItem != null)
                    {
                        countrylist[j].appcount = countryItem.appcount;
                    }
                    else
                    {
                        countrylist[j].appcount = 0;
                    }
                    countrylist[j].adcd = adcdCountryList[j].adcd;
                    countrylist[j].adnm = adcdCountryList[j].adnm;
                    countrylist[j].adcdId = adcdCountryList[j].adcdId;
                    countrylist[j].parentId = i + 1;
                    concurrentList.Add(countrylist[j]);
                }
                list[i].adcd = adcdList[i].adcd;
                list[i].adcdId = i + 1;
                list[i].adnm = adcdList[i].adnm;
                list[i].parentId = 0;
            }
        }
        private List<SelectSumAppUserList> GetTownList(string adcd, string adcdName, IDbConnection db)
        {
            string sql = "select COUNT(*) as userCount,left(a.adcd, 12) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 9) + "%' and b.adnm like '%" + adcdName + "%' ";
            sql += "group by left(a.adcd, 12) order by left(a.adcd, 12)";
            string sqlAdcdInfo = "  select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where adcd like  '"+ adcd.Substring(0, 9) + "%' ";
            if (adcdName != null)
            {
                sqlAdcdInfo += " and adnm like '%" + adcdName + "%' order by adcd  ";
            }
            else
            {
                sqlAdcdInfo += " and adcd!='"+adcd+"'  order by adcd  ";
            }
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 12) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 9) + "%' and b.adnm like '%" + adcdName + "%' group by left(b.adcd, 12) order by left(b.adcd, 12)";
            List<SelectSumAppUserList> list = db.SqlList<SelectSumAppUserList>(sql);
            List<SelectSumAppUserList> adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uList = db.SqlList<SelectSumAppUserList>(userSql);
            for (int i = 0; i < list.Count; i++)
            {
                var userItem = uList.Find(x => x.adcd == list[i].adcd);
                if (userItem != null)
                {
                    list[i].appcount = userItem.appcount;
                }
                if (i == 0)
                {
                    if (adcdName==null)
                    {
                        list[i].adnm = "镇本级";
                    }
                    else
                    {
                        list[i].adnm = adcdList[i].adnm;
                    }
                    
                    list[i].adcd = adcd;
                    list[i].adcdId = list.Count + 1;
                    list[i].parentId = 0;
                }
                else
                {
                    if (adcdName == null)
                    {
                        list[i].adcd = adcdList[i - 1].adcd;
                        list[i].adcdId = i + 1;
                        list[i].adnm = adcdList[i - 1].adnm;
                        list[i].parentId = 0;
                    }
                    else
                    {
                        list[i].adcd = adcdList[i].adcd;
                        list[i].adcdId = i + 1;
                        list[i].adnm = adcdList[i].adnm;
                        list[i].parentId = 0;
                    }

                }

            }
            return list;
        }
        private List<SelectSumAppUserList> GetCountryList(string adcd, string adcdName, IDbConnection db)
        {
            string sql = "select COUNT(*) as userCount,left(a.adcd, 9) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '" + adcd.Substring(0, 6) + "%' ";
            sql += "group by left(a.adcd, 9) order by left(a.adcd, 9)";
            string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd + "')  order by adcd ";
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 9) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 6) + "%' group by left(b.adcd, 9) order by left(b.adcd, 9)";
            List<SelectSumAppUserList> list = db.SqlList<SelectSumAppUserList>(sql);
            List<SelectSumAppUserList> adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uList = db.SqlList<SelectSumAppUserList>(userSql);
            //所有县级的list
            List<SelectSumAppUserList> countryAllList = new List<SelectSumAppUserList>();
            for (int i = 1; i < list.Count; i++)
            {
                //注册人员的统计放入list
                var userItem = uList.Find(x => x.adcd == list[i].adcd);
                if (userItem != null)
                {
                    list[i].appcount = userItem.appcount;
                }
                else
                {
                    list[i].appcount = 0;
                }
                //查找每个县的统计放入countryList
                string countrySql = "select COUNT(*) as userCount,left(a.adcd, 12) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '" + list[i].adcd + "%' group by left(a.adcd, 12) order by left(a.adcd, 12)";
                string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcdList[i-1].adcd + "') order by adcd ";
                string countryUList = "select COUNT(*) as appcount,left(b.adcd, 12) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + list[i].adcd + "%' group by left(b.adcd, 12) order by left(b.adcd, 12)";
                List<SelectSumAppUserList> countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                for (int j = 0; j < countrylist.Count; j++)
                {
                    var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                    if (countryItem != null)
                    {
                        countrylist[j].appcount = countryItem.appcount;
                    }
                    else
                    {
                        countrylist[j].appcount = 0;
                    }
                    if (j != 0)
                    {
                        countrylist[j].adcd = adcdCountryList[j - 1].adcd;
                        countrylist[j].adnm = adcdCountryList[j - 1].adnm;
                        countrylist[j].adcdId = adcdCountryList[j - 1].adcdId;
                        countrylist[j].parentId = i + 1;
                    }
                    else
                    {
                        countrylist[j].adcd = countrylist[j].adcd + "000";
                        countrylist[j].adnm = "镇本级";
                        List<ADCDInfo> parentList = db.SqlList<ADCDInfo>(" select id,parentId from ADCDInfo where adcd='" + countrylist[j].adcd + "'");
                        if (parentList != null)
                        {
                            countrylist[j].adcdId = parentList[0].Id;
                            countrylist[j].parentId = i + 1;
                        }
                    }
                }
                countryAllList.AddRange(countrylist);
                list[i].adcd = adcdList[i-1].adcd;
                list[i].adcdId = i + 1;
                list[i].adnm = adcdList[i-1].adnm;
                list[i].parentId = 0;
            }
            if (list.Count > 0)
            {

                list[0].adnm = "县本级";
                list[0].adcd = adcd;
                list[0].adcdId = list.Count + 1;
                if (uList.Count > 0)
                {
                    var userItem = uList.Find(x => x.adcd == adcd.Substring(0,9));
                    if (userItem != null)
                    {
                        list[0].appcount = userItem.appcount;
                    }
                }
                list[0].parentId = 0;
            }
            //list.Remove(list[0]);
            //将所有的县级加入市级列表
            list.AddRange(countryAllList);
            if (adcdName != null)
            {
                list = list.FindAll(x => x.adnm.Contains(adcdName));
            }
            return list;
        }
        private List<SelectSumAppUserList> GetCityList(string adcd,string adcdName, IDbConnection db)
        {
            string sql = "select COUNT(*) as userCount,left(a.adcd, 6) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' and a.adcd like  '"+adcd.Substring(0,4)+ "%'  ";
            sql += "group by left(a.adcd, 6) order by left(a.adcd, 6)";
            string sqlAdcdInfo = " select adcd,adnm,id as adcdId,0 as parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='" + adcd+ "')  order by adcd ";
            string userSql = "select COUNT(*) as appcount,left(b.adcd, 6) as adcd from AppGetReg a  left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '" + adcd.Substring(0, 4) + "%' group by left(b.adcd, 6) order by left(b.adcd, 6)";
            List<SelectSumAppUserList> list = db.SqlList<SelectSumAppUserList>(sql);         
            List<SelectSumAppUserList> adcdList = db.SqlList<SelectSumAppUserList>(sqlAdcdInfo);
            List<SelectSumAppUserList> uList= db.SqlList<SelectSumAppUserList>(userSql);
            //所有县级的list
            List<SelectSumAppUserList> countryAllList = new List<SelectSumAppUserList>();
            for(int i=0;i<list.Count;i++)
            {
                //注册人员的统计放入list
                var userItem = uList.Find(x => x.adcd == list[i].adcd);
                if (userItem != null)
                {
                    list[i].appcount = userItem.appcount;
                }
                else
                {
                    list[i].appcount = 0;
                }
                //查找每个县的统计放入countryList
                string countrySql = "select COUNT(*) as userCount,left(a.adcd, 9) as adcd from AppAllUserView a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000'and a.adcd like  '"+list[i].adcd+"%' group by left(a.adcd, 9) order by left(a.adcd, 9)";
                string countryAdcdSql = " select adcd,adnm,id as adcdId, parentId from ADCDInfo where parentId in( select id from ADCDInfo where adcd='"+ adcdList[i].adcd + "') order by adcd ";
                string countryUList = "select COUNT(*) as appcount,left(b.adcd, 9) as adcd from AppGetReg a left join ADCDInfo b on a.adcdId = b.Id where a.adcdId is not null and RIGHT(b.adcd, 11)!= '00000000000' and b.adcd like  '"+list[i].adcd+"%' group by left(b.adcd, 9) order by left(b.adcd, 9)";
                List<SelectSumAppUserList> countrylist = db.SqlList<SelectSumAppUserList>(countrySql);
                List<SelectSumAppUserList> adcdCountryList = db.SqlList<SelectSumAppUserList>(countryAdcdSql);
                List<SelectSumAppUserList> uCountryList = db.SqlList<SelectSumAppUserList>(countryUList);
                for (int j = 0; j < countrylist.Count; j++)
                {
                    var countryItem = uCountryList.Find(x => x.adcd == countrylist[j].adcd);
                    if (countryItem != null)
                    {
                        countrylist[j].appcount = countryItem.appcount;
                    }
                    else
                    {
                        countrylist[j].appcount = 0;
                    }
                    if (j != 0)
                    {
                        countrylist[j].adcd = adcdCountryList[j-1].adcd;
                        countrylist[j].adnm = adcdCountryList[j-1].adnm;
                        countrylist[j].adcdId = adcdCountryList[j-1].adcdId;
                        countrylist[j].parentId = i*100000 + 1;
                    }
                    else
                    {
                        countrylist[j].adcd = countrylist[j].adcd + "000000";
                        countrylist[j].adnm = "县本级";
                        List<ADCDInfo> parentList = db.SqlList<ADCDInfo>(" select id,parentId from ADCDInfo where adcd='" + countrylist[j].adcd + "'");
                        if (parentList != null)
                        {
                            countrylist[j].adcdId = parentList[0].Id;
                            countrylist[j].parentId = i * 100000 + 1;
                        }
                    }
                }
                countryAllList.AddRange(countrylist);
                list[i].adcd = adcdList[i].adcd;
                list[i].adcdId = i * 100000 + 1;
                list[i].adnm = adcdList[i].adnm;
                list[i].parentId = 0;
            }
            //将所有的县级加入市级列表
            list.AddRange(countryAllList);
            if (adcdName != null)
            {
                list = list.FindAll(x => x.adnm.Contains(adcdName));
            }
            return list;
        }
        /// <summary>
        /// 统计为注册人的信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BsTableDataSource<Model.SumAppUser.SumAppUser> GetSumAppUserList(RouteNoAppUser request)
        {
            using (var db = DbFactory.Open())
            {
                string userSql = "select a.adcd,a.adcdId,b.adnm,b.parentId,a.phone,a.UserName from AppAllUserView a left join ADCDInfo b on a.adcdId=b.Id  where a.adcdId is not null and a.adcd is not null and RIGHT(a.adcd, 11)!= '00000000000' ";
                string appSql = "select b.adcd,a.adcdId,b.adnm,b.parentId,a.Mobile as phone,a.UserName from AppGetReg a left join ADCDInfo b on  a.adcdId=b.Id where a.adcdId is not null  and RIGHT(b.adcd, 11)!= '00000000000'  ";
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "省级")
                {
                    userSql += "  and a.adcd like '33%'";
                    appSql += "  and b.adcd like '33%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "市级")
                {
                    userSql += "  and a.adcd like '" + request.adcd.Substring(0, 4) + "%'";
                    appSql += "  and b.adcd like '" + request.adcd.Substring(0, 4) + "%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "县级")
                {
                    userSql += "  and a.adcd like '" + request.adcd.Substring(0, 6) + "%'";
                    appSql += "  and b.adcd like '" + request.adcd.Substring(0, 6) + "%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "镇级")
                {
                    userSql += "  and a.adcd like '" + request.adcd.Substring(0, 9) + "%'";
                    appSql += "  and b.adcd like '" + request.adcd.Substring(0, 9) + "%'";
                }
                if (AdcdHelper.GetByAdcdRole(request.adcd) == "村级")
                {
                    userSql += "  and a.adcd like '" + request.adcd + "%'";
                    appSql += "  and b.adcd like '" + request.adcd + "%'";
                }
                userSql += "  group by a.adcd,a.adcdId,b.adnm,b.parentId,a.phone,a.UserName order by a.adcdId";
                appSql += "  group by b.adcd,a.adcdId,b.adnm,b.parentId,a.Mobile,a.UserName order by a.adcdId";
                List<Model.SumAppUser.SumAppUser> allList = 
                    db.SqlList<Model.SumAppUser.SumAppUser>(userSql);
                List<Model.SumAppUser.SumAppUser> appList = 
                    db.SqlList<Model.SumAppUser.SumAppUser>(appSql);
                var list = GetUserList(allList, appList);
                var pageList = list.Skip(request.PageSize * (request.PageIndex - 1))
                   .Take(request.PageSize).ToList();
                return new BsTableDataSource<Model.SumAppUser.SumAppUser> { total = list.Count(), rows = pageList };
            }
        }

        private List<Model.SumAppUser.SumAppUser> GetUserList(List<Model.SumAppUser.SumAppUser> allList, List<Model.SumAppUser.SumAppUser> appList)
        {
            foreach (var item in appList)
            {
                int a=allList.RemoveAll(x=>x.phone==item.phone&&x.adcd==item.adcd);
            }
            return allList;
        }
        /// <summary>
        /// 统计用户增加已录入的信息
        /// </summary>
        /// <param name="allList"></param>
        /// <param name="appList"></param>
        /// <param name="adcd"></param>
        /// <returns></returns>
        private List<SelectSumAppUserList> GetSumAppUserList(List<SelectSumAppUserList> allList, List<SelectSumAppUserList> appList,string adcd)
        {
            foreach(var item in appList)
            {
                var userItem=allList.Find(x => x.adcd == item.adcd);
                userItem.appcount = item.appcount;
            }
            if (AdcdHelper.GetByAdcdRole(adcd) == "县级"&&allList.Count>0)
            {
                allList[0].parentId = 0;
            }
            if (AdcdHelper.GetByAdcdRole(adcd) == "镇级" && allList.Count > 0)
            {
                allList[0].parentId = 0;
            }
            return allList;
        }
        /// <summary>
        /// 核对人员的信息斌修改
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public BaseResult CheckUser(RouteCheckAppUser request)
        {
            using (var db = DbFactory.Open())
            {
                //判断移动端是否注册该用户
                var webapi = ConfigurationManager.AppSettings["appApi"];
                using (var client = new JsonServiceClient(webapi))
                {
                    var r = client.Get<AppRegUser>("?username=" + request.phone.Trim());
                    if (r.type != "error")//app端已注册
                    {
                        return new BaseResult { IsSuccess = false, ErrorMsg = "该人员未注册!" };
                    }
                }
                //查询系统中的手机号
                var viewList = db.SqlList<AppUserIView>("select phone, userName, adcdId from AppAlluserView where phone = '"+request.phone.Trim()+"' ");
                //删除掉所有已经导入的信息然后在重新插入一遍
                bool del=db.Delete<AppGetReg>(x => x.Mobile == request.phone.Trim())>=0?true:false;
                if (del)
                {
                    StringBuilder insertSql = new StringBuilder();
                    foreach (var item in viewList)
                    {
                        insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE())");
                    }
                    return db.ExecuteSql(insertSql.ToString())>0 ? new BaseResult { IsSuccess = true, ErrorMsg = "数据检测成功!" }: new BaseResult { IsSuccess = false, ErrorMsg = "插入失败!" };
                }
                return new BaseResult { IsSuccess = false, ErrorMsg = "删除数据失败!" };
            }
        }
        /// <summary>
        /// 核对镇里面人数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResult CheckTown(RouteCheckTown request)
        {
            using (var db = DbFactory.Open())
            {
                //查询所有镇级信息
                List<AppUserIView> phoneList = db.SqlList<AppUserIView>("select phone, userName, adcdId from AppAlluserView where adcd='" + request.adcd + "'");
                //删除所有镇级注册信息
                bool del = db.ExecuteSql("delete from AppGetReg where  Mobile in (select distinct Mobile from AppGetReg where AdcdId in (select Id from ADCDInfo where adcd='" + request.adcd + "')) and AdcdId in (select Id from ADCDInfo where adcd='" + request.adcd + "')") >= 0 ? true : false;
                if (!del)
                {
                    return new BaseResult { IsSuccess = false, ErrorMsg = "删除数据失败!" };
                }
                //核对移动端信息进行信息插入
                StringBuilder insertSql = new StringBuilder();
                var webapi = ConfigurationManager.AppSettings["appApi"];
                foreach (var item in phoneList)
                {
                    //判断移动端是否注册该用户
                    var r = db.Single<AppUser>(x => x.Phone == item.phone);
                    if (r != null)
                    {
                        insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE());");
                    }
                    //using (var client = new JsonServiceClient(webapi))
                    //{
                    //    var r = client.Get<AppRegUser>("?username=" + item.phone.Trim());
                    //    if (r.type == "error")//app端已注册
                    //    {
                    //        insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE());");
                    //    }
                    //}
                }
                if (insertSql.ToString().Length == 0) {
                    return new BaseResult { IsSuccess = false, ErrorMsg = "该村没有任何人注册!" };
                }
                return db.ExecuteSql(insertSql.ToString()) > 0 ? new BaseResult { IsSuccess = true, ErrorMsg = "数据检测成功!" } : new BaseResult { IsSuccess = false, ErrorMsg = "插入失败!" };

                ////查询系统中镇里面已经注册的信息
                //List<AppUserIView> phoneList = db.SqlList<AppUserIView>("select phone, userName, adcdId from  AppAlluserView where  phone in (select distinct Mobile from AppGetReg where AdcdId in (select Id from ADCDInfo where adcd like '" + adcd.Substring(0, 9) + "%'))");
                ////删除所有手机号
                //bool del = db.ExecuteSql("delete from AppGetReg where  Mobile in (select distinct Mobile from AppGetReg where AdcdId in (select Id from ADCDInfo where adcd like '" + adcd.Substring(0, 9) + "%'))") >=0?true:false;
                //if (del)
                //{
                //    StringBuilder insertSql = new StringBuilder();
                //    foreach (var item in phoneList)
                //    {
                //        insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE())");
                //    }
                //    return db.ExecuteSql(insertSql.ToString()) > 0 ? new BaseResult { IsSuccess = true, ErrorMsg = "数据检测成功!" } : new BaseResult { IsSuccess = false, ErrorMsg = "插入失败!" };
                //}
                //return new BaseResult { IsSuccess = false, ErrorMsg = "删除数据失败!" };
            }
        }
        //同步该方法
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public BaseResult CheckTownUser(RouteCheckTownUser request)
        {
            using (var db = DbFactory.Open())
            {
                //查询系统中镇里面已经注册的信息
                List<AppUserIView> phoneList = new List<AppUserIView>();
                bool del = false;
                if (AdcdHelper.GetByAdcdRole(adcd) == "县级")
                {
                    phoneList = db.SqlList<AppUserIView>("select phone, userName, adcdId from AppAlluserView where adcdId in (select Id from ADCDInfo where adcd like '" + adcd + "')");
                    //删除所有手机号
                    del = db.ExecuteSql("delete from AppGetReg where  Mobile in (select distinct Mobile from AppGetReg where AdcdId in (select Id from ADCDInfo where adcd like '" + adcd + "'))") >= 0 ? true : false;
                }
                else
                {
                    phoneList = db.SqlList<AppUserIView>("select phone, userName, adcdId from AppAlluserView where adcdId in (select Id from ADCDInfo where adcd like '" + adcd.Substring(0, 9) + "%')");
                    //删除所有手机号
                    del = db.ExecuteSql("delete from AppGetReg where  Mobile in (select distinct Mobile from AppGetReg where AdcdId in (select Id from ADCDInfo where adcd like '" + adcd.Substring(0, 9) + "%'))") >= 0 ? true : false;
                }

                if (del)
                {
                    StringBuilder insertSql = new StringBuilder();
                    var webapi = ConfigurationManager.AppSettings["appApi"];
                    foreach (var item in phoneList)
                    {
                        //判断移动端是否注册该用户
                        var r = db.Single<AppUser>(x => x.Phone == item.phone);
                        if (r != null)
                        {
                            insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE());");
                        }
                        //using (var client = new JsonServiceClient(webapi))
                        //{
                        //    var r = client.Get<AppRegUser>("?username=" + item.phone.Trim());
                        //    if (r.type == "error")//app端已注册
                        //    {
                        //        insertSql.Append("insert into AppGetReg(Mobile, UserName, AdcdId,CreateTime) values('" + item.phone.Trim() + "','" + item.userName.Trim() + "','" + item.adcdId + "',GETDATE());");
                        //    }
                        //}
                    }
                    if (insertSql.ToString().Length == 0)
                    {
                        return new BaseResult { IsSuccess = false, ErrorMsg = "该镇没有任何人注册!" };
                    }
                    return db.ExecuteSql(insertSql.ToString()) > 0 ? new BaseResult { IsSuccess = true, ErrorMsg = "数据检测成功!" } : new BaseResult { IsSuccess = false, ErrorMsg = "插入失败!" };
                }
                return new BaseResult { IsSuccess = false, ErrorMsg = "删除数据失败!" };
            }
        }
        /// <summary>
        /// 获取消息统计列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<SumMessagePersonReadModel> GetMessageReadStateListSum(RouteGetMessageReadStateListSum request)
        {
            using (var db=DbFactory.Open())
            {
                try
                {
                    IGradelevelFactory factory = GetFactory(request.adcd);
                    return factory.GetMessageReadStateListSum(request, db);
                }
                catch (Exception ex)
                {
                    return null;
                }
                
            }
        }
        private IGradelevelFactory GetFactory(string adcd)
        {
            switch (AdcdHelper.GetByAdcdRole(adcd))
            {
                case "省级":
                    return new ProvinceFactory();
                case "市级":
                    return new CityFactory();
                case "县级":
                    return new CountryFactory();
                case "镇级":
                    return new TownFactory();
                case "村级":
                    return new VillageFactory();
                default:
                    throw new Exception("系统错误");

            }
        }
    }
}
