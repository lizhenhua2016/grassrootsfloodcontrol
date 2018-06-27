using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Supervise
{
    public class GetDutyInfoByWarnInfoIdViewModel
    {
        public string Adnm { get; set; }

        public int ShouldNum { get; set; }

        public int IsDutyNum { get; set; }

        public int NotDutyNum { get; set; }

        public string Remark { get; set; }

        public int IsSignNum { get; set; }

        public int NotSignNum { get; set; }

        public int TranferNum { get; set; }

        public string Adcd { get; set; }

        public int AdcdId { get; set; }

        public int WarnInfoId { get; set; }

        public int PAdcdId { get; set; }
    }
}
