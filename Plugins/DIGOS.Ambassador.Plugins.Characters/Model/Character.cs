﻿//
//  Character.cs
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using DIGOS.Ambassador.Core.Services;
using DIGOS.Ambassador.Plugins.Core.Model.Entity;
using DIGOS.Ambassador.Plugins.Core.Model.Servers;
using DIGOS.Ambassador.Plugins.Core.Model.Users;
using Discord;
using JetBrains.Annotations;
using Image = DIGOS.Ambassador.Plugins.Characters.Model.Data.Image;

namespace DIGOS.Ambassador.Plugins.Characters.Model
{
    /// <summary>
    /// Represents a user's character.
    /// </summary>
    [PublicAPI]
    [Table("Characters", Schema = "CharacterModule")]
    public class Character : IOwnedNamedEntity, IServerEntity
    {
        /// <inheritdoc />
        public long ID { get; set; }

        /// <inheritdoc />
        public long ServerID { get; set; }

        /// <inheritdoc />
        [Required, NotNull]
        public virtual User Owner { get; set; }

        /// <inheritdoc />
        [Required, NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the character is the user's default character.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the character is currently in use on the server.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Gets or sets a URL pointing to the character's avatar.
        /// </summary>
        [Required, NotNull]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the nickname that a user should have when playing as the character.
        /// </summary>
        [CanBeNull]
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the character summary.
        /// </summary>
        [Required, NotNull]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the full description of the character.
        /// </summary>
        [Required, NotNull]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the character is NSFW.
        /// </summary>
        public bool IsNSFW { get; set; }

        /// <summary>
        /// Gets or sets the images associated with the character.
        /// </summary>
        [Required, NotNull, ItemNotNull]
        public virtual List<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Gets or sets the preferred pronoun family of the character.
        /// </summary>
        [Required, NotNull]
        public string PronounProviderFamily { get; set; }

        /// <summary>
        /// Gets or sets a custom role that gets applied along with the character, similar to a nickname.
        /// </summary>
        [CanBeNull]
        public virtual CharacterRole Role { get; set; }

        /// <inheritdoc />
        [NotMapped, NotNull]
        public string EntityTypeDisplayName => nameof(Character);

        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class.
        /// </summary>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized", Justification = "Initialized by EF Core.")]
        protected Character()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class.
        /// </summary>
        /// <param name="owner">The owner of the character.</param>
        /// <param name="name">The character's name.</param>
        /// <param name="avatarUrl">The avatar URL for the character.</param>
        /// <param name="nickname">The character's nickname. Defaults to the character's name.</param>
        /// <param name="summary">The character's summary.</param>
        /// <param name="description">The character's description.</param>
        /// <param name="pronounProviderFamily">The character's pronoun provider family.</param>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor", Justification = "Required by EF Core.")]
        public Character
        (
            [NotNull] User owner,
            [NotNull] string name,
            [NotNull] string avatarUrl,
            [CanBeNull] string nickname = null,
            [NotNull] string summary = "No summary set.",
            [NotNull] string description = "No description set.",
            [NotNull] string pronounProviderFamily = "They"
        )
        {
            this.Owner = owner;
            this.Name = name;
            this.AvatarUrl = avatarUrl;
            this.Summary = summary;
            this.Description = description;
            this.PronounProviderFamily = pronounProviderFamily;
            this.Nickname = nickname ?? name;
        }

        /// <inheritdoc />
        public bool IsOwner(User user)
        {
            return IsOwner(user.DiscordID);
        }

        /// <inheritdoc />
        public bool IsOwner(IUser user)
        {
            return IsOwner((long)user.Id);
        }

        /// <inheritdoc />
        public bool IsOwner(long userID)
        {
            return this.Owner.DiscordID == userID;
        }
    }
}
