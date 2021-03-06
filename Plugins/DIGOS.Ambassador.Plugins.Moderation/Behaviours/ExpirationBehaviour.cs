//
//  ExpirationBehaviour.cs
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DIGOS.Ambassador.Plugins.Moderation.Model;
using DIGOS.Ambassador.Plugins.Moderation.Services;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.Behaviours;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Moderation.Behaviours
{
    /// <summary>
    /// Rescinds expired warnings and bans.
    /// </summary>
    [UsedImplicitly]
    internal sealed class ExpirationBehaviour : ContinuousDiscordBehaviour<ExpirationBehaviour>
    {
        /// <inheritdoc/>
        protected override bool UseTransaction => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpirationBehaviour"/> class.
        /// </summary>
        /// <param name="client">The Discord client.</param>
        /// <param name="services">The services.</param>
        /// <param name="logger">The logging instance for this type.</param>
        public ExpirationBehaviour
        (
            DiscordSocketClient client,
            IServiceProvider services,
            ILogger<ExpirationBehaviour> logger
        )
            : base(client, services, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> OnTickAsync(CancellationToken ct, IServiceProvider tickServices)
        {
            var warningService = tickServices.GetRequiredService<WarningService>();
            var banService = tickServices.GetRequiredService<BanService>();
            var loggingService = tickServices.GetRequiredService<ChannelLoggingService>();

            foreach (var guild in this.Client.Guilds)
            {
                if (ct.IsCancellationRequested)
                {
                    return OperationResult.FromError("Operation was cancelled.");
                }

                var botUser = guild.GetUser(this.Client.CurrentUser.Id);
                if (botUser is null)
                {
                    // The bot is probably not ready yet
                    break;
                }

                var warnings = await warningService.GetWarningsAsync(guild, ct);
                foreach (var warning in warnings.Where(w => w.IsTemporary))
                {
                    // We'll use a transaction per warning to avoid timeouts
                    using var warningTransaction = new TransactionScope
                    (
                        TransactionScopeOption.Required,
                        this.TransactionOptions,
                        TransactionScopeAsyncFlowOption.Enabled
                    );

                    var rescindWarningResult = await RescindWarningIfExpiredAsync
                    (
                        loggingService,
                        warningService,
                        guild,
                        warning,
                        ct
                    );

                    if (rescindWarningResult.IsSuccess)
                    {
                        warningTransaction.Complete();
                        continue;
                    }

                    this.Log.LogWarning(rescindWarningResult.ErrorReason);
                }

                if (!botUser.GuildPermissions.BanMembers)
                {
                    // No point in trying to rescind bans if the bot doesn't have ban perms
                    continue;
                }

                var bans = await banService.GetBansAsync(guild, ct);
                foreach (var ban in bans.Where(b => b.IsTemporary))
                {
                    // We'll use a transaction per warning to avoid timeouts
                    using var banTransaction = new TransactionScope
                    (
                        TransactionScopeOption.Required,
                        this.TransactionOptions,
                        TransactionScopeAsyncFlowOption.Enabled
                    );

                    var rescindBanResult = await RescindBanIfExpiredAsync
                    (
                        loggingService,
                        banService,
                        guild,
                        ban,
                        ct
                    );

                    if (rescindBanResult.IsSuccess)
                    {
                        banTransaction.Complete();
                        continue;
                    }

                    this.Log.LogWarning(rescindBanResult.ErrorReason);
                }
            }

            return OperationResult.FromSuccess();
        }

        private async Task<OperationResult> RescindBanIfExpiredAsync
        (
            ChannelLoggingService loggingService,
            BanService bans,
            SocketGuild guild,
            UserBan ban,
            CancellationToken ct
        )
        {
            if (ct.IsCancellationRequested)
            {
                return OperationResult.FromError("Operation was cancelled.");
            }

            if (!(ban.ExpiresOn <= DateTime.UtcNow))
            {
                // No rescinding is needed, so we'll just bail out
                return OperationResult.FromSuccess();
            }

            var rescinder = guild.GetUser(this.Client.CurrentUser.Id);
            var notifyResult = await loggingService.NotifyUserUnbannedAsync(ban, rescinder);
            if (!notifyResult.IsSuccess)
            {
                return OperationResult.FromError(notifyResult);
            }

            var deleteResult = await bans.DeleteBanAsync(ban, ct);
            if (!deleteResult.IsSuccess)
            {
                return OperationResult.FromError(deleteResult);
            }

            try
            {
                await guild.RemoveBanAsync((ulong)ban.User.DiscordID);
            }
            catch (HttpException hex) when (hex.HttpCode == HttpStatusCode.NotFound)
            {
                // Already unbanned
                return OperationResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return OperationResult.FromError(ex);
            }

            return OperationResult.FromSuccess();
        }

        private async Task<OperationResult> RescindWarningIfExpiredAsync
        (
            ChannelLoggingService loggingService,
            WarningService warnings,
            SocketGuild guild,
            UserWarning warning,
            CancellationToken ct
        )
        {
            if (ct.IsCancellationRequested)
            {
                return OperationResult.FromError("Operation was cancelled.");
            }

            if (!(warning.ExpiresOn <= DateTime.UtcNow))
            {
                // No rescinding is needed, so we'll just bail out
                return OperationResult.FromSuccess();
            }

            var rescinder = guild.GetUser(this.Client.CurrentUser.Id);
            var notifyResult = await loggingService.NotifyUserWarningRemovedAsync(warning, rescinder);
            if (!notifyResult.IsSuccess)
            {
                return OperationResult.FromError(notifyResult);
            }

            var deleteResult = await warnings.DeleteWarningAsync(warning, ct);
            if (!deleteResult.IsSuccess)
            {
                return OperationResult.FromError(deleteResult);
            }

            return OperationResult.FromSuccess();
        }
    }
}
