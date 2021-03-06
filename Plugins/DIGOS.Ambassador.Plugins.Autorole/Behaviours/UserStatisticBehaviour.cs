//
//  UserStatisticBehaviour.cs
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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Plugins.Autorole.Concurrency;
using DIGOS.Ambassador.Plugins.Autorole.Model.Conditions;
using DIGOS.Ambassador.Plugins.Autorole.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.Behaviours;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Autorole.Behaviours
{
    /// <summary>
    /// Acts on various user-related events, logging statistics.
    /// </summary>
    [UsedImplicitly]
    internal sealed class UserStatisticBehaviour : ClientEventBehaviour<UserStatisticBehaviour>
    {
        /// <summary>
        /// Holds a set of semaphores for user IDs.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, TimedSemaphoreSlim> _timestampSemaphores;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStatisticBehaviour"/> class.
        /// </summary>
        /// <param name="client">The discord client.</param>
        /// <param name="services">The services.</param>
        /// <param name="logger">The logging instance.</param>
        public UserStatisticBehaviour
        (
            DiscordSocketClient client,
            IServiceProvider services,
            ILogger<UserStatisticBehaviour> logger
        )
            : base(client, services, logger)
        {
            _timestampSemaphores = new ConcurrentDictionary<ulong, TimedSemaphoreSlim>();
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> MessageUpdatedAsync
        (
            Cacheable<IMessage, ulong> oldMessage,
            SocketMessage newMessage,
            ISocketMessageChannel channel
        )
        {
            if (newMessage.Author.IsBot || newMessage.Author.IsWebhook)
            {
                return OperationResult.FromSuccess();
            }

            if (!(newMessage.Author is IGuildUser guildUser))
            {
                return OperationResult.FromSuccess();
            }

            using var eventScope = this.Services.CreateScope();
            var userStatistics = eventScope.ServiceProvider.GetRequiredService<UserStatisticsService>();
            return await UpdateLastActivityTimestampForUserAsync(userStatistics, guildUser);
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> UserJoinedAsync(SocketGuildUser user)
        {
            if (user.IsBot || user.IsWebhook)
            {
                return OperationResult.FromSuccess();
            }

            using var eventScope = this.Services.CreateScope();
            var userStatistics = eventScope.ServiceProvider.GetRequiredService<UserStatisticsService>();
            return await UpdateLastActivityTimestampForUserAsync(userStatistics, user);
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> ReactionAddedAsync
        (
            Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction
        )
        {
            var reactingUser = await channel.GetUserAsync(reaction.UserId);
            if (reactingUser.IsBot || reactingUser.IsWebhook)
            {
                return OperationResult.FromSuccess();
            }

            if (!(reactingUser is IGuildUser guildUser))
            {
                return OperationResult.FromSuccess();
            }

            using var eventScope = this.Services.CreateScope();
            var userStatistics = eventScope.ServiceProvider.GetRequiredService<UserStatisticsService>();
            return await UpdateLastActivityTimestampForUserAsync(userStatistics, guildUser);
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> UserVoiceStateUpdatedAsync
        (
            SocketUser user,
            SocketVoiceState oldState,
            SocketVoiceState newState
        )
        {
            if (user.IsBot || user.IsWebhook)
            {
                return OperationResult.FromSuccess();
            }

            if (!(user is IGuildUser guildUser))
            {
                return OperationResult.FromSuccess();
            }

            using var eventScope = this.Services.CreateScope();
            var userStatistics = eventScope.ServiceProvider.GetRequiredService<UserStatisticsService>();
            return await UpdateLastActivityTimestampForUserAsync(userStatistics, guildUser);
        }

        /// <inheritdoc/>
        protected override async Task<OperationResult> MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot || message.Author.IsWebhook)
            {
                return OperationResult.FromSuccess();
            }

            if (!(message.Author is SocketGuildUser guildUser))
            {
                return OperationResult.FromSuccess();
            }

            if (!(message.Channel is SocketTextChannel textChannel))
            {
                return OperationResult.FromSuccess();
            }

            // First, let's get some valid service instances
            using var eventScope = this.Services.CreateScope();
            var autoroles = eventScope.ServiceProvider.GetRequiredService<AutoroleService>();

            var autorolesOnServer = (await autoroles.GetAutorolesAsync(guildUser.Guild)).ToList();

            var wantsToUpdateChannelMessageCounts = autorolesOnServer.Any
            (
                a => a.Conditions.Any(c => c is MessageCountInChannelCondition)
            );

            var wantsToUpdateServerMessageCounts = autorolesOnServer.Any
            (
                a => a.Conditions.Any(c => c is MessageCountInGuildCondition)
            );

            var wantsToUpdateLastActivityTime = autorolesOnServer.Any
            (
                a => a.Conditions.Any(c => c is TimeSinceLastActivityCondition)
            );

            if
            (
                !wantsToUpdateChannelMessageCounts &&
                !wantsToUpdateServerMessageCounts &&
                !wantsToUpdateLastActivityTime
            )
            {
                return OperationResult.FromSuccess();
            }

            var userStatistics = eventScope.ServiceProvider.GetRequiredService<UserStatisticsService>();

            var updateResult = await UpdateLastActivityTimestampForUserAsync(userStatistics, guildUser);
            if (!updateResult.IsSuccess)
            {
                return OperationResult.FromError(updateResult);
            }

            if (wantsToUpdateChannelMessageCounts)
            {
                var updateChannelCountResult = await UpdateChannelMessageCountAsync(userStatistics, guildUser, textChannel);
                if (!updateChannelCountResult.IsSuccess)
                {
                    return OperationResult.FromError(updateChannelCountResult);
                }
            }

            if (!wantsToUpdateServerMessageCounts)
            {
                return OperationResult.FromSuccess();
            }

            var updateServerCountResult = await UpdateServerMessageCountAsync(userStatistics, guildUser);
            if (!updateServerCountResult.IsSuccess)
            {
                return OperationResult.FromError(updateServerCountResult);
            }

            return OperationResult.FromSuccess();
        }

        private async Task<OperationResult> UpdateServerMessageCountAsync
        (
            UserStatisticsService userStatistics,
            SocketGuildUser guildUser,
            CancellationToken ct = default
        )
        {
            var getGlobalStats = await userStatistics.GetOrCreateUserServerStatisticsAsync(guildUser, ct);
            if (!getGlobalStats.IsSuccess)
            {
                return OperationResult.FromError(getGlobalStats);
            }

            var globalStats = getGlobalStats.Entity;

            long? totalMessageCount;
            if (!(globalStats.TotalMessageCount is null))
            {
                totalMessageCount = globalStats.TotalMessageCount.Value + 1;
            }
            else
            {
                // Compute the first-time sum
                long sum = 0;
                foreach (var guildChannel in guildUser.Guild.TextChannels)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return OperationResult.FromError("Operation was cancelled.");
                    }

                    var countResult = await CountUserMessagesAsync(guildChannel, guildUser, ct);
                    if (countResult.IsSuccess)
                    {
                        sum += countResult.Entity;
                    }
                    else if (!(countResult.Exception is null))
                    {
                        return OperationResult.FromError(countResult);
                    }
                }

                totalMessageCount = sum;
            }

            var setResult = await userStatistics.SetTotalMessageCountAsync
            (
                globalStats,
                totalMessageCount,
                ct
            );

            if (!setResult.IsSuccess)
            {
                return OperationResult.FromError(setResult);
            }

            return OperationResult.FromSuccess();
        }

        private async Task<OperationResult> UpdateChannelMessageCountAsync
        (
            UserStatisticsService userStatistics,
            SocketGuildUser guildUser,
            SocketTextChannel textChannel,
            CancellationToken ct = default
        )
        {
            var getChannelStats = await userStatistics.GetOrCreateUserChannelStatisticsAsync
            (
                guildUser,
                textChannel,
                ct
            );

            if (!getChannelStats.IsSuccess)
            {
                return OperationResult.FromError(getChannelStats);
            }

            var channelStats = getChannelStats.Entity;

            long channelMessageCount = 0;
            if (!(channelStats.MessageCount is null))
            {
                channelMessageCount = channelStats.MessageCount.Value + 1;
            }
            else
            {
                var countResult = await CountUserMessagesAsync(textChannel, guildUser, ct);
                if (countResult.IsSuccess)
                {
                    channelMessageCount = countResult.Entity;
                }
                else if (!(countResult.Exception is null))
                {
                    return OperationResult.FromError(countResult);
                }
            }

            var setChannelCountResult = await userStatistics.SetChannelMessageCountAsync
            (
                channelStats,
                channelMessageCount,
                ct
            );

            if (!setChannelCountResult.IsSuccess)
            {
                return OperationResult.FromError(setChannelCountResult);
            }

            return OperationResult.FromSuccess();
        }

        /// <summary>
        /// Updates the last activity timestamp for the given user.
        /// </summary>
        /// <param name="userStatistics">The statistics service.</param>
        /// <param name="guildUser">The guild user.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<OperationResult> UpdateLastActivityTimestampForUserAsync
        (
            UserStatisticsService userStatistics,
            IGuildUser guildUser,
            CancellationToken ct = default
        )
        {
            var userSemaphore = _timestampSemaphores.GetOrAdd(guildUser.Id, u => new TimedSemaphoreSlim(1, 1));
            if (userSemaphore.Semaphore.CurrentCount == 0)
            {
                // Someone else is already updating the timestamp right now
                return OperationResult.FromSuccess();
            }

            try
            {
                await userSemaphore.Semaphore.WaitAsync(ct);
                userSemaphore.UpdateTimestamp();

                var getGlobalStats = await userStatistics.GetOrCreateUserServerStatisticsAsync(guildUser, ct);
                if (!getGlobalStats.IsSuccess)
                {
                    this.Log.LogError(getGlobalStats.Exception, getGlobalStats.ErrorReason);
                    return OperationResult.FromError(getGlobalStats);
                }

                var globalStats = getGlobalStats.Entity;

                var updateTimestamp = await userStatistics.UpdateTimestampAsync(globalStats, ct);
                if (!updateTimestamp.IsSuccess)
                {
                    return OperationResult.FromError(updateTimestamp);
                }

                return OperationResult.FromSuccess();
            }
            finally
            {
                userSemaphore.Semaphore.Release();

                await CleanSemaphoresAsync(ct);
            }
        }

        /// <summary>
        /// Removes old semaphore entries from the internal cache.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task CleanSemaphoresAsync(CancellationToken ct)
        {
            // Clean up old semaphores
            var keys = _timestampSemaphores.Keys;
            foreach (var key in keys)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                if (!_timestampSemaphores.TryGetValue(key, out var value))
                {
                    // Doesn't exist, or someone is using it
                    continue;
                }

                if (value.Semaphore.CurrentCount == 0)
                {
                    // Someone is using this
                    continue;
                }

                try
                {
                    await value.Semaphore.WaitAsync(ct);

                    if ((DateTimeOffset.UtcNow - value.Timestamp) > TimeSpan.FromMinutes(15))
                    {
                        _timestampSemaphores.TryRemove(key, out _);
                    }
                }
                finally
                {
                    value.Semaphore.Release();
                }
            }
        }

        private async Task<RetrieveEntityResult<long>> CountUserMessagesAsync
        (
            IMessageChannel channel,
            IUser user,
            CancellationToken ct = default
        )
        {
            long sum = 0;
            try
            {
                var latestMessage = (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();
                if (latestMessage is null)
                {
                    return 0;
                }

                // We'll explicitly include the latest message, since it'd get ignored otherwise
                if (latestMessage.Author.Id == user.Id)
                {
                    sum += 1;
                }

                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return RetrieveEntityResult<long>.FromError("Operation was cancelled.");
                    }

                    var channelMessages = channel.GetMessagesAsync(latestMessage, Direction.Before);

                    var processedBatches = 0;
                    await foreach (var channelMessageBatch in channelMessages.WithCancellation(ct))
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return RetrieveEntityResult<long>.FromError("Operation was cancelled.");
                        }

                        if (channelMessageBatch.Count == 0)
                        {
                            continue;
                        }

                        foreach (var channelMessage in channelMessageBatch)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                return RetrieveEntityResult<long>.FromError("Operation was cancelled.");
                            }

                            latestMessage = channelMessage;
                            if (latestMessage.Author.Id == user.Id)
                            {
                                sum += 1;
                            }
                        }

                        processedBatches += 1;
                    }

                    if (processedBatches == 0)
                    {
                        break;
                    }
                }
            }
            catch (HttpException hex) when (hex.WasCausedByMissingPermission())
            {
                return RetrieveEntityResult<long>.FromError("No permissions to read the channel.");
            }
            catch (Exception ex)
            {
                return RetrieveEntityResult<long>.FromError(ex);
            }

            return sum;
        }
    }
}
