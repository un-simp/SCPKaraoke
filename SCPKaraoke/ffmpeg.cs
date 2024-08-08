using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Exiled.API.Features.Core.Generic;
using UnityEngine;
using MEC;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace SCPKaraoke
{
    public class Ffmpeg 
    {

        public IEnumerator<float> DownloadFfmpegCoroutine(string ffmpegPath)
        {
            yield return Timing.WaitUntilDone(DownloadFfmpegAsync(ffmpegPath));
        }

        private IEnumerator<float> DownloadFfmpegAsync(string ffmpegPath)
        {
            var task = Task.Run(async () =>
            {
                DirectoryInfo ffmpeg = Directory.CreateDirectory(ffmpegPath);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
                FFmpeg.SetExecutablesPath(ffmpegPath);
            });

            while (!task.IsCompleted)
            {
                yield return Timing.WaitForOneFrame;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("FFmpeg download failed: " + task.Exception);
            }
            else
            {
                Debug.Log("FFmpeg download completed successfully.");
            }
        }

     

        public IEnumerator<float> ConvertToOggCoroutine(string inputFilePath, string outputFilePath)
        {
            yield return Timing.WaitUntilDone(ConvertToOggAsync(inputFilePath, outputFilePath));
        }

        private IEnumerator<float> ConvertToOggAsync(string inputFilePath, string outputFilePath)
        {
            var task = Task.Run(async () =>
            {
                var conversion = FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{inputFilePath}\"")
                    .AddParameter("-acodec libvorbis")
                    .AddParameter("-ac 1")
                    .AddParameter("-ar 48k")
                    .SetOutput(outputFilePath);

                await conversion.Start();
            });

            while (!task.IsCompleted)
            {
                yield return Timing.WaitForOneFrame;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Conversion to OGG failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Conversion to OGG completed successfully.");
            }
        }
    }
}