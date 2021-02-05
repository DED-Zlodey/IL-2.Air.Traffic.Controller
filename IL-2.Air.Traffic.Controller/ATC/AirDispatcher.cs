using IL_2.Air.Traffic.Controller.Data;
using NAudio.Lame;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IL_2.Air.Traffic.Controller.ATC
{
    class AirDispatcher
    {
        /// <summary>
        /// IAM-токен сервиса Yandex.Cloud
        /// </summary>
        public string IamToken { get; private set; }
        /// <summary>
        /// Дата истечения срока действия IAM-токена (Очевидно UTC). Время жизни IAM-токена — не больше 12 часов, но рекомендуется запрашивать его чаще, например каждый час.
        /// </summary>
        public DateTime ExpirationDateIamToken { get; private set; }
        /// <summary>
        /// Дата крайнего запроса IAM-токена. Время местное.
        /// </summary>
        public DateTime LastRequestIamToken { get; private set; }
        private SqlWatcher sqlWatcher;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public AirDispatcher()
        {
            sqlWatcher = new SqlWatcher();
            UpdateIamToken();
        }
        /// <summary>
        /// Обновляет IamToken. Время жизни IAM-токена — не больше 12 часов, но рекомендуется запрашивать его чаще, например каждый час.
        /// </summary>
        private async Task UpdateIamToken()
        {
            HttpClient client = new HttpClient();
            string url = "https://iam.api.cloud.yandex.net/iam/v1/tokens";
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{ yandexPassportOauthToken: \"" + AppSet.Config.AccessToken + "\" }", Encoding.UTF8, "application/json"),
                RequestUri = new Uri(url)
            };
            var response = await client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                LastRequestIamToken = DateTime.Now;
                var responseMessage = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseMessage);
                foreach (var item in json)
                {
                    if (item.Key == "iamToken")
                    {
                        IamToken = item.Value.ToString();
                    }
                    if (item.Key == "expiresAt")
                    {
                        ExpirationDateIamToken = DateTime.Parse(item.Value.ToString());
                    }
                }
                Console.WriteLine("IAM-Token Updatetd: " + LastRequestIamToken.ToShortDateString() + " " + LastRequestIamToken.ToLongTimeString());
            }
            else
            {
                Console.WriteLine("Error! Status Code: " + response.StatusCode);
            }
        }
        /// <summary>
        /// Связывается с сервисом Yandex.Cloud отправляет текст и получает в ответ файл, который передается в SRS
        /// </summary>
        /// <param name="speech">Объект содержащий данные для вывода голосового сообщения</param>
        /// <returns></returns>
        public async Task Tts(Speech speech)
        {
            await UpdateIamToken();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + IamToken);
            var speed = Math.Round(speech.Speed, 1);
            var values = new Dictionary<string, string>
            {
                { "text", speech.Message },
                { "voice", speech.Voice },
                { "emotion", speech.Emotion },
                { "speed", speed.ToString().Replace(",", ".") },
                { "lang", speech.Lang },
                { "format", "lpcm" },
                { "sampleRateHertz", "48000" },
                { "folderId", AppSet.Config.FolderId }
            };
            var sampleRate = 48000;
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var ms = new MemoryStream(responseBytes);
            var rs = new RawSourceWaveStream(ms, new WaveFormat(sampleRate, 16, 1));
            var outpath = "example.wav";
            WaveFileWriter.CreateWaveFile(outpath, rs);
            using (var reader = new AudioFileReader("example.wav"))
            using (var writer = new LameMP3FileWriter("speech.mp3", reader.WaveFormat, 192))
                reader.CopyTo(writer);
            SendToSRS(speech);
        }
        /// <summary>
        /// Отправляет в SRS файл speech.mp3
        /// </summary>
        /// <param name="speech"></param>
        private void SendToSRS(Speech speech)
        {
            var freq = Math.Round(speech.Frequency, 1).ToString();
            var coal = speech.Coalition;
            var pathMp3 = AppDomain.CurrentDomain.BaseDirectory + @"speech.mp3";
            var argstr = pathMp3 + " " + freq + " AM " + coal + " 6002 " + speech.NameSpeaker.Replace(" ", "-").Replace("   ", "-") + " 1.0";
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(AppSet.Config.DirSRS + @"\IL2-SRS-External-Audio.exe", argstr);
            processStartInfo.WorkingDirectory = AppSet.Config.DirSRS;
            processStartInfo.RedirectStandardOutput = true; //Выводить в родительское окно
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; // не создавать окно CMD
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            process.StartInfo = processStartInfo;
            process.Start();
            string text = process.StandardOutput.ReadToEnd();
            Console.WriteLine(text);
            process.WaitForExit();
            Console.WriteLine("End sending");
            Program.Occupied = true;
        }
    }
}
