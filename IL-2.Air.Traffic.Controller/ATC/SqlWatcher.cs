using IL_2.Air.Traffic.Controller.Data;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace IL_2.Air.Traffic.Controller.ATC
{
    class SqlWatcher
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        private string ConnectionString { get; set; }
        /// <summary>
        /// Конструктор по дофеолту
        /// </summary>
        public SqlWatcher()
        {
            ConnectionString = AppSet.Config.ConnectionString;
            StartWatching();
        }
        /// <summary>
        /// Общий метод запуск мониторинга
        /// </summary>
        private void StartWatching()
        {
            try
            {
                SqlDependency.Stop(ConnectionString);
                SqlDependency.Start(ConnectionString);
                ExecuteWatchingQuerySpeech();
                Console.WriteLine("Database monitoring is activated");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start monitoring DB tables." + Environment.NewLine + ex.Message );
            }
        }
        /// <summary>
        /// Подписка на изменения в таблице dbo.Speech
        /// </summary>
        private void ExecuteWatchingQuerySpeech()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("select Emotion from dbo.Speech", connection))
                {
                    var sqlDependency = new SqlDependency(command);
                    sqlDependency.OnChange += OnDatabaseChange_Speech;
                    command.ExecuteReader();
                }
            }
        }
        /// <summary>
        /// Событие вызывается когда в таблице БД появилась новая запись.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDatabaseChange_Speech(object sender, SqlNotificationEventArgs e)
        {
            if (SqlNotificationInfo.Insert.Equals(e.Info))
            {
                ExpertDB db = new ExpertDB();
                var state = db.Speech.ToList();
                foreach(var item in state)
                {
                    var ent = db.Speech.First(x => x.id == item.id);
                    Console.WriteLine(DateTime.Now + " |INFO| " + ent.RecipientMessage + " " + ent.Message +  " " + ent.Frequency + " " + ent.Lang);
                    Program.QSpeech.Enqueue(item);
                    db.Speech.Remove(ent);
                }
                db.SaveChanges();
                db.Dispose();
            }
            ExecuteWatchingQuerySpeech();
        }
    }
}
