using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client
{
    public class MyConfig
    {
        public const Key DefaultTalkKey = Key.LeftCtrl;

        const string ConfigFileName = "ClientConfig.json";

        /// <summary>
        /// Talk key value is in Wpf Key enum, it's different than vk_key
        /// Besure to KeyInterop to do conversion
        /// </summary>
        public Key TalkKey { get; set; }

        public MyConfig()
        {
            InitDefaultValues();
        }

        public void InitDefaultValues()
        {
            if (TalkKey == Key.None)
            {
                TalkKey = DefaultTalkKey;
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static MyConfig Load()
        {
            if (!File.Exists(ConfigFileName))
            {
                var newConfig = new MyConfig();
                newConfig.Save();
            }

            var configFromFile = JsonConvert.DeserializeObject<MyConfig>(File.ReadAllText(ConfigFileName));
            return configFromFile;
        }
    }
}
