using Dy.Common;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Supervise
{
    public class CountyTransInfo
    {
        [field("自增ID", "int", null, null)]
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        
        [field("县名", "string", null, null)]
        public string adnm { get; set; }

        [field("行政编码", "string", null, null)]
        public string adcd { get; set; }

        [field("需转移人数", "int", null, null)]
        public int Transfernum { get; set; }

        [field("责任人数", "int", null, null)]
        public int Personnum { get; set; }

        [field("行政区域id", "int", null, null)]
        public int AdcdId { get; set; }


        public int PAdcdId { get; set; }

    }
}
