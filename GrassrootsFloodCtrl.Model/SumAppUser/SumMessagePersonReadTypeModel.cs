using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.SumAppUser
{
    public class SumMessagePersonReadTypeModel
    {
        public string reciveAdcd { get; set; }
        public string adnm { get; set; }
        public int? messageCount { get; set; }
        public int readtype { get; set; }
        public int grade { get; set; }
        public int id { get; set; }
        public int parentId { get; set; }
        public string sendMessage { get; set; }
        public string receiveUserName { get; set; }
    }
}
