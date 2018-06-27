using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Town
{
    public class TownLiableInfoResponse
    {
        public string Name { get; set; }

        public string Position { get; set; }

        public string ReceiveUserPhone { get; set; }

        public DateTime SendTime { get; set; }
    }
}
