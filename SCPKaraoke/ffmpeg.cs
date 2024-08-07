
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
namespace SCPKaraoke
{
    public class Ffmpeg
    {
        public async Task DownloadFfmpeg(string ffmpegPath)
        {
            DirectoryInfo ffmpeg = Directory.CreateDirectory(ffmpegPath);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official,ffmpegPath);
            FFmpeg.SetExecutablesPath(ffmpegPath);
        }
        public async Task ConvertToOgg(string inputFilePath, string outputFilePath)
        {
            // Set the conversion options
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputFilePath}\"")
                .AddParameter("-acodec libvorbis")
                .AddParameter("-ac 1")
                .AddParameter("-ar 48k")
                .SetOutput(outputFilePath);

            // Start the conversion
            await conversion.Start();
        }
    }
}