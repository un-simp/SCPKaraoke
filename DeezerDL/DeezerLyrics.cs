using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeezerDL
{
    // interacts with deezer to get synced lyrics 
    public class DeezerLyrics
    {
        private readonly HttpClient _client;

        public DeezerLyrics(HttpClient client)
        {
            _client = client;

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.GetToken().Result);

        }

        private HttpClient Client => _client;

        public async Task DownloadLyrics(uint songId, string path)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "https://pipe.deezer.com/api");

            JObject trackId = new JObject
            {
                ["trackId"] = songId.ToString()
            };
            JObject queryObj = new JObject
            {
                ["query"] =
                    "query SynchronizedTrackLyrics($trackId: String!) {  track(trackId: $trackId) {    ...SynchronizedTrackLyrics    __typename  }}fragment SynchronizedTrackLyrics on Track {  id  lyrics {    ...Lyrics    __typename  }  album {    cover {      small: urls(pictureRequest: {width: 100, height: 100})      medium: urls(pictureRequest: {width: 264, height: 264})      large: urls(pictureRequest: {width: 800, height: 800})      explicitStatus      __typename    }    __typename  }  __typename}fragment Lyrics on Lyrics {  id  copyright  text  writers  synchronizedLines {    ...LyricsSynchronizedLines    __typename  }  __typename}fragment LyricsSynchronizedLines on LyricsSynchronizedLine {  lrcTimestamp  line  lineTranslated  milliseconds  duration  __typename}",
                ["variables"] = trackId
            };
            request.Content = new StringContent(queryObj.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                throw new LrcClientException($"Deezer took too long to respond! Error is: {e}");
            }
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject responseJson = JObject.Parse(responseBody);

            // Console.WriteLine(responseJson.ToString());
            JsonDeezerLyricsResponse lyrics =
                JsonConvert.DeserializeObject<JsonDeezerLyricsResponse>(responseJson.ToString());
            if (lyrics.Data.Track.Lyrics == null)
            {
                throw new LrcClientException("No lyrics on Deezer!");
            }
            var lrcFile = ParseLyrics(lyrics);
            File.WriteAllLines(path,lrcFile);
            // File.WriteAllText("lyrics.lrc", lyricsInfo.SyncedLyrics);
        }

        private List<string> ParseLyrics(JsonDeezerLyricsResponse lyrics)
        {
            List<string> lrcLyrics = new List<string>();
            foreach (var lyric in lyrics.Data.Track.Lyrics.SynchronizedLines)
            {
                lrcLyrics.Add($"{lyric.LrcTimestamp} {lyric.Line}");
            }

            return lrcLyrics;
        }
        private async Task<string> GetToken()
        {

            var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Post,
                "https://auth.deezer.com/login/arl?i=c&jo=p&rto=n"));
            var tokenJson = await response.Content.ReadAsStringAsync();
            Token token = JsonConvert.DeserializeObject<Token>(tokenJson);
            return token.jwt;
        }

        public class Token
        {
            [JsonProperty("jwt")] public string jwt { get; set; }

            [JsonProperty("refresh_token")] public string refresh_token { get; set; }
        }

        public class JsonDeezerLyricsResponse
        {
            [JsonProperty("data")] public DataResponse Data { get; set; }


            public class DataResponse
            {
                [JsonProperty("track")] public TrackResponse Track { get; set; }

                public class TrackResponse
                {
                    [JsonProperty("id")] public string Id { get; set; }

                    [JsonProperty("lyrics")] public LyricsResponse Lyrics { get; set; }

                    [JsonProperty("__typename")] public string Typename { get; set; }

                    public class LyricsResponse
                    {
                        [JsonProperty("id")] public string Id { get; set; }

                        [JsonProperty("copyright")] public string Copyright { get; set; }

                        [JsonProperty("text")] public string Text { get; set; }

                        [JsonProperty("writers")] public string Writers { get; set; }

                        [JsonProperty("synchronizedLines")]
                        public List<SynchronizedLineResponse> SynchronizedLines { get; set; }

                        [JsonProperty("__typename")] public string Typename { get; set; }

                        public class SynchronizedLineResponse
                        {
                            [JsonProperty("lrcTimestamp")] public string LrcTimestamp { get; set; }

                            [JsonProperty("line")] public string Line { get; set; }

                            [JsonProperty("lineTranslated")] public object LineTranslated { get; set; }

                            [JsonProperty("milliseconds")] public int Milliseconds { get; set; }

                            [JsonProperty("duration")] public int Duration { get; set; }

                            [JsonProperty("__typename")] public string Typename { get; set; }
                        }
                    }
                }
            }
        }

    }
}
