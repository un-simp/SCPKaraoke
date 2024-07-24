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
            Log.Warning($"SCPKaraoke {Version} is ready for people to sing in!");
        }
    }
}