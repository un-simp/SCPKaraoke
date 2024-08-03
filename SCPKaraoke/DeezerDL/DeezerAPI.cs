using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginAPI.Core;

namespace SCPKaraoke.DeezerDL
{
    public class DeezerAPI
    {
        private readonly DeezerDRM _deezerDrm;
        private readonly HttpClient _client;

        public class SongData
        {
            [JsonProperty("LYRICS_ID")] public int LyricsId { get; set; }

            [JsonProperty("MD5_ORIGIN")] public string Md5Origin { get; set; }

            [JsonProperty("FILESIZE_MP3_320")] public string FileSizeMp3320 { get; set; }

            [JsonProperty("FILESIZE_MP3_256")] public string FileSizeMp3256 { get; set; }
            [JsonProperty("SNG_TITLE")] public string SongTitle { get; set; }
            [JsonProperty("ART_NAME")] public string SongArtist { get; set; }
            [JsonProperty("ALB_TITLE")] public string SongAlbum { get; set; }
            [JsonProperty("DURATION")] public string Duration { get; set; }

            

            [JsonIgnore]
            public int MediaVersion
            {
                get
                {
                    if (!string.IsNullOrEmpty(FileSizeMp3320) && FileSizeMp3320 != "0")
                    {
                        return 3;
                    }

                    if (!string.IsNullOrEmpty(FileSizeMp3256) && FileSizeMp3256 != "0")
                    {
                        return 5;
                    }

                    return 1;
                }
            }
        }

        public class Root
        {
            [JsonProperty("DATA")] public SongData Data { get; set; }
        }

        public DeezerAPI(string arl)
        {
            HttpClientHandler handler = new HttpClientHandler();
            CookieContainer cookieContainer = new CookieContainer();
            _client = new HttpClient(handler);
            _deezerDrm = new DeezerDRM();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.CookieContainer = cookieContainer;
            // Set up headers
            var headers = new Dictionary<string, string>
            {
                { "Pragma", "no-cache" },
                { "Origin", "https://www.deezer.com" },
                { "Accept-Encoding", "gzip, deflate, br" },
                { "Accept-Language", "en-US,en;q=0.9" },
                {
                    "User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36"
                },
                { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
                { "Accept", "*/*" },
                { "Cache-Control", "no-cache" },
                // { "X-Requested-With", "XMLHttpRequest" },
                { "Connection", "keep-alive" },
                { "Referer", "https://www.deezer.com/login" },
                { "DNT", "1" }
            };

            foreach (var header in headers)
            {
                _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }


            cookieContainer.Add(new Uri("https://www.deezer.com"),
                new Cookie("arl",
                    $"{arl}"));
            cookieContainer.Add(new Uri("https://www.deezer.com"), new Cookie("comeback", "1"));

        }

        private HttpClient Client => _client;
        
/// <summary>
/// 
/// </summary>
/// <param name="songid"></param>
/// <returns>{ Md5Origin, LyricsId, MediaVersion, SongTitle,SongAlbum,SongArtist,Duration }</returns>
/// <exception cref="DeezerClientException"></exception>
        public async Task<List<object>> GetInfo(uint songid)
        {
            string url = $"https://www.deezer.com/en/track/{songid.ToString()}";
            Log.Warning(url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string htmlContent = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(htmlContent);
            string pattern = @"{""DATA"":.*}";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                // Extract the JSON data from the <script> tag
                string jsonData = match.Value;
                Console.WriteLine(jsonData);
                Root root = JsonConvert.DeserializeObject<Root>(jsonData);
                if (root.Data.Md5Origin == null)
                {
                    throw new DeezerClientException("You are not logged in! Set config and try again");
                }

                return new List<object> { root.Data.Md5Origin, root.Data.LyricsId, root.Data.MediaVersion, root.Data.SongTitle,root.Data.SongAlbum,root.Data.SongArtist,root.Data.Duration };
            }

            throw new DeezerClientException(
                "Could not find object for songData, make sure you are able to access deezer.com");
        }
        private DeezerDRM DRM => _deezerDrm;
        public async Task DownloadSong(uint songId, string outPath, List<Object> songInfo = null)
        {
            List<object> songData;
            if (songInfo == null)
            {
                 songData = await GetInfo(songId);
            }
            else
            {
                 songData = songInfo;
                 
            }
            string bfKey = DRM.CalcBlowfish(songId);
            string url = DRM.GenUrLdata(songId, songData[0] as string, (int)songData[1], (int)songData[2]);
            Console.Write(url);
            await DownloadAudio(url, bfKey,outPath);
        }

        private async Task DownloadAudio(string url, string key, string outPath)
        {
            var file = File.OpenWrite(Path.Combine(outPath,"out.mp3"));
            int currentBlock = 0; // what block we are on rn
            using HttpResponseMessage response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            {
                byte[] buffer = new byte[2048];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    // Console.WriteLine($"about to decrypt chunk {currentBlock}");
                    byte[] decryptedChunk = DRM.DecryptChunk(buffer, bytesRead, currentBlock, key);
                    file.Write(decryptedChunk, 0, decryptedChunk.Length);
                    currentBlock++;
                }
            }
        }

    }
}
public class DeezerClientException : Exception
{
    public DeezerClientException() { }

    public DeezerClientException(string message) 
        : base(message) { }

    public DeezerClientException(string message, Exception inner) 
        : base(message, inner) { }
}