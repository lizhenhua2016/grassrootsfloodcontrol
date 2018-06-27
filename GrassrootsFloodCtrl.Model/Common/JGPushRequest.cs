using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Common
{
    public class JGPushRequest
    {
        public string token { get; set; }
        public string[] members { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string mod { get; set; }
        public string param { get; set; }
        public bool production { get; set; }
    }
}
