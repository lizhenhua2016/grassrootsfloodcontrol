using GrassrootsFloodCtrl.Model.SumAppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Logic.Common
{
    public class ArrangeHelper
    {
        /// <summary>
        /// 省级排序
        /// </summary>
        /// <param name="list"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static List<SelectSumAppUserList> GetList(List<SelectSumAppUserList> list)
        {
            List<SelectSumAppUserList> arrangeList = new List<SelectSumAppUserList>();
            List<SelectSumAppUserList> parentList = list.FindAll(x => x.parentId == 0).ToList();
            foreach (var item in parentList)
            {
                //查找放入第一个市
                arrangeList.Add(item);
                //查找县级的统计
                List<SelectSumAppUserList> countryList = list.FindAll(x => x.parentId == item.adcdId);
                foreach (var country in countryList)
                {
                    //查找县下面的镇的统计
                    arrangeList.Add(country);
                    List<SelectSumAppUserList> townList = list.FindAll(x => x.parentId == country.adcdId);
                    arrangeList.AddRange(townList);
                }
            }
            return arrangeList;
        }
    }
}
