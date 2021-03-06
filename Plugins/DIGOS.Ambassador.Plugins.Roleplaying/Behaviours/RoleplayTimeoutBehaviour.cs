//
//  RoleplayTimeoutBehaviour.cs
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Roleplaying.Model;
using DIGOS.Ambassador.Plugins.Roleplaying.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.Behaviours;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Roleplaying.Behaviours
{
    /// <summary>
    /// Times out roleplays, stopping them if they've been inactive for more than a set time.
    /// </summary>
    [UsedImplicitly]
    internal sealed class RoleplayTimeoutBehaviour : ContinuousDiscordBehaviour<RoleplayTimeoutBehaviour>
    {
        /// <summary>
        /// Gets the feedback service.
        /// </summary>
        private UserFeedbackService Feedback { get; }

        /// <inheritdoc/>
        protected override TimeSpan TickDelay => TimeSpan.FromMinutes(1);

        /// <inheritdoc/>
        protected override bool UseTransaction => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleplayTimeoutBehaviour"/> class.
        /// </summary>
        /// <param name="client">The discord client.</param>
        /// <param name="services">The services.</param>
        /// <param name="logger">The logging instance for this type.</param>
        /// <param name="feedback">The feedback service.</param>
        public RoleplayTimeoutBehaviour
        (
            DiscordSocketClient client,
            IServiceProvider services,
            ILogger<RoleplayTimeoutBehaviour> logger,
            UserFeedbackService feedback
        )
            : base(client, services, logger)
        {
            this.Feedback = feedback;
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> OnTickAsync(CancellationToken ct, IServiceProvider tickServices)
        {
            var roleplayService = tickServices.GetRequiredService<RoleplayDiscordService>();

            foreach (var guild in this.Client.Guilds)
            {
                if (ct.IsCancellationRequested)
                {
                    return OperationResult.FromError("Operation was cancelled.");
                }

                var getRoleplays = await roleplayService.GetRoleplaysAsync(guild);
                if (!getRoleplays.IsSuccess)
                {
                    continue;
                }

                var timedOutRoleplays = getRoleplays.Entity
                    .Where(r => r.IsActive)
                    .Where(r => r.LastUpdated.HasValue)
                    .Where(r => DateTime.Now - r.LastUpdated > TimeSpan.FromHours(72))
                    .ToList();

                foreach (var roleplay in timedOutRoleplays)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return OperationResult.FromError("Operation was cancelled.");
                    }

                    // We'll use a transaction per warning to avoid timeouts
                    using var timeoutTransaction = new TransactionScope
                    (
                        TransactionScopeOption.Required,
                        this.TransactionOptions,
                        TransactionScopeAsyncFlowOption.Enabled
                    );

                    var stopRoleplay = await roleplayService.StopRoleplayAsync(roleplay);
                    if (!stopRoleplay.IsSuccess)
                    {
                        return OperationResult.FromError(stopRoleplay);
                    }

                    var notifyResult = await NotifyOwnerAsync(roleplay);
                    if (!notifyResult.IsSuccess)
                    {
                        return OperationResult.FromError(notifyResult);
                    }

                    timeoutTransaction.Complete();
                }
            }

            return OperationResult.FromSuccess();
        }

        /// <summary>
        /// Notifies the owner of the roleplay that it was stopped because it timed out.
        /// </summary>
        /// <param name="roleplay">The roleplay.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<OperationResult> NotifyOwnerAsync(Roleplay roleplay)
        {
            var owner = this.Client.GetUser((ulong)roleplay.Owner.DiscordID);
            if (owner is null)
            {
                return OperationResult.FromError("Failed to get the owner.");
            }

            var notification = this.Feedback.CreateEmbedBase();
            notification.WithDescription
            (
                $"Due to inactivity, your roleplay \"{roleplay.Name}\" has been stopped."
            );

            notification.WithFooter
            (
                $"You can restart it by running !rp start \"{roleplay.Name}\"."
            );

            try
            {
                await owner.SendMessageAsync(string.Empty, embed: notification.Build());
            }
            catch (HttpException hex) when (hex.WasCausedByDMsNotAccepted())
            {
                // Nom nom nom
            }

            return OperationResult.FromSuccess();
        }
    }
}
