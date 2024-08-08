// -----------------------------------------------------------------------
// <copyright file="ParentCommandExample.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.Events.Commands.PluginManager;

namespace SCPKaraoke.Commands
{
#pragma warning disable SA1402
    // Usings
    using System;

    using CommandSystem;

    using Exiled.Permissions.Extensions; // Use this if you want to add perms

    /// <inheritdoc/>
    [CommandHandler(typeof(RemoteAdminCommandHandler))] // You can change the command handler
    public class SCPKaraokeCommand : ParentCommand
    {

        public SCPKaraokeCommand()
        {
            LoadGeneratedCommands();
            
        }

        /// <inheritdoc />
        public override string Command { get; } = "karaoke";   

        /// <inheritdoc />
        public override string[] Aliases { get; } = { "kar" };   // ALIASES, is dont necessary to add aliases, if you want to add a aliase just put = null;

        /// <inheritdoc />
        public override string Description { get; } = "YOUR DESC";  

        /// <inheritdoc />
        public sealed override void LoadGeneratedCommands() 
        {
            RegisterCommand(new Start());
            RegisterCommand(new Stop());
        }

        /// <inheritdoc />
        // Here starts your command code
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // If you want to add permissions you need to use that
            if (!sender.CheckPermission("exiled.parenttest"))
            {
                response = "You dont have perms";
                return false;
            }

            // Put here your code
            // Make sure to put return and response here
            response = "Done!";
            return true;
        }
    }

   
}