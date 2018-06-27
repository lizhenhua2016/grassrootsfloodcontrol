using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel
{
   public class AppRegPersonCondition
    {
        //姓名
        public string username { get; set; }
        //手机号
       public string hanphone { get; set; }
        //adcdid
        public int? adcdid { get; set; }
        //镇adcd
        public string townadcd { get; set; }
        //县adcd
        public string countyadcd { get; set; }
        //村adcd
        public string villageadcd { get; set; }
        //年份
        public int? year { get; set; }
        //批量删除对象
        public List<AdcdItems> AdcdIds { get; set; }
    }
    public class AdcdItems
    {
        public string phone { get; set; }
        public int? adcdId { get; set; }
        public string username { get; set; }
        public string adcd { get; set; }
    }
}
