using System;
using CommandSystem;
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
            response = "you opted out of karaoke";
            return true;
        }


    }
}