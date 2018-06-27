namespace GrassrootsFloodCtrl.Model
{
    public class UserPosition
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string adcd { get; set; }
        public string Position { get; set; }
        //public bool IsSend { get; set; }
        public int TotalNum { get; set; }
    }

    public class UserPostionList
    {
        public int id { get; set; }
        public string UserName { get; set; }

        public string UserMobile { get; set; }
        public string Adcd { get; set; }
        public string UserPestion { get; set; }
        public int ToatalNum { get; set; }

        public bool IsSend { get; set; }
    }
}