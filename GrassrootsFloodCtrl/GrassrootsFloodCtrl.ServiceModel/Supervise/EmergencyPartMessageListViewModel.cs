using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.ServiceModel.Supervise
{
    public class EmergencyPartMessageListViewModel
    {
        public int Id { get; set; }

        public string EventName { get; set; }

        public int AdcdId { get; set; }

        public string StartTime { get; set; }

        public string Adnm { get; set; }

        public int Transfernum { get; set; }

        public int Personnum { get; set; }

        public string UserName { get; set; }
    }
}