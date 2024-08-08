using System.ComponentModel;
using Exiled.API.Interfaces;


namespace SCPKaraoke
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; }

        [Description("Your arl to a Deezer account (this is only ever sent to deezer) can be any account of any level (even free)")]
        public string DeezerArl { get; set; } = "";
        
    }
}