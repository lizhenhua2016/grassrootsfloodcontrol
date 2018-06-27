using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RedisHelper
{
    public sealed class RedisConfig : ConfigurationSection
    {
       
        public static string WriteServerConStr{
            get {
                return string.Format("{0},{1}","127.0.0.1:6379","127.0.0.1:6380");
            }
        }
        public static string ReadServerConStr
        {
            get
            {
                return  string.Format("{0}", "127.0.0.1:6379");
            }
        }
        public static int MaxWritePoolSize
        {
            get
            {
                return 50;
            }
        }
        public static int MaxReadPoolSize
        {
            get
            {
                return 200;
            }
        }
        public static bool AutoStart
        {
            get
            {
                return true;
            }
        }

    }
}
