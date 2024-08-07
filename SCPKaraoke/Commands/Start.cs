using CommandSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DeezerDL;
using PluginAPI.Core;
using Log = PluginAPI.Core.Log;

namespace SCPKaraoke.Commands
{
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Start : ICommand
{
    public string Command { get; } = "start";

    public string[] Aliases { get; } = new string[] { "s",};

    public string Description { get; } = "Starts karaoke, needs a song id.";
    private List<Object> _songInfo;
    private bool _ifSameChannel;


    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        string songs = Path.Combine(Path.GetDirectoryName(PluginHandler.Get(Plugin.Singleton).MainConfigPath)!,
            "songs");

        uint songID = Convert.ToUInt32(arguments.At(0));
        if (arguments.At(0) == null)
        {
            response = "No song id!";
            return true;
        }

        try
        {
            _ifSameChannel = Convert.ToBoolean(arguments.At(1));
        }
        catch (Exception)
        {
            _ifSameChannel = false;
        }
        

        Log.Warning(songID.ToString());
        Task.Run(async () =>
        {
            var deezerClient = new DeezerAPI(Plugin.Singleton.Config.DeezerARL);
            _songInfo = await deezerClient.GetInfo(songID);
            await deezerClient.DownloadSong(songID, Path.Combine(songs, "out.mp3"), _songInfo);
            await new Ffmpeg().ConvertToOgg(Path.Combine(songs, "out.mp3"), Path.Combine(songs, "out.ogg"));
            await deezerClient.DownloadLyrics(songID, Path.Combine(songs, "lyrics.lrc"));
        }).Wait();



        // next load lyrics
        // //i just send all the lyrics at once and tell the game to never clear the broadcast lists
        // // its scuffed. doesn't consider for other plugins which do clear the list and never will allow for stopping 
        // // but GOD its beautiful and works somehow
        // // THIS WILL BE REFACTORED HOLY FUCK
        // for (int i = 0; i < lrc.GetNumberOfLyrics(); i++)
        // {
        //     broadCastlul(lrc.GetLyricFromNumber(i)[1],2);
        // }
        KaraokeSync krc = new KaraokeSync(Path.Combine(songs, "lyrics.lrc"), Path.Combine(songs, "out.ogg"),_songInfo[3].ToString(), _songInfo[5].ToString(),_ifSameChannel);
        krc.StartSongAndLyrics();
        // krc.AnnounceSongThenPlay(10);
        response = "song has started!";
        return true;
    }


    // private async Task DownloadSongandLyrics(uint songID)
    // {
    //     var deezerClient = new DeezerAPI(Config.DeezerARL);
    //     var info =  await deezerClient.GetInfo(songID);
    //     await deezerClient.DownloadSong(songID);
    //     var lrclib = new LrcLibLyrics();
    //     await lrclib.DownloadLyrics(info[3].ToString() ,info[5].ToString(),info[6].ToString(),info[4].ToString());
    // }
    // private ReferenceHub SpawnBot(string name)
    // {
    //     var newPlayer = Object.Instantiate(NetworkManager.singleton.playerPrefab);
    //     var fakeconnection = new FakeNetworkConnection(0);
    //     var hub = newPlayer.GetComponent<ReferenceHub>();
    //     NetworkServer.AddPlayerForConnection(fakeconnection, newPlayer);
    //     hub.authManager.InstanceMode = ClientInstanceMode.Host;
    //
    //     try
    //     {
    //         hub.nicknameSync.SetNick(name);
    //     }
    //     catch (Exception)
    //     {
    //     }
    //
    //     return hub;
    // }

}



}