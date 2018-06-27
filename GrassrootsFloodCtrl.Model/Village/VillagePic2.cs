using Dy.Common;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Village
{
    public class VillagePic2
    {
        public string id { get; set; }
        public string Adcd { get; set; }
        public string PicName { get; set; }
        public int isDelete { get; set; }
        public int Year { get; set; }
        public string operateLog { get; set; }
        public DateTime CreatTime { get; set; }
        public int AuditNums { get; set; }
    }
}
