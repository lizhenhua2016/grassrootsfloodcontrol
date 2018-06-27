using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.SumAppMessage
{
    public class AppSumTownModel
    {
        public string adnm { get; set; }
        public string sendMessageByUserName { get; set; }
        public DateTime receiveDateTime { get; set; }
        public string sendMessage { get; set; }
        public int appWarnEventId { get; set; }
        public string adcd { get; set; }
    }
}
