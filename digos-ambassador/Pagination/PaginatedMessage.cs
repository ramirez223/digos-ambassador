﻿//
//  PaginatedMessage.cs
//
//  Author:
//        Jarl Gullberg <jarl.gullberg@gmail.com>
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DIGOS.Ambassador.Services;
using DIGOS.Ambassador.Services.Interactivity.Messages;

using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

// ReSharper disable AssignmentIsFullyDiscarded
namespace DIGOS.Ambassador.Pagination
{
    /// <summary>
    /// A page building class for paginated galleries.
    /// </summary>
    /// <typeparam name="TContent">The type of content in the pager.</typeparam>
    /// <typeparam name="TType">The type of the pager.</typeparam>
    public abstract class PaginatedMessage<TContent, TType> : InteractiveMessage, IPager<TContent, TType>
        where TType : PaginatedMessage<TContent, TType>
    {
        /// <summary>
        /// Gets the user interaction service.
        /// </summary>
        private UserFeedbackService Feedback { get; }

        /// <inheritdoc />
        public virtual IList<TContent> Pages { get; protected set; }

        /// <inheritdoc />
        public PaginatedAppearanceOptions Appearance { get; set; } = PaginatedAppearanceOptions.Default;

        private int _currentPage = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedMessage{T1,T2}"/> class.
        /// </summary>
        /// <param name="feedbackService">The user feedback service.</param>
        /// <param name="sourceUser">The user who caused the interactive message to be created.</param>
        protected PaginatedMessage
        (
            UserFeedbackService feedbackService,
            IUser sourceUser
        )
            : base(sourceUser)
        {
            this.Feedback = feedbackService;
            this.Pages = new List<TContent>();

            this.Appearance.Color = Color.DarkPurple;
        }

        /// <inheritdoc/>
        public virtual TType AppendPage(TContent page)
        {
            this.Pages.Add(page);
            return (TType)this;
        }

        /// <inheritdoc/>
        public virtual TType WithPages(IEnumerable<TContent> pages)
        {
            this.Pages = pages.ToList();

            return (TType)this;
        }

        /// <inheritdoc/>
        public abstract Embed BuildEmbed(int page);

        /// <inheritdoc/>
        protected override async Task<IUserMessage> DisplayAsync([NotNull] IMessageChannel channel)
        {
            if (!this.Pages.Any())
            {
                throw new InvalidOperationException("The pager is empty.");
            }

            var embed = BuildEmbed(this._currentPage - 1);

            var message = await channel.SendMessageAsync(string.Empty, embed: embed).ConfigureAwait(false);

            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                if (this.Pages.Count > 1)
                {
                    await message.AddReactionAsync(this.Appearance.First);
                    await message.AddReactionAsync(this.Appearance.Back);
                    await message.AddReactionAsync(this.Appearance.Next);
                    await message.AddReactionAsync(this.Appearance.Last);

                    var manageMessages = await CanManageMessages();

                    var canJump =
                        this.Appearance.JumpDisplayCondition == JumpDisplayCondition.Always ||
                        (this.Appearance.JumpDisplayCondition == JumpDisplayCondition.WithManageMessages && manageMessages);

                    if (canJump)
                    {
                        await message.AddReactionAsync(this.Appearance.Jump);
                    }

                    if (this.Appearance.DisplayInformationIcon)
                    {
                        await message.AddReactionAsync(this.Appearance.Help);
                    }
                }

                await message.AddReactionAsync(this.Appearance.Stop);
            });

            return message;
        }

        /// <remarks>
        /// This override forwards to the added handler, letting removed reactions act the same as added reactions.
        /// </remarks>
        /// <inheritdoc/>
        public override Task HandleRemovedInteractionAsync(SocketReaction reaction) =>
            HandleAddedInteractionAsync(reaction);

        /// <inheritdoc/>
        public override async Task HandleAddedInteractionAsync(SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified)
            {
                // Ignore unspecified users
                return;
            }

            var interactingUser = reaction.User.Value;
            if (interactingUser.Id != this.SourceUser.Id)
            {
                if (interactingUser is IGuildUser guildUser)
                {
                    // If the user has permission to manage messages, they should be allowed to interact in all cases
                    if (!guildUser.GetPermissions(this.MessageContext.Channel as IGuildChannel).ManageMessages)
                    {
                        return;
                    }
                }
                else
                {
                    // We only allow interactions from the user who created the message
                    return;
                }
            }

            var emote = reaction.Emote;

            if (emote.Equals(this.Appearance.First))
            {
                this._currentPage = 1;
            }
            else if (emote.Equals(this.Appearance.Next))
            {
                if (this._currentPage >= this.Pages.Count)
                {
                    return;
                }

                ++this._currentPage;
            }
            else if (emote.Equals(this.Appearance.Back))
            {
                if (this._currentPage <= 1)
                {
                    return;
                }

                --this._currentPage;
            }
            else if (emote.Equals(this.Appearance.Last))
            {
                this._currentPage = this.Pages.Count;
            }
            else if (emote.Equals(this.Appearance.Stop))
            {
                await this.Interactivity.DeleteInteractiveMessageAsync(this);
                return;
            }
            else if (emote.Equals(this.Appearance.Jump))
            {
                _ = Task.Run
                (
                    async () =>
                    {
                        bool Filter(IUserMessage m) => m.Author.Id == reaction.UserId;

                        var responseResult = await this.Interactivity.GetNextMessageAsync(this.MessageContext.Channel, Filter, TimeSpan.FromSeconds(15));
                        if (!responseResult.IsSuccess)
                        {
                            return;
                        }

                        var response = responseResult.Entity;

                        if (!int.TryParse(response.Content, out int request) || request < 1 || request > this.Pages.Count)
                        {
                            _ = response.DeleteAsync().ConfigureAwait(false);

                            var eb = this.Feedback.CreateFeedbackEmbed(response.Author, Color.DarkPurple, "Please specify a page to jump to.");

                            await this.Feedback.SendEmbedAndDeleteAsync(this.MessageContext.Channel, eb);
                            return;
                        }

                        this._currentPage = request;
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await UpdateAsync().ConfigureAwait(false);
                    }
                );
            }
            else if (emote.Equals(this.Appearance.Help))
            {
                var user = this.Interactivity.Client.GetUser(reaction.UserId);
                var eb = this.Feedback.CreateFeedbackEmbed(user, Color.DarkPurple, this.Appearance.HelpText);

                await this.Feedback.SendEmbedAndDeleteAsync(this.MessageContext.Channel, eb, this.Appearance.InfoTimeout);
                return;
            }

            if (await CanManageMessages())
            {
                _ = this.Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }

            await UpdateAsync().ConfigureAwait(false);
        }

        private async Task<bool> CanManageMessages()
        {
            if (this.MessageContext.Channel is IGuildChannel guildChannel)
            {
                var botUser = this.Interactivity.Client.CurrentUser;
                var botGuildUser = await guildChannel.Guild.GetUserAsync(botUser.Id);

                return botGuildUser.GetPermissions(guildChannel).ManageMessages;
            }

            return false;
        }

        /// <inheritdoc/>
        protected override async Task UpdateAsync()
        {
            var embed = BuildEmbed(this._currentPage - 1);

            await this.Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
