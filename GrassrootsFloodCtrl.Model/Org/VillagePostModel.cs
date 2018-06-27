using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Org
{
    public class VillagePostModel
    {
        public List<PostVillageUserModel> administrator { get; set; }
        public List<PostVillageUserModel> connector { get; set; }
        public List<PostVillageUserModel> rescueAndRescueTeam { get; set; }
        public List<PostVillageUserModel> personnelTransferGroup { get; set; }
        public List<PostVillageUserModel> inspector { get; set; }
        public List<PostVillageUserModel> warntor { get; set; }
    }
}
