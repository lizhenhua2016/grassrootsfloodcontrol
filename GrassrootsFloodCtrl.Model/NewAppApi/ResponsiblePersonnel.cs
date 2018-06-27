using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.NewAppApi
{
    public class ResponsiblePersonnel
    {
        public string ResponsiblePersonnelId { get; set; }
        public string ResponsiblePersonnelName { get; set; }
        public string ResponsiblePersonnelMobile { get; set; }
        public string AdcdId { get; set; }
        public string ResponsiblePersonnelDepartment { get; set; }
        public string ResponsiblePersonnelPosition { get; set; }
        public bool IsAudit { get; set; }
        public bool IsLeader { get; set; }
        public bool IsDeputy { get; set; }
        public bool IsOrdinary { get; set; }
        public string CreateTime { get; set; }
        public string PassWord { get; set; }
        public bool IsRegApp { get; set; }
    }
}
