using System.IO;
using System.Threading.Tasks;
using PluginAPI.Core;
using SCPSLAudioApi;
using PluginAPI.Core.Attributes;

namespace SCPKaraoke
{
    public class Plugin
    {
        public static Plugin Singleton;

       [PluginConfig] public Config Config;
        
        // Plugin version
        private const string Version = "1.0.0";

        [PluginEntryPoint("SCPKaraoke", Version, "Lets people sing along to music!", "Un!")]
        void LoadPlugin()
        {
            Singleton = this;
            
            if (!Config.IsEnabled)
                return;
            Startup.SetupDependencies();
            PluginAPI.Events.EventManager.RegisterEvents(this);
            Log.Info("Making directory!");
            Log.Info("Making directory!");
            var directory = Path.Combine(Path.GetDirectoryName(PluginHandler.Get(Plugin.Singleton).MainConfigPath)!,
                "songs");
            Log.Info(directory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Log.Info("Getting FFMPEG");
            Task.Run(async () => await new Ffmpeg().DownloadFfmpeg(Path.Combine(Path.GetDirectoryName(PluginHandler.Get(Plugin.Singleton).MainConfigPath)!, "ffmpeg"))).Wait();
            Log.Info("Got FFMPEG!");

            Log.Warning($"SCPKaraoke {Version} is ready for people to sing in!");
        }
    }
}