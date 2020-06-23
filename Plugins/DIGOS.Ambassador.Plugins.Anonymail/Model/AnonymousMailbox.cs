//
//  AnonymousMailbox.cs
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DIGOS.Ambassador.Core.Database.Entities;
using DIGOS.Ambassador.Plugins.Core.Model.Servers;
using JetBrains.Annotations;

namespace DIGOS.Ambassador.Plugins.Anonymail.Model
{
    /// <summary>
    /// Represents an anonymous mailbox.
    /// </summary>
    [PublicAPI]
    [Table("AnonymousMailboxes", Schema = "AnonymailModule")]
    public class AnonymousMailbox : EFEntity
    {
        /// <summary>
        /// Gets the server that the mailbox is on.
        /// </summary>
        public virtual Server Server { get; private set; } = null!;

        /// <summary>
        /// Gets the ID of the Discord channel that the mails should be delivered to.
        /// </summary>
        public long DiscordChannelID { get; internal set; }

        /// <summary>
        /// Gets the mails delivered to the mailbox.
        /// </summary>
        public virtual List<AnonymousMail> Mails { get; private set; } = null!;

        /// <summary>
        /// Gets a list of users who have some form of special treatment by this mailbox.
        /// </summary>
        public virtual List<AnonymousMailboxUser> MailboxUsers { get; private set; } = null!;

        /// <summary>
        /// Gets the name of the mailbox.
        /// </summary>
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMailbox"/> class.
        /// </summary>
        /// <remarks>Required by EF core.</remarks>
        protected AnonymousMailbox()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMailbox"/> class.
        /// </summary>
        /// <param name="server">The server the mailbox is on.</param>
        /// <param name="name">The name of the mailbox.</param>
        /// <param name="discordChannelID">The channel ID the mailbox delivers to.</param>
        public AnonymousMailbox(Server server, string name, long discordChannelID)
        {
            this.Server = server;
            this.Name = name;
            this.DiscordChannelID = discordChannelID;
            this.Mails = new List<AnonymousMail>();
            this.MailboxUsers = new List<AnonymousMailboxUser>();
        }
    }
}
