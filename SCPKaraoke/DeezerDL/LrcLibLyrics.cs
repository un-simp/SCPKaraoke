using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SCPKaraoke.DeezerDL
{
    // interacts with LRCLIB to get synced lyrics if deezer fails (or right now the only source i cant fucking deal with deezer's api anymore)
    public class LrcLibLyrics
    {
        private readonly HttpClient _client;
        private HttpClient Client => _client;

        public LrcLibLyrics()
        {
            _client = new HttpClient();
            var headers = new Dictionary<string, string>
            {
                { "Pragma", "no-cache" },
                { "Origin", "https://www.lrclib.net" },
                { "Accept-Encoding", "gzip, deflate, br" },
                { "Accept-Language", "en-US,en;q=0.9" },
                {
                    "User-Agent",
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36 SCPKaraoke/0.0.1"
                },
                { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
                { "Accept", "*/*" },
                { "Cache-Control", "no-cache" },
                { "Connection", "keep-alive" },
                { "DNT", "1" }
            };
            foreach (var header in headers)
            {
                _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        /// <summary>
        ///  Downloads lyrics
        /// </summary>
        /// <param name="songName"> the name of the song</param>
        /// <param name="songArtist"> the performing artist</param>
        /// <param name="duration">length of song in seconds</param>
        /// <param name="songAlbum">the album its from (nullable will use the song name)</param>
        public async Task DownloadLyrics(string songName, string songArtist, string duration,string outPath, string songAlbum = null)
        {
            string baseUrl;
            switch (songAlbum)
            {
                case null:
                    baseUrl =
                        $"https://lrclib.net/api/get?track_name={Uri.EscapeDataString(songName)}&artist_name={Uri.EscapeDataString(songArtist)}&album_name={Uri.EscapeDataString(songAlbum)}&duration={duration}";
                    break;
                default:
                    baseUrl =
                        $"https://lrclib.net/api/get?track_name={Uri.EscapeDataString(songName)}&artist_name={Uri.EscapeDataString(songArtist)}&album_name={Uri.EscapeDataString(songName)}&duration={duration}";
                    break;
            }

            Console.WriteLine(baseUrl);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
            var response = await Client.SendAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new LrcClientException("This song has no lyrics on LRCLIB!");
            }
            response.EnsureSuccessStatusCode();
            string lyrics = await response.Content.ReadAsStringAsync();
            LyricsInfo lyricsInfo = JsonConvert.DeserializeObject<LyricsInfo>(lyrics);
            File.WriteAllText(outPath,lyricsInfo.SyncedLyrics);
        }
    }

    public class LyricsInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public double Duration { get; set; }
        public bool Instrumental { get; set; }
        public string PlainLyrics { get; set; }
        public string SyncedLyrics { get; set; }
    }
    public class LrcClientException : Exception
    {
        public LrcClientException() { }

        public LrcClientException(string message) 
            : base(message) { }

        public LrcClientException(string message, Exception inner) 
            : base(message, inner) { }
    }
}
