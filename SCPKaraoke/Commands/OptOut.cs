using System;
using CommandSystem;
using Exiled.API.Features;

namespace SCPKaraoke.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class OptOut : ICommand
    {
        public string Command { get; } = "optout";

        public string[] Aliases { get; } = new string[] { "oo",};

        public string Description { get; } = "Opt out of karaoke (but why tho?)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                KaraokeSync.Current.Participators.RemoveAll(player => player.Equals(Player.Get(sender).Id));
                response = "you opted out of karaoke";
            }
            catch (NullReferenceException)
            {
                response = "no karaoke is happening rn";
            }

            return true;
        }


    }
}