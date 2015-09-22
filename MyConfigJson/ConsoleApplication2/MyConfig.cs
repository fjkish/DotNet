using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ConsoleApplication2
{
    public static class MyConfig
    {
        private static _JsonConfigValues _jsonConfigValues;
        private static ConfigValues _configValues;

        private static _JsonConfigValues LoadJson()
        {
            var strAppDir = AppDomain.CurrentDomain.BaseDirectory;

            var configFile = Path.Combine(strAppDir, "Config.json");

            Console.WriteLine(configFile);

            using (var r = new StreamReader(configFile)) {
                var json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<_JsonConfigValues>(json);
            }
        }

        public static ConfigValues GetConfigValues()
        {
            if (_jsonConfigValues == null) {
                _jsonConfigValues = LoadJson();
            }
            if (_configValues == null) {
                _configValues = new ConfigValues(_jsonConfigValues);
            }


            return _configValues;
        }

        public class _JsonConfigValues
        {
            public DateTime datetime;
            public int dir1;
            public string filename;
            public List<string> mylist;
        }

        public class ConfigValues
        {
            public ConfigValues(_JsonConfigValues jsonConfigValues)
            {
                dir1 = jsonConfigValues.dir1;
                filename = jsonConfigValues.filename;
                datetime = jsonConfigValues.datetime;
                mylist = jsonConfigValues.mylist;
            }

            public int dir1 { get; private set; }
            public string filename { get; private set; }
            public DateTime datetime { get; private set; }
            public List<string> mylist { get; private set; }
        }
    }
}