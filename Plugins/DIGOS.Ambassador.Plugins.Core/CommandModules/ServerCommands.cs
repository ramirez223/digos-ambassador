﻿//
//  ServerCommands.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Threading.Tasks;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Extensions.Results;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Core.Extensions;
using DIGOS.Ambassador.Plugins.Core.Model;
using DIGOS.Ambassador.Plugins.Core.Permissions;
using DIGOS.Ambassador.Plugins.Core.Services.Servers;
using DIGOS.Ambassador.Plugins.Permissions.Preconditions;

using Discord.Commands;
using JetBrains.Annotations;
using static Discord.Commands.ContextType;
using PermissionTarget = DIGOS.Ambassador.Plugins.Permissions.Model.PermissionTarget;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Core.CommandModules
{
    /// <summary>
    /// Server-related commands, such as viewing or editing info about a specific server.
    /// </summary>
    [UsedImplicitly]
    [Group("server")]
    [Alias("server", "guild")]
    [Summary("Server-related commands, such as viewing or editing info about a specific server.")]
    public class ServerCommands : ModuleBase
    {
        private readonly UserFeedbackService _feedback;
        private readonly ServerService _servers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCommands"/> class.
        /// </summary>
        /// <param name="database">A database context from the context pool.</param>
        /// <param name="feedback">The user feedback service.</param>
        /// <param name="servers">The servers service.</param>
        public ServerCommands(CoreDatabaseContext database, UserFeedbackService feedback, ServerService servers)
        {
            _feedback = feedback;
            _servers = servers;
        }

        /// <summary>
        /// Shows general information about the current server.
        /// </summary>
        [UsedImplicitly]
        [Command("show")]
        [Alias("show", "info")]
        [Summary("Shows general information about the current server.")]
        [RequireContext(Guild)]
        [RequirePermission(typeof(ShowServerInfo), PermissionTarget.Self)]
        public async Task<RuntimeResult> ShowServerAsync()
        {
            var eb = _feedback.CreateEmbedBase();

            var guild = this.Context.Guild;

            var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
            if (!getServerResult.IsSuccess)
            {
                return getServerResult.ToRuntimeResult();
            }

            var server = getServerResult.Entity;

            eb.WithTitle(guild.Name);

            if (!(guild.SplashUrl is null))
            {
                eb.WithThumbnailUrl(guild.SplashUrl);
            }
            else if (!(guild.IconUrl is null))
            {
                eb.WithThumbnailUrl(guild.IconUrl);
            }

            var getDescriptionResult = _servers.GetDescription(server);
            if (getDescriptionResult.IsSuccess)
            {
                eb.WithDescription(getDescriptionResult.Entity);
            }
            else
            {
                eb.WithDescription("The server doesn't have a description set.");
            }

            eb.AddField("Permission Warnings", server.SuppressPermissionWarnings ? "On" : "Off", true);
            eb.AddField("NSFW", server.IsNSFW ? "Yes" : "No");
            eb.AddField("Send first-join message", server.SendJoinMessage ? "Yes" : "No");
            eb.AddField
            (
                "First-join message",
                server.JoinMessage is null ? "Not set" : server.JoinMessage.Ellipsize(1024)
            );

            await _feedback.SendEmbedAsync(this.Context.Channel, eb.Build());
            return RuntimeCommandResult.FromSuccess();
        }

        /// <summary>
        /// Shows the server's join message.
        /// </summary>
        [UsedImplicitly]
        [Command("join-message")]
        [Summary("Shows the server's join message.")]
        [RequireContext(Guild)]
        [RequirePermission(typeof(ShowServerInfo), PermissionTarget.Self)]
        public async Task<RuntimeResult> ShowJoinMessageAsync()
        {
            var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
            if (!getServerResult.IsSuccess)
            {
                return getServerResult.ToRuntimeResult();
            }

            var server = getServerResult.Entity;

            var getJoinMessageResult = _servers.GetJoinMessage(server);
            if (!getJoinMessageResult.IsSuccess)
            {
                return getJoinMessageResult.ToRuntimeResult();
            }

            var eb = _feedback.CreateEmbedBase();

            eb.WithTitle("Welcome!");
            eb.WithDescription(getJoinMessageResult.Entity);

            await _feedback.SendEmbedAsync(this.Context.Channel, eb.Build());
            return RuntimeCommandResult.FromSuccess();
        }

        /// <summary>
        /// Clears the join message.
        /// </summary>
        [UsedImplicitly]
        [Command("clear-join-message")]
        [Summary("Clears the join message.")]
        [RequireContext(Guild)]
        [RequirePermission(typeof(EditServerInfo), PermissionTarget.Self)]
        public async Task<RuntimeResult> ClearJoinMessageAsync()
        {
            var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
            if (!getServerResult.IsSuccess)
            {
                return getServerResult.ToRuntimeResult();
            }

            var server = getServerResult.Entity;

            var result = await _servers.ClearJoinMessageAsync(server);
            if (!result.IsSuccess)
            {
                return result.ToRuntimeResult();
            }

            return RuntimeCommandResult.FromSuccess("Join message cleared.");
        }

        /// <summary>
        /// Server info setter commands.
        /// </summary>
        [UsedImplicitly]
        [Group("set")]
        public class SetCommands : ModuleBase
        {
            private readonly ServerService _servers;

            /// <summary>
            /// Initializes a new instance of the <see cref="SetCommands"/> class.
            /// </summary>
            /// <param name="database">A database context from the context pool.</param>
            /// <param name="servers">The servers service.</param>
            public SetCommands(CoreDatabaseContext database, ServerService servers)
            {
                _servers = servers;
            }

            /// <summary>
            /// Sets the server's description.
            /// </summary>
            /// <param name="newDescription">The new description.</param>
            [UsedImplicitly]
            [Command("description")]
            [Summary("Sets the server's description.")]
            [RequireContext(Guild)]
            [RequirePermission(typeof(EditServerInfo), PermissionTarget.Self)]
            public async Task<RuntimeResult> SetDescriptionAsync(string newDescription)
            {
                var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
                if (!getServerResult.IsSuccess)
                {
                    return getServerResult.ToRuntimeResult();
                }

                var server = getServerResult.Entity;

                var result = await _servers.SetDescriptionAsync(server, newDescription);
                if (!result.IsSuccess)
                {
                    return result.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("Server description set.");
            }

            /// <summary>
            /// Sets the server's first-join message.
            /// </summary>
            /// <param name="newJoinMessage">The new join message.</param>
            [UsedImplicitly]
            [Command("join-message")]
            [Summary("Sets the server's first-join message.")]
            [RequireContext(Guild)]
            [RequirePermission(typeof(EditServerInfo), PermissionTarget.Self)]
            public async Task<RuntimeResult> SetJoinMessageAsync(string newJoinMessage)
            {
                var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
                if (!getServerResult.IsSuccess)
                {
                    return getServerResult.ToRuntimeResult();
                }

                var server = getServerResult.Entity;

                var result = await _servers.SetJoinMessageAsync(server, newJoinMessage);
                if (!result.IsSuccess)
                {
                    return result.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("Server first-join message set.");
            }

            /// <summary>
            /// Sets whether the server is NSFW.
            /// </summary>
            /// <param name="isNsfw">Whether the server is NSFW.</param>
            [UsedImplicitly]
            [Command("is-nsfw")]
            [Summary("Sets whether the server is NSFW.")]
            [RequireContext(Guild)]
            [RequirePermission(typeof(EditServerInfo), PermissionTarget.Self)]
            public async Task<RuntimeResult> SetIsNSFWAsync(bool isNsfw)
            {
                var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
                if (!getServerResult.IsSuccess)
                {
                    return getServerResult.ToRuntimeResult();
                }

                var server = getServerResult.Entity;

                var result = await _servers.SetIsNSFWAsync(server, isNsfw);
                if (!result.IsSuccess)
                {
                    return result.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess
                (
                    $"The server is {(isNsfw ? "now set as NSFW" : "no longer NSFW")}."
                );
            }

            /// <summary>
            /// Sets whether the bot sends join messages to new users.
            /// </summary>
            /// <param name="sendJoinMessage">Whether the bot sends join messages to new users.</param>
            [UsedImplicitly]
            [Command("send-join-messages")]
            [Summary("Sets whether the bot sends join messages to new users.")]
            [RequireContext(Guild)]
            [RequirePermission(typeof(EditServerInfo), PermissionTarget.Self)]
            public async Task<RuntimeResult> SetSendJoinMessagesAsync(bool sendJoinMessage)
            {
                var getServerResult = await _servers.GetOrRegisterServerAsync(this.Context.Guild);
                if (!getServerResult.IsSuccess)
                {
                    return getServerResult.ToRuntimeResult();
                }

                var server = getServerResult.Entity;

                var result = await _servers.SetSendJoinMessageAsync(server, sendJoinMessage);
                if (!result.IsSuccess)
                {
                    return result.ToRuntimeResult();
                }

                var willDo = sendJoinMessage
                    ? "will now send first-join messages to new users"
                    : "no longer sends first-join messages";

                return RuntimeCommandResult.FromSuccess($"The server {willDo}.");
            }
        }
    }
}
