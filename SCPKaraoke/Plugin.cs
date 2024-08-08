using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MEC;
using SCPSLAudioApi;
using Log = Exiled.API.Features.Log;

namespace SCPKaraoke
{
    public class Plugin : Plugin<Config>
    {
        public Ffmpeg ffmpeg;
        public override string Name => "SCP Karaoke"; // The name of the plugin
        public override string Prefix => "KAR"; // Prefix for MyPlugin
        public override string Author => "un-simp"; // Author name
        public override Version Version => new Version(1, 0, 0); // Plugin version
        public override Version RequiredExiledVersion => new Version(9, 0, 0); // Minimum required Exiled version
        // We are not going to override IgnoreRequiredVersionCheck as it's set to false by default
        public static Plugin Singleton;
        public override void OnEnabled()
        {
            Log.Info($"{Name} has been enabled!");
            Startup.SetupDependencies();
            string ffmpegPath = Path.Combine(Path.GetDirectoryName(Singleton.ConfigPath)!, "ffmpeg");
            var directory = Path.Combine(Path.GetDirectoryName(Singleton.ConfigPath)!,
                "songs");
            Log.Info($"song directory created at {directory}");
            if (!Directory.Exists(directory))
            {
                Log.Info("Making songs directory");
                Directory.CreateDirectory(directory);
            }
            Log.Info("Getting FFMPEG");
            ffmpeg = new Ffmpeg();
            Timing.RunCoroutine(ffmpeg.DownloadFfmpegCoroutine(ffmpegPath));
            Log.Info("Got FFMPEG!");
            base.OnEnabled(); 
        }
    
        public override void OnDisabled()
        {
            Log.Info($"{Name} has been disabled!");
            base.OnDisabled();
        }

    }
}