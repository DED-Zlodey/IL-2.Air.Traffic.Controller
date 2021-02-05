using IL_2.Air.Traffic.Controller.ATC;
using IL_2.Air.Traffic.Controller.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IL_2.Air.Traffic.Controller
{
    class Program
    {
        private static AirDispatcher dispatcher;
        public static Queue<Speech> QSpeech = new Queue<Speech>();
        public static bool Occupied { get; set; } = true;
        static void Main(string[] args)
        {
            AppSet.SetUp();
            if(AppSet.ValidationConfig)
            {
                dispatcher = new AirDispatcher();
                while (string.IsNullOrEmpty(dispatcher.IamToken))
                {

                }
                Timer t = new Timer(TimerCallback, null, 0, 1000);
                Console.WriteLine("Waiting for a task...............");
            }
            Console.ReadKey(true);
        }
        private static void TimerCallback(Object o)
        {
            if(Occupied)
            {
                if(QSpeech.Count > 0)
                {
                    Occupied = false;
                    Console.WriteLine(DateTime.Now);
                    var ent = QSpeech.Dequeue();
                    Action actionwDB = () =>
                    {
                        dispatcher.Tts(ent);
                    };
                    Task taskwDB = Task.Factory.StartNew(actionwDB);
                }
            }
        }
    }
}
