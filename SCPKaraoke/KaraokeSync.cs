using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using MEC;
// wtf is this naming this is my fault its just so shit can interaction with the other shit yk how it goes
using SCPKaraoke.LRCParser;
using SCPSLAudioApi.AudioCore;
using VoiceChat;
using Server = Exiled.Events.Handlers.Server;

namespace SCPKaraoke
{
    public class KaraokeSync
    {
        private readonly LrcParser _lrc;
        private readonly AudioPlayerBase _audiosource;
        private readonly Npc _bot;
        private int _nextLyric;
        private readonly string _lrcPath;
        private readonly string _songPath;
        private readonly string _songName;
        public readonly List<int> Participators = new List<int>();
        private readonly bool _ifSameChannel;
        private readonly string _songArtist;

        // hopefully only one instance is made crossing fingers if not god is dead
        public static KaraokeSync Current { get; private set; }




        public KaraokeSync(string lrcPath, string songPath, string songName, string songArtist, bool ifSameChannel)
        {
            _lrc = new LrcParser(lrcPath);
            _lrcPath = lrcPath;
            _songPath = songPath;
            _songArtist = songArtist;
            _songName = songName;
            _bot = Npc.Spawn($"{_songName} - {songArtist}", RoleTypeId.Overwatch);
            _audiosource = AudioPlayerBase.Get(_bot.ReferenceHub);
            _audiosource.CurrentPlay = songPath;
            Current = this;
            foreach (var p in Player.List)
            {
                Participators.Add(p.Id);
            }

            _ifSameChannel = ifSameChannel;
        }

        private void EndPrematurely(EndingRoundEventArgs ev)
        {
            Map.Broadcast(10, "The song has ended prematurely due to a round end event");
            EndSong();

        }

        private AudioPlayerBase AudioPlayer => _audiosource;

        public void AnnounceSongThenPlay(int timeToStart, Player playerExecuted)
        {
            Log.Info(_songPath);
            Log.Info(_lrcPath);
            Cassie.Clear();
            Cassie.Message("10 seconds");
            Map.Broadcast((ushort)timeToStart,
                _ifSameChannel
                    ? $"<b><size=20>Karaoke for {_songName} by {_songArtist} will start in {timeToStart.ToString()} seconds! \n Open your Console and type \".optout\" to opt out.\n You'll all be in the same channel, blame {playerExecuted.Nickname}"
                    : $"<b><size=32>Karaoke for {_songName} by {_songArtist} will start in {timeToStart.ToString()} seconds! \n Open your Console and type \".optout\" to opt out."
            );
            // use mec here 
            Timing.CallDelayed(timeToStart, StartSongAndLyrics);



        }

        public void StartSongAndLyrics()
        {
            Server.EndingRound += EndPrematurely;
            _nextLyric = 0;
            AudioPlayer.BroadcastTo = Participators;
            if (_ifSameChannel)
            {
                foreach (var playerId in Participators)
                {
                    Player player = Player.Get(playerId);
                    player.VoiceModule.CurrentChannel = VoiceChatChannel.RoundSummary;
                }
            }

            AudioPlayer.BroadcastChannel = VoiceChatChannel.RoundSummary;
            AudioPlayer.AllowUrl = false;
            AudioPlayer.LogDebug = true;
            AudioPlayer.Play(-1);
            // so that the bot will end itself after the song is done
            AudioPlayerBase.OnFinishedTrack += AudioPlayerBaseOnOnFinishedTrack;
            // subscribe to update thread so our other code isnt running all the fucking time and only after this has been run
            Timing.RunCoroutine(LyricsSender(), "lyrics");
        }

        private void AudioPlayerBaseOnOnFinishedTrack(AudioPlayerBase playerbase, string track, bool directplay,
            ref int nextqueuepos)
        {
            Timing.KillCoroutines("lyrics");
            _bot.Destroy();
        }


        public string EndSong()
        {
            //hopefully we shouldnt be put in a state where this code can be acsessed and there is no song playing
            // unsubscribe from method so our compare logic stops therefore the lyrics stop
            Timing.KillCoroutines("lyrics");
            AudioPlayer.Stoptrack(true);
            _bot.Destroy();
            File.Delete(_lrcPath);
            File.Delete(_songPath);
            if (_ifSameChannel)
            {
                foreach (var playerId in Participators)
                {
                    Player player = Player.Get(playerId);
                    player.VoiceModule.CurrentChannel = player.VoiceModule._lastChannel;
                }
            }

            Server.EndingRound -= EndPrematurely;
            return "Song Stopped!";
        }

        private IEnumerator<float> LyricsSender()
        {
            for (;;) //repeat the following infinitely
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
                    foreach (var playerId in Participators)
                    {
                        Player player = Player.Get(playerId);
                        player.Broadcast(8, _lrc.GetLyricFromNumber(_nextLyric)[1], Broadcast.BroadcastFlags.Normal,
                            true);
                    }

                    _nextLyric++;
                }

                yield return Timing.WaitForOneFrame;

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



    }
}