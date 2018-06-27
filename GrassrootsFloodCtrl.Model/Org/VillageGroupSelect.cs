using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Org
{
    public class VillageGroupSelect
    {
        public List<UserPosition> administrator { get; set; }
        public List<UserPosition> connector { get; set; }
        public List<UserPosition> rescueAndRescueTeam { get; set; }
        public List<UserPosition> personnelTransferGroup { get; set; }
        public List<UserPosition> inspector { get; set; }
        public List<UserPosition> warntor { get; set; }
    }
}
