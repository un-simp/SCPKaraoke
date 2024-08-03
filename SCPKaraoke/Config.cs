using System.ComponentModel;
using System.IO;
using PluginAPI.Core;

namespace SCPKaraoke
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        [Description("Your arl to a Deezer account (this is only ever sent to deezer) can be any account of any level (even free)")]
        public string DeezerARL { get; set; } = "";
        
    }
}