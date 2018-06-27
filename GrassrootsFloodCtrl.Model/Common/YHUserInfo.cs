using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Common
{
    public class YHUserInfo
    {
        public string uid { get; set; }
        public string loginname { get; set; }
        public string username { get; set; }
        public string idnum { get; set; }
        public string nation { get; set; }
        public string sex { get; set; }
        public string birthday { get; set; }
        public string politicsface { get; set; }
        public string schools { get; set; }
        public string specialty { get; set; }
        public string education { get; set; }
        public string degree { get; set; }
        public string headpicture { get; set; }
        public string userstatus { get; set; }
        public string codeby { get; set; }
        public string title { get; set; }
        public string officeaddr { get; set; }
        public string officephone { get; set; }
        public UserOrg userORg { get; set; }
        public string mobilephone { get; set; }
        public string email { get; set; }
        public string mobilephone2 { get; set; }
        public string virtualnum { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string postcode { get; set; }
    }
    [Serializable]
    public class UserOrg:Object
    {
        public string uid { get; set; }
        public string oid { get; set; }
        public string jobids { get; set; }
        public string positionids { get; set; }
        public string orderby { get; set; }
    }
}
