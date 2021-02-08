using IL_2.Air.Traffic.Controller.ATC;
using IL_2.Air.Traffic.Controller.Data;
using IL_2.Air.Traffic.Controller.SRS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace IL_2.Air.Traffic.Controller
{
    class Program
    {
        private static AirDispatcher dispatcher;
        public static Queue<Speech> QSpeech = new Queue<Speech>();
        public static bool Occupied { get; set; } = true;
        public static ClientsSRS ClientsSRS { get; set; }
        static void Main(string[] args)
        {
            AppSet.SetUp();
            if(AppSet.ValidationConfig)
            {
                dispatcher = new AirDispatcher();
                while (string.IsNullOrEmpty(dispatcher.IamToken))
                {

                }
                Timer t = new Timer(TimerCallback, null, 0, 2000);
                Console.WriteLine("Waiting for a task...............");
            }
            Console.ReadKey(true);
        }
        private async static void TimerCallback(Object o)
        {
            if(Occupied)
            {
                if(QSpeech.Count > 0)
                {
                    Occupied = false;
                    Console.WriteLine(DateTime.Now);
                    var ent = QSpeech.Dequeue();
                    await dispatcher.Tts(ent);
                }
                else
                {
                    Occupied = false;
                    UpdateListSRSClients();
                    Occupied = true;
                }
            }
            GC.Collect();
        }
        private static void UpdateListSRSClients()
        {
            try
            {
                var json = string.Empty;
                using (var fs = File.OpenRead(AppSet.Config.DirSRS + @"\clients-list.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = sr.ReadToEnd();
                ClientsSRS = JsonConvert.DeserializeObject<ClientsSRS>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
