using CommandSystem;
using System;
using System.Collections.Generic;
using CentralAuth;
using UnityEngine;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Events;
using SCPKaraoke.LRCParser;
using SCPSLAudioApi.AudioCore;
using VoiceChat;

namespace SCPKaraoke.Commands
{
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Start : ICommand
{
    public string Command { get; } = "start";

    public string[] Aliases { get; } = new string[] { "s",};

    public string Description { get; } = "Test command.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        ReferenceHub audioBot = SpawnBot("Balls");
        AudioPlayerBase audioPlayer = AudioPlayerBase.Get(audioBot);
        response = "Success on Spawn, attempting to play audio source";
        audioPlayer.BroadcastChannel = VoiceChatChannel.RoundSummary;
        audioPlayer.AllowUrl = true;
        audioPlayer.CurrentPlay = "url";
        // next load lyrics
        LrcParser lrc = new LrcParser("/mnt/2E9808EC7E84BBD8/deemix/Mckenna Grace - Checkered Vans.lrc");
        //i just send all the lyrics at once and tell the game to never clear the broadcast lists
        // its scuffed. dosent consider for other plugins which do clear the list and never will allow for stopping 
        // but GOD its beautiful and works somehow
        // THIS WILL BE REFACTORED HOLY FUCK
        for (int i = 0; i < lrc.GetNumberOfLyrics(); i++)
        {
            broadCastlul(lrc.GetLyricFromNumber(i)[1],2);
        }
    
        audioPlayer.Play(-1);
        return true;
    }

    public void broadCastlul(string bc, int duration)
    {
        foreach (var p in Player.GetPlayers())
            if (p.Role != RoleTypeId.None)
                p.SendBroadcast(bc,(ushort)duration, shouldClearPrevious: false);
        return;
    }
    
    
    private ReferenceHub SpawnBot(string name)
    {
        GameObject clone = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
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
        hub.roleManager.ServerSetRole(RoleTypeId.Overwatch, RoleChangeReason.RemoteAdmin);
        Player.PlayersUserIds.Add(name, hub);
        EventManager.ExecuteEvent(new PlayerJoinedEvent(hub));
        return hub;
    }
}
}