using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Supervise
{
    public class EmergencyStartCityListViewModel
    {
        public int AdcdId { get; set; }

        public int PAdcdId { get; set; }

        public string AdcdCode { get; set; }

        public string Adnm { get; set; }

        public int IsStart { get; set; }
    }
}
