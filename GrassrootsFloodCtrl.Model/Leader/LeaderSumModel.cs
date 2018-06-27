using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrassrootsFloodCtrl.Model.Leader
{
    public class LeaderSumModel
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public int? floodCount { get; set; }
        public int? villagePicCount { get; set; }
        public int? appUserCount { get; set; }
        public AdministrativeDivision administrativeDivision { get; set; }
        public HiddenRiskPoint hiddenRiskPoint { get; set; }
        public PersionLiableCount persionLiableCount { get; set; }
        public ManagementCount management { get; set; }
        public MessageRegistrationInfo messageRegistration { get; set; }
    }
    public class AdministrativeDivision
    {
        public int? countryCount { get; set; }
        public int? townCount { get; set; }
        public int? villageCount { get; set; }
    }
    public class HiddenRiskPoint
    {
        public int? pointPersonCount { get; set; }
        public int? dangerousCount { get; set; }
        public int? torrentialFloodCount { get; set; }
        public int? geologyCount { get; set; }
        public int? lowLyingCount { get; set; }
        public int? poolCount { get; set; }
        public int? dikeCount { get; set; }
        public int? seawallCount { get; set; }
        public int? otherCount { get; set; }
    }
    public class PersionLiableCount
    {
        public int? countryCount { get; set; }
        public int? townCount { get; set; }
        public int? villageCount { get; set; }
    }
    public class ManagementCount
    {
        public int? countryCount { get; set; }
        public int? managementCount { get; set; }
        public int? dutyCount { get; set; }
    }

    public class CityInfo {
        public string adnm { get; set; }
    }
    public class AllUserInfo {
        public int sumCount { get; set; }
    }
    public class RegistrationUserInfo {
        public int personCount { get; set; }
    }
    public class MessageRegistrationInfo {
        public List<string> cityList { get; set; }
        public List<int> allList { get; set; }
        public List<int> regList { get; set; }
    }

}
