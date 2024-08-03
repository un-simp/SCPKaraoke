using PluginAPI.Core;
using CommandSystem;
using System;

namespace SCPKaraoke.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class MainCommand : ParentCommand
    {
        public override string Command => "karaoke";
        public override string Description => "Main command for SCP Karaoke";
        public override string[] Aliases => new string[] { };

        public override void LoadGeneratedCommands()
        {
            try
            {
                RegisterCommand(new Start());
            }
            catch (Exception e)
            {
                Log.Warning($"Caught an exception while registering commands. ");
                Log.Debug($"{e}");
            }
        }

        public MainCommand() => this.LoadGeneratedCommands();

        protected override bool ExecuteParent(ArraySegment<string> argumements, ICommandSender sender,
            out string response)
        {
            response = "Enter a valid subcommand: \n";
            foreach (var individualsArgs in this.Commands)
            {
                string args = "";
                if (individualsArgs.Value is IUsageProvider usage)
                {
                    foreach (var arg in usage.Usage)
                    {
                        args += $"[{arg}]";
                    }
                }
                if (sender is not ServerConsoleSender )
                    response += $"<color=yellow> {individualsArgs.Key} {args}<color=white>-> {individualsArgs.Value.Description}. \n";
                else
                    response += $" {individualsArgs.Key} {args} -> {individualsArgs.Value.Description}. \n";
            }

            return false;
        }
    }
}