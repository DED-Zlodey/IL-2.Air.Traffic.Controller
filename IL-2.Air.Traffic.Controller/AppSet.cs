using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace IL_2.Air.Traffic.Controller
{
    public static class AppSet
    {
        /// <summary>
        /// Основные параметры программы
        /// </summary>
        public static ConfigJson Config { get; private set; }
        /// <summary>
        /// Если true файл конфигурации не содержит дефолтных значений, если false программа не сконфигурирована
        /// </summary>
        public static bool ValidationConfig { get; private set; } = true;
        /// <summary>
        /// Конфигурирование программы согласно файлу конфигурации config.json
        /// </summary>
        public static void SetUp()
        {
            CreateConfigFile();
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            Config = JsonConvert.DeserializeObject<ConfigJson>(json);
            if(Config.AccessToken.Equals("<AccessToken>"))
            {
                ValidationConfig = false;
            }
            if (Config.FolderId.Equals("<FolderId>"))
            {
                ValidationConfig = false;
            }
            if(!ValidationConfig)
            {
                Console.WriteLine("Attention!!! The configuration file does not contain user data!!!");
            }
        }
        /// <summary>
        /// Если файл конфигурации config.json отсутствует, он будет создан с дефолтными параметрами
        /// </summary>
        private static void CreateConfigFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "config.json";
            if(!File.Exists(path))
            {
                string defaultjsonstr = "{ \"ConnectionString\": \"Data Source=.\\\\SQLEXPRESS;Initial Catalog=Expert;Integrated Security=True;\",\"FolderId\": \"<FolderId>\",\"AccessToken\": \"<AccessToken>\",\"DirSRS\": \"D:\\\\SRS\\\\IL2-SRS\" }";
                File.WriteAllText(path, defaultjsonstr);
            }
        }
    }
}
