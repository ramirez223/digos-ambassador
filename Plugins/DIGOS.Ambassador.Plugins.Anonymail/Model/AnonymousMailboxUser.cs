//
//  AnonymousMailboxUser.cs
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

using System.ComponentModel.DataAnnotations.Schema;
using DIGOS.Ambassador.Core.Database.Entities;
using JetBrains.Annotations;

namespace DIGOS.Ambassador.Plugins.Anonymail.Model
{
    /// <summary>
    /// Represents a join entry between a user and a mailbox.
    /// </summary>
    [PublicAPI]
    [Table("AnonymousMailboxUsers", Schema = "AnonymailModule")]
    public class AnonymousMailboxUser : EFEntity
    {
        /// <summary>
        /// Gets the user.
        /// </summary>
        public virtual AnonymizedUser User { get; private set; } = null!;

        /// <summary>
        /// Gets the mailbox the user has some special setting in.
        /// </summary>
        public virtual AnonymousMailbox Mailbox { get; private set; } = null!;

        /// <summary>
        /// Gets a value indicating whether the user is blocked from sending mail to the mailbox.
        /// </summary>
        public bool IsBlocked { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMailboxUser"/> class.
        /// </summary>
        /// <remarks>Required by EF Core.</remarks>
        protected AnonymousMailboxUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMailboxUser"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="mailbox">The mailbox.</param>
        public AnonymousMailboxUser(AnonymizedUser user, AnonymousMailbox mailbox)
        {
            this.User = user;
            this.Mailbox = mailbox;
        }
    }
}
