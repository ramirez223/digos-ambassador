//
//  AnonymousMail.cs
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
using System.ComponentModel.DataAnnotations.Schema;
using DIGOS.Ambassador.Core.Database.Entities;
using JetBrains.Annotations;

namespace DIGOS.Ambassador.Plugins.Anonymail.Model
{
    /// <summary>
    /// Represents an anonymous mail.
    /// </summary>
    [PublicAPI]
    [Table("AnonymousMails", Schema = "AnonymailModule")]
    public class AnonymousMail : EFEntity
    {
        /// <summary>
        /// Gets the author of the mail.
        /// </summary>
        public virtual AnonymizedUser Author { get; private set; } = null!;

        /// <summary>
        /// Gets the time when the message was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// Gets the contents of the mail.
        /// </summary>
        public string Contents { get; private set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMail"/> class.
        /// </summary>
        /// <remarks>Required by EF core.</remarks>
        protected AnonymousMail()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMail"/> class.
        /// </summary>
        /// <param name="author">The author of the message.</param>
        /// <param name="contents">The contents of the message.</param>
        public AnonymousMail(AnonymizedUser author, string contents)
        {
            this.Author = author;
            this.Contents = contents;
            this.Timestamp = DateTimeOffset.UtcNow;
        }
    }
}
