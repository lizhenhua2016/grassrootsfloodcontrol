using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.NewAppApi
{
    public class ResponsibleAdcdDo
    {
        public string AdcdId { get; set; }
        public string AdcdCode { get; set; }
        public string ResponsiblePersonnelName { get; set; }
        public bool IsLeader { get; set; }
    }
}
