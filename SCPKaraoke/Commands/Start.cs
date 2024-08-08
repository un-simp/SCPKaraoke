using CommandSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DeezerDL;
using Exiled.API.Features;
using MEC;

namespace SCPKaraoke.Commands
{
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Start : ICommand
{
    public string Command { get; } = "start";

    public string[] Aliases { get; } = { "s",};

    public string Description { get; } = "Starts karaoke, needs a song id.";
    private List<Object> _songInfo;
    private bool _ifSameChannel;


    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        string songs = Path.Combine(Path.GetDirectoryName(Plugin.Singleton.ConfigPath)!,
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
        

        Log.Info(songID.ToString());
        Timing.RunCoroutine(DownloadAndConvertSongAsync(songID,songs));
        

        // next load lyrics
        // im leaving this in cause its really fucking funny
        // //i just send all the lyrics at once and tell the game to never clear the broadcast lists
        // // its scuffed. doesn't consider for other plugins which do clear the list and never will allow for stopping 
        // // but GOD its beautiful and works somehow
        // // THIS WILL BE REFACTORED HOLY FUCK
        // for (int i = 0; i < lrc.GetNumberOfLyrics(); i++)
        // {
        //     broadCastlul(lrc.GetLyricFromNumber(i)[1],2);
        // }
        KaraokeSync krc = new KaraokeSync(Path.Combine(songs, "lyrics.lrc"), Path.Combine(songs, "out.ogg"),_songInfo[3].ToString(), _songInfo[5].ToString(),_ifSameChannel);
        krc.AnnounceSongThenPlay(10, Player.Get(sender));
        response = "song has started!";
        return true;
    }

    private IEnumerator<float> DownloadAndConvertSongAsync(uint songID, string songs)
    {
        
        var task = Task.Run(async () =>
        {
            var deezerClient = new DeezerAPI(Plugin.Singleton.Config.DeezerArl);
            _songInfo = await deezerClient.GetInfo(songID);
            await deezerClient.DownloadSong(songID, Path.Combine(songs, "out.mp3"), _songInfo);
            Timing.RunCoroutine(new Ffmpeg().ConvertToOggCoroutine(Path.Combine(songs, "out.mp3"), Path.Combine(songs, "out.ogg")));
            await deezerClient.DownloadLyrics(songID, Path.Combine(songs, "lyrics.lrc"));
        });

        while (!task.IsCompleted)
        {
            yield return Timing.WaitForOneFrame;
        }

        if (task.IsFaulted)
        {
            Log.Error("Song download and conversion failed: " + task.Exception);
        }
        else
        {
            Log.Info("Song download and conversion completed successfully.");
        }
    }
}
}