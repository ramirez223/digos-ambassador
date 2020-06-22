//
//  AnonymizedUser.cs
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
    /// Represents an anonymized user.
    /// </summary>
    [PublicAPI]
    [Table("AnonymizedUsers", Schema = "AnonymailModule")]
    public class AnonymizedUser : EFEntity
    {
        /// <summary>
        /// Gets the identity hash of the user. This value is deterministic on a per-user basis, and is computed as the
        /// SHA512 hash of the concatenation of the user's database ID, a semicolon, and the user's Discord ID. That is,
        /// for a user with the ID 1 and the Discord ID 135347310845624320, their identity hash would be computed as
        ///
        /// sha512("1:135347310845624320")
        ///
        /// This serves to decouple the user's actual identity from the anonymized user identity, while still
        /// maintaining a deterministic result from a real user to an anonymized user.
        ///
        /// This is not a perfect solution, and a true anonymization is not possible while at the same time maintaining
        /// the ability for the recipients of anonymous mail to block or restrict certain users from using the mailbox.
        /// It does, however, prevent accidental unmasking of users by database administrators or other passersby.
        /// </summary>
        public string IdentityHash { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymizedUser"/> class.
        /// </summary>
        /// <param name="identityHash">The identity hash.</param>
        public AnonymizedUser(string identityHash)
        {
            this.IdentityHash = identityHash;
        }
    }
}
