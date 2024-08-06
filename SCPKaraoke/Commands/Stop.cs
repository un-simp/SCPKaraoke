using CommandSystem;
using System;


namespace SCPKaraoke.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Stop : ICommand
    {
        public string Command { get; } = "stop";

        public string[] Aliases { get; } = new string[] { "e", };

        public string Description { get; } = "Test command.";



        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Attempt to get karaokeSync class
            try
            {
                KaraokeSync karaoke = KaraokeSync.Current;
                karaoke.EndSong();
            }
            catch (NullReferenceException e)
            {
                response = "There is no karaoke playing right now (according to me at least :shrug:)";
                return false;
            }
            response = "Complete";
            return true;
        }
    }
}