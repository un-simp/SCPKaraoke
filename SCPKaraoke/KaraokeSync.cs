using System.IO;
using CentralAuth;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
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
        private ReferenceHub _bot;
        private int _nextLyric;
        
        public KaraokeSync(string LRCPath, string songPath)
        {
            _lrc = new LrcParser(LRCPath);
            _bot = SpawnBot("balls");
            _audiosource  = AudioPlayerBase.Get(_bot);
            _audiosource.CurrentPlay = songPath;

        }

        private AudioPlayerBase AudioPlayer => _audiosource;
        public void StartSongAndLyrics()
        {
            
            // subscribe to update thread so our other code isnt running all the fucking time and only after this has been run
            StaticUnityMethods.OnUpdate += StaticUnityMethodsOnOnUpdate;
            _nextLyric = 0;
            AudioPlayer.BroadcastChannel = VoiceChatChannel.RoundSummary;
            AudioPlayer.AllowUrl = false;
            AudioPlayer.LogDebug = false;
            AudioPlayer.Play(-1);
        }


        public string EndSong()
        {
            if (AudioPlayer.ShouldPlay)
            {
                // unsubscribe from method so our compare logic stops therefore the lyrics stop
                StaticUnityMethods.OnUpdate -= StaticUnityMethodsOnOnUpdate;
                AudioPlayer.Stoptrack(true);
                EventManager.ExecuteEvent(new PlayerLeftEvent(_bot));
                return "Song Stopped!";
            }

            return "silly billy no song is playing!";
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
        
        
        
        private ReferenceHub SpawnBot(string name)
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
            return hub;
        }
    }
   
}