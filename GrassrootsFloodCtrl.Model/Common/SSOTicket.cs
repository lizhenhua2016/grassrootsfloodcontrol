using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Common
{
    public class SSOTicket
    {
        public string result { get; set; }
        public string errmsg { get; set; }
        public string authcode { get; set; }
        public string loginname { get; set; }
        public string username { get; set; }
        public string orgcoding { get; set; }
        public string maploginname { get; set; }
        public string memo { get; set; }
    }
}
