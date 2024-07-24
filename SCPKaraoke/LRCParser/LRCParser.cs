using System.Collections.Generic;
using System.IO;
using PluginAPI.Core;
namespace SCPKaraoke.LRCParser
{
    public class LrcParser
    {
        private readonly string _filePath;
        private string[] _loadedLyricFile;


        public LrcParser(string filePath)
        {
            _filePath = filePath;
            LoadLrc(); // Load lyrics upon instantiation
        }

        private void LoadLrc()
        {
            try
            {
                _loadedLyricFile = File.ReadAllLines(_filePath);
            }
            catch (IOException ex)
            {
                Log.Error("Error reading the file: " + ex.Message);
                _loadedLyricFile = null;
            }
        }

        public int GetNumberOfLyrics()
        {
            return _loadedLyricFile.Length;
    }

        public string GetLyricFromTime(string timeCode)
        {
            if (_loadedLyricFile == null || _loadedLyricFile.Length == 0) return null;

            List<string> lyricsFound = new List<string>();

            foreach (string line in _loadedLyricFile)
            {
                if (line.StartsWith(timeCode))
                {
                    string lyric = line.Substring(10).Trim();
                    if (!string.IsNullOrEmpty(lyric))
                    {
                        lyricsFound.Add(lyric);
                    }
                }
            }

            return lyricsFound.Count == 0 ? null : string.Join("\n", lyricsFound);
        }

        public string[] GetLyricFromNumber(int lyricNumber)
        {
            if (_loadedLyricFile == null || lyricNumber < 0 || lyricNumber >= _loadedLyricFile.Length)
            {
                return new[] { "null", "null" };
            }

            var timeCode = _loadedLyricFile[lyricNumber].Substring(0, 10).Trim();
            var lyric = _loadedLyricFile[lyricNumber].Substring(10).Trim();

            return new[] { timeCode, string.IsNullOrEmpty(lyric) ? "\u266b" : lyric };
        }
    }
}