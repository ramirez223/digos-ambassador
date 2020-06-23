//
//  AnonymailService.cs
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

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DIGOS.Ambassador.Core.Database.Extensions;
using DIGOS.Ambassador.Plugins.Anonymail.Model;
using DIGOS.Ambassador.Plugins.Core.Model.Servers;
using DIGOS.Ambassador.Plugins.Core.Model.Users;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Anonymail.Services
{
    /// <summary>
    /// Contains business logic for handling anonymous mails.
    /// </summary>
    [PublicAPI]
    public class AnonymailService
    {
        private readonly AnonymailDatabaseContext _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymailService"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        public AnonymailService(AnonymailDatabaseContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Sends an anonymous mail to the given mailbox with the given contents.
        /// </summary>
        /// <param name="user">The author of the mail.</param>
        /// <param name="mailbox">The mailbox to send to.</param>
        /// <param name="contents">The contents of the mail.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public CreateEntityResult<AnonymousMail> SendMail
        (
            AnonymizedUser user,
            AnonymousMailbox mailbox,
            string contents
        )
        {
            var isUserBlocked = mailbox.MailboxUsers.Any(mu => mu.User == user && mu.IsBlocked);
            if (isUserBlocked)
            {
                return CreateEntityResult<AnonymousMail>.FromError
                (
                    "The user isn't allowed to send messages to this mailbox."
                );
            }

            var mail = _database.CreateProxy<AnonymousMail>(user, contents);
            mailbox.Mails.Add(mail);

            return mail;
        }

        /// <summary>
        /// Determines whether a mailbox with the given name exists on the given server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="name">The name.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>true if a mailbox exists; otherwise, false.</returns>
        public Task<bool> HasMailboxAsync(Server server, string name, CancellationToken ct = default)
        {
            server = _database.NormalizeReference(server);

            return _database.AnonymousMailboxes.AsQueryable().AnyAsync
            (
                m => m.Server == server && m.Name == name, ct
            );
        }

        /// <summary>
        /// Gets the mailbox with the given name on the given server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="name">The name of the mailbox.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<RetrieveEntityResult<AnonymousMailbox>> GetMailboxAsync
        (
            Server server,
            string name,
            CancellationToken ct = default
        )
        {
            server = _database.NormalizeReference(server);

            var result = await _database.AnonymousMailboxes.AsQueryable().FirstOrDefaultAsync
            (
                m => m.Server == server && m.Name == name,
                ct
            );

            return result ?? RetrieveEntityResult<AnonymousMailbox>.FromError("There's no mailbox with that name.");
        }

        /// <summary>
        /// Creates a mailbox on the given server with the given name, linked to the given channel ID.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="name">The name of the mailbox.</param>
        /// <param name="channelID">The ID of the linked channel.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<AnonymousMailbox>> CreateMailboxAsync
        (
            Server server,
            string name,
            long channelID,
            CancellationToken ct = default
        )
        {
            server = _database.NormalizeReference(server);

            if (await HasMailboxAsync(server, name, ct))
            {
                return CreateEntityResult<AnonymousMailbox>.FromError("A mailbox with that name already exists.");
            }

            var mailbox = _database.CreateProxy<AnonymousMailbox>(server, name, channelID);
            _database.AnonymousMailboxes.Update(mailbox);

            return mailbox;
        }

        /// <summary>
        /// Deletes the given mailbox from the database.
        /// </summary>
        /// <param name="mailbox">The mailbox to delete.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A deletion result which may or may not have succeeded.</returns>
        public async Task<DeleteEntityResult> DeleteMailboxAsync
        (
            AnonymousMailbox mailbox,
            CancellationToken ct = default
        )
        {
            if (!await _database.AnonymousMailboxes.AsQueryable().ContainsAsync(mailbox, ct))
            {
                return DeleteEntityResult.FromError("There's no mailbox like that in the database.");
            }

            _database.AnonymousMailboxes.Remove(mailbox);

            return DeleteEntityResult.FromSuccess();
        }

        /// <summary>
        /// Gets or creates an anonymized user for the given real user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<RetrieveEntityResult<AnonymizedUser>> GetOrCreateAnonymizedUserAsync
        (
            User user,
            CancellationToken ct = default
        )
        {
            user = _database.NormalizeReference(user);

            if (!await HasAnonymizedUserAsync(user, ct))
            {
                return CreateAnonymizedUser(user);
            }

            return await GetAnonymizedUserAsync(user, ct);
        }

        /// <summary>
        /// Determines whether an anonymized entry for the given user already exists in the database.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>true if an anonymized user already exists; otherwise, false.</returns>
        public Task<bool> HasAnonymizedUserAsync(User user, CancellationToken ct = default)
        {
            user = _database.NormalizeReference(user);

            var identityHash = CreateIdentityHash(user);
            return _database.AnonymizedUsers.AsQueryable().AnyAsync
            (
                u => u.IdentityHash == identityHash,
                ct
            );
        }

        /// <summary>
        /// Anonymizes the given user.
        /// </summary>
        /// <param name="user">The user to anonymize.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public AnonymizedUser CreateAnonymizedUser(User user)
        {
            user = _database.NormalizeReference(user);

            var identityHash = CreateIdentityHash(user);

            var anonymizedUser = _database.CreateProxy<AnonymizedUser>(identityHash);
            _database.AnonymizedUsers.Update(anonymizedUser);

            return anonymizedUser;
        }

        /// <summary>
        /// Gets the anonymized user that maps to the given real user from the database.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The anonymized user.</returns>
        public Task<AnonymizedUser> GetAnonymizedUserAsync(User user, CancellationToken ct = default)
        {
            user = _database.NormalizeReference(user);

            var identityHash = CreateIdentityHash(user);

            return _database.AnonymizedUsers.AsQueryable().FirstAsync
            (
                u => u.IdentityHash == identityHash,
                ct
            );
        }

        /// <summary>
        /// Creates an identity hash from the given user. This value is deterministic on a per-user basis, and is
        /// computed as the SHA512 hash of the UTF-8 concatenation of the user's database ID, a semicolon, and the
        /// user's Discord ID. That is, for a user with the ID 1 and the Discord ID 135347310845624320, their identity
        /// hash would be computed as
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
        /// <param name="user">The user to generate the identity hash of.</param>
        /// <returns>The identity hash.</returns>
        private string CreateIdentityHash(User user)
        {
            using var sha256 = SHA256.Create();
            var identity = $"{user.ID}:{user.DiscordID}";
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(identity));

            var builder = new StringBuilder();
            foreach (var value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
