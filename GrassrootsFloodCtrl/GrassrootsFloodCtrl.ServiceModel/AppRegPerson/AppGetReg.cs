using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel
{
   public class AppGetReg
    {
        public string id { get; set; }
        public string Mobile { get; set; }
        public string UserName { get; set; }
        public int AdcdId { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
