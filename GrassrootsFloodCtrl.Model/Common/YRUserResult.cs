using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Common
{
    public class YRUserResult
    {
        public string result { get; set; }
        public string errmsg { get; set; }
        public string total { get; set; }
        public List<YRSSODept> user { get; set; }
    }
}
