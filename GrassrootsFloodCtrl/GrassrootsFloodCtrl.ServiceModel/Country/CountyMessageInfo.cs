using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel
{
    public class CountyMessageInfo
    {
        public string Message { get; set; }

        public int WarnInfoId { get; set; }

        public int ShouldHereCount { get; set; }

        public int PostedCount { get; set; }

        public int PostingCount { get; set; }

        public int TotalTransferPerson { get; set; }
    }
}
