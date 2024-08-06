using System.Collections.Generic;
using System.IO;
using System.Timers;
using CentralAuth;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
// wtf is this naming this is my fault its just so shit can interaction with the other shit yk how it goes
using SCPKaraoke.LRCParser;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using VoiceChat;

namespace SCPKaraoke
{
    public class KaraokeSync
    {
        private readonly LrcParser _lrc;
        private readonly AudioPlayerBase _audiosource;
        private List<System.Object> _botList;
        private ReferenceHub _bot;
        private string _botName;
        private int _nextLyric;
        private string _lrcPath;
        private string _songPath;
        private string _songName;

        private string _songArtist;
        // hopefully only one instance is made crossing fingers if not god is dead
        public static KaraokeSync Current { get; private set; }


        
        
        public KaraokeSync(string LRCPath, string songPath,string songName, string songArtist)
        {
            _lrc = new LrcParser(LRCPath);
            _lrcPath = LRCPath;
            _songPath = songPath;
            _songArtist = songArtist;
            _songName = songName;
            _botList = SpawnBot($"{_songName} - {songArtist}");
            _bot = _botList[1] as ReferenceHub;
            _botName = _botList[0] as string;
            _audiosource  = AudioPlayerBase.Get(_bot);
            _audiosource.CurrentPlay = songPath;
            Current = this;
        }

                [PluginEvent(ServerEventType.RoundEnd)]
        private void EndPrematurely()
        {
            EndSong();
            broadCastlul("The song has ended prematurely due to a round end event", 10);
        }

        private AudioPlayerBase AudioPlayer => _audiosource;

        public  void AnnounceSongThenPlay(int timeToStart)
        {
            Log.Info(_songPath);
            Log.Info(_lrcPath);
            Cassie.Clear();
            Cassie.Message("10 seconds");
            broadCastlul($"<b><size=32>Karaoke for {_songName} by {_songArtist} will start in {timeToStart.ToString()} seconds! \n Open your Console and type \".karaoke optout\" to opt out.",timeToStart);
            Timer timer = new Timer(timeToStart *1000);
            timer.AutoReset = false;
            timer.Elapsed += TimerOnElapsed;
            timer.Enabled = true;

        }
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            StartSongAndLyrics();
        }
        public void StartSongAndLyrics()
        {
            
            // subscribe to update thread so our other code isnt running all the fucking time and only after this has been run
            StaticUnityMethods.OnUpdate += StaticUnityMethodsOnOnUpdate;
            _nextLyric = 0;
            AudioPlayer.BroadcastChannel = VoiceChatChannel.RoundSummary;
            AudioPlayer.AllowUrl = false;
            AudioPlayer.LogDebug = true;
            AudioPlayer.Play(-1);
            // so that the bot will end itself after the song is done
            AudioPlayerBase.OnFinishedTrack += AudioPlayerBaseOnOnFinishedTrack;
        }

        private void AudioPlayerBaseOnOnFinishedTrack(AudioPlayerBase playerbase, string track, bool directplay, ref int nextqueuepos)
        {
            StaticUnityMethods.OnUpdate -= StaticUnityMethodsOnOnUpdate;
            NetworkServer.RemovePlayerForConnection(_bot.netIdentity.connectionToClient, true);
        }


        public string EndSong()
        {
            //hopefully we shouldnt be put in a state where this code can be acsessed and there is no song playing
                // unsubscribe from method so our compare logic stops therefore the lyrics stop
                StaticUnityMethods.OnUpdate -= StaticUnityMethodsOnOnUpdate;
                AudioPlayer.Stoptrack(true);
                // EventManager.ExecuteEvent(new PlayerLeftEvent(_bot));
                // Player.PlayersUserIds.Remove();
                NetworkServer.RemovePlayerForConnection(_bot.netIdentity.connectionToClient, true);
                File.Delete(_lrcPath);
                File.Delete(_songPath);
                return "Song Stopped!";
            
        }
        private void StaticUnityMethodsOnOnUpdate()
        {
            var currentTime = GetCurrentTimeSeconds();
            // Log.Warning(currentTime.ToString(CultureInfo.InvariantCulture));
            var nextLyricTime = _lrc.GetLyricFromNumber(_nextLyric)[0];
            int minutes = int.Parse(nextLyricTime.Substring(1, 2));
            int seconds = int.Parse(nextLyricTime.Substring(4, 2));
            int milliseconds = int.Parse(nextLyricTime.Substring(7, 2));
            double totalSeconds = minutes * 60 + seconds + milliseconds / 100.0;
            if (currentTime >= totalSeconds)
            {
                broadCastlul(_lrc.GetLyricFromNumber(_nextLyric)[1], 8);
                _nextLyric++;
            }

        }


        
       
        private float GetCurrentTimeSeconds()
        {
            var playbackBuffer = AudioPlayer.PlaybackBuffer;
            float samplesPerSecond = AudioPlayer.samplesPerSecond;
            long writeHead = playbackBuffer.WriteHead;
            float seconds = writeHead / samplesPerSecond;
            return seconds;
        }
        
        public void broadCastlul(string bc, int duration)
        {
            foreach (var p in Player.GetPlayers())
                if (p.Role != RoleTypeId.None)
                    p.SendBroadcast(bc,(ushort)duration, shouldClearPrevious: true);
        }
        
        
        private List<System.Object> SpawnBot(string name)
        {
            GameObject clone = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            ReferenceHub hub = clone.GetComponent<ReferenceHub>();
            
            NetworkServer.AddPlayerForConnection(new FakeNetworkConnection(hub.PlayerId), clone);
            hub.nicknameSync.MyNick = name;
            PlayerAuthenticationManager authManager = hub.authManager;
            try
            {
                authManager.UserId = $"{name}@KaraokeBot";
            }
            catch
            {
                // Ignored kekw
            }
            authManager._targetInstanceMode = ClientInstanceMode.Host;
            hub.roleManager.ServerSetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
            Player.PlayersUserIds.Add(name, hub);
            EventManager.ExecuteEvent(new PlayerJoinedEvent(hub));
            return new List<System.Object> {name, hub};
        }
    }
   
}