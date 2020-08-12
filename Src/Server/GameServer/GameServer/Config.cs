using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace GameServer
{
    class Config
    {
        class ConfigData
        {
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }

            public string DBServerIP { get; set; }
            public int DBServerPort { get; set; }
            public string DBUser { get; set; }
            public string DBPass { get; set; }
        }

        static ConfigData conig;

        public static string ServerIP { get { return conig.ServerIP; } }
        public static int ServerPort { get { return conig.ServerPort; } }

        public static string DBServerIP { get { return conig.DBServerIP; } }
        public static int DBServerPort { get { return conig.DBServerPort; } }
        public static string DBUser { get { return conig.DBUser; } }
        public static string DBPass { get { return conig.DBPass; } }


        public static void LoadConfig(string filename)
        {
            string json = File.ReadAllText(filename);
            conig = JsonConvert.DeserializeObject<ConfigData>(json);
        }

    }
}
