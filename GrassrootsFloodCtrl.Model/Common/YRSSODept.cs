using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Common
{
    [Serializable]
    public class YRSSODept : List<Object>
    {
        public string officefax { get; set; }
        public string birthday { get; set; }
        public string userid { get; set; }
        public string usertitle { get; set; }
        public string officialtype { get; set; }
        public string city { get; set; }
        public string loginname { get; set; }
        public string username { get; set; }
        public string loginpwd { get; set; }
        public string useable { get; set; }
        public string officephone { get; set; }
        public string province { get; set; }
        public string homephone { get; set; }
        public string orgcoding { get; set; }
        public string extendorgcoding { get; set; }
        public string officeid { get; set; }
        public string postcode { get; set; }
        public string homeaddress { get; set; }
        public string country { get; set; }
        public string officenum { get; set; }
        public string headpicture { get; set; }
        public string officeaddress { get; set; }
        public string idnum { get; set; }
        public string email { get; set; }
        public string userposition { get; set; }
        public string orderby { get; set; }
        public string virtualnum { get; set; }
        public string mobile { get; set; }
    }
}
