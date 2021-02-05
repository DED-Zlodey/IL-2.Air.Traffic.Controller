using Newtonsoft.Json;

namespace IL_2.Air.Traffic.Controller
{
    public struct ConfigJson
    {
        /// <summary>
        /// Строка подключения к SQL серверу
        /// </summary>
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Id каталога в сервисе Yandex.Cloud
        /// </summary>
        [JsonProperty("FolderId")]
        public string FolderId { get; private set; }
        /// <summary>
        /// OAuth-токен в сервисе Яндекс.OAuth 
        /// </summary>
        [JsonProperty("AccessToken")]
        public string AccessToken { get; private set; }
        /// <summary>
        /// Директория в которой лежит SRS server
        /// </summary>
        [JsonProperty("DirSRS")]
        public string DirSRS { get; private set; }
    }
}
