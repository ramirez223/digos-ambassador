﻿//
//  KinkService.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DIGOS.Ambassador.Core.Database.Extensions;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Core.Services.Users;
using DIGOS.Ambassador.Plugins.Kinks.Extensions;
using DIGOS.Ambassador.Plugins.Kinks.Model;
using Discord;
using Humanizer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Kinks.Services
{
    /// <summary>
    /// Service class for user kinks.
    /// </summary>
    [PublicAPI]
    public sealed class KinkService
    {
        private readonly KinksDatabaseContext _database;
        private readonly UserService _users;
        private readonly UserFeedbackService _feedback;

        /// <summary>
        /// Initializes a new instance of the <see cref="KinkService"/> class.
        /// </summary>
        /// <param name="feedback">The feedback service.</param>
        /// <param name="users">The user service.</param>
        /// <param name="database">The database.</param>
        public KinkService
        (
            UserFeedbackService feedback,
            UserService users,
            KinksDatabaseContext database
        )
        {
            _feedback = feedback;
            _users = users;
            _database = database;
        }

        /// <summary>
        /// Gets a kink by its name.
        /// </summary>
        /// <param name="name">The name of the kink.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<Kink>> GetKinkByNameAsync(string name, CancellationToken ct = default)
        {
            return await _database.Kinks.SelectFromBestLevenshteinMatchAsync
            (
                x => x,
                k => k.Name,
                name,
                ct: ct
            );
        }

        /// <summary>
        /// Builds an informational embed for the given kink.
        /// </summary>
        /// <param name="kink">The kink.</param>
        /// <returns>An embed.</returns>
        [Pure]
        public Embed BuildKinkInfoEmbed(Kink kink)
        {
            var eb = _feedback.CreateEmbedBase();

            eb.WithTitle(kink.Name.Transform(To.TitleCase));
            eb.WithDescription(kink.Description);

            return eb.Build();
        }

        /// <summary>
        /// Gets a user's kink preference by the kink name.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <param name="name">The kink name.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<UserKink>> GetUserKinkByNameAsync
        (
            IUser discordUser,
            string name,
            CancellationToken ct = default
        )
        {
            var getUserKinksResult = await GetUserKinksAsync(discordUser, ct: ct);
            if (!getUserKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<UserKink>.FromError(getUserKinksResult);
            }

            var userKinks = getUserKinksResult.Entity;

            return userKinks.SelectFromBestLevenshteinMatch(x => x, k => k.Kink.Name, name);
        }

        /// <summary>
        /// Builds an embed displaying the user's current preference for a given kink.
        /// </summary>
        /// <param name="userKink">The kink.</param>
        /// <returns>An embed.</returns>
        [Pure]
        public EmbedBuilder BuildUserKinkInfoEmbedBase(UserKink userKink)
        {
            var eb = _feedback.CreateEmbedBase();

            eb.AddField(userKink.Kink.Name.Transform(To.TitleCase), userKink.Kink.Description);
            eb.AddField("Current preference", userKink.Preference.Humanize());

            return eb;
        }

        /// <summary>
        /// Gets the given user's kink preferences.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <param name="query">Additional query statements.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>The user's kinks.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<IReadOnlyList<UserKink>>> GetUserKinksAsync
        (
            IUser discordUser,
            Func<IQueryable<UserKink>, IQueryable<UserKink>>? query = null,
            CancellationToken ct = default
        )
        {
            query ??= q => q;

            var getUserResult = await _users.GetOrRegisterUserAsync(discordUser, ct);
            if (!getUserResult.IsSuccess)
            {
                return RetrieveEntityResult<IReadOnlyList<UserKink>>.FromError(getUserResult);
            }

            var user = getUserResult.Entity;

            return RetrieveEntityResult<IReadOnlyList<UserKink>>.FromSuccess
            (
                await _database.UserKinks.ServersideQueryAsync(q => query(q.Where(k => k.User == user)), ct)
            );
        }

        /// <summary>
        /// Builds a paginated embed displaying the given overlapping kinks.
        /// </summary>
        /// <param name="firstUser">The first user.</param>
        /// <param name="secondUser">The second user.</param>
        /// <param name="kinks">The kinks.</param>
        /// <returns>A paginated embed.</returns>
        [Pure]
        public IEnumerable<EmbedBuilder> BuildKinkOverlapEmbeds
        (
            IUser firstUser,
            IUser secondUser,
            IEnumerable<UserKink> kinks
        )
        {
            var pages =
            (
                from batch in kinks.Batch(3)
                from kink in batch
                select BuildUserKinkInfoEmbedBase(kink).WithTitle
                (
                    $"Matching kinks between {firstUser.Mention} and {secondUser.Mention}"
                )
            )
            .ToList();

            return pages;
        }

        /// <summary>
        /// Builds a paginated embed displaying the given kinks.
        /// </summary>
        /// <param name="kinks">The kinks.</param>
        /// <returns>A paginated embed.</returns>
        [Pure]
        public IEnumerable<EmbedBuilder> BuildPaginatedUserKinkEmbeds
        (
            IEnumerable<UserKink> kinks
        )
        {
            var pages =
            (
                from batch in kinks.Batch(3)
                from kink in batch
                select BuildUserKinkInfoEmbedBase(kink)
            )
            .ToList();

            return pages;
        }

        /// <summary>
        /// Sets the user's preference for the given kink.
        /// </summary>
        /// <param name="userKink">The user's kink.</param>
        /// <param name="preference">The new preference.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A modification result which may or may not have succeeded.</returns>
        public async Task<ModifyEntityResult> SetKinkPreferenceAsync
        (
            UserKink userKink,
            KinkPreference preference,
            CancellationToken ct = default
        )
        {
            userKink.Preference = preference;
            await _database.SaveChangesAsync(ct);

            return ModifyEntityResult.FromSuccess();
        }

        /// <summary>
        /// Gets a user's kink preferences by the F-List kink ID.
        /// </summary>
        /// <param name="discordUser">The discord user.</param>
        /// <param name="onlineKinkID">The F-List kink ID.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>The user's kink preference.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<UserKink>> GetUserKinkByFListIDAsync
        (
            IUser discordUser,
            int onlineKinkID,
            CancellationToken ct = default
        )
        {
            var getKinkResult = await GetKinkByFListIDAsync(onlineKinkID, ct);
            if (!getKinkResult.IsSuccess)
            {
                return RetrieveEntityResult<UserKink>.FromError(getKinkResult);
            }

            var getUserKinksResult = await GetUserKinksAsync(discordUser, ct: ct);
            if (!getUserKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<UserKink>.FromError(getUserKinksResult);
            }

            var userKinks = getUserKinksResult.Entity;
            var userKink = userKinks.FirstOrDefault(k => k.Kink.FListID == onlineKinkID);

            if (!(userKink is null))
            {
                return RetrieveEntityResult<UserKink>.FromSuccess(userKink);
            }

            var kink = getKinkResult.Entity;
            var addKinkResult = await AddUserKinkAsync(discordUser, kink, ct);
            if (!addKinkResult.IsSuccess)
            {
                return RetrieveEntityResult<UserKink>.FromError(addKinkResult);
            }

            return RetrieveEntityResult<UserKink>.FromSuccess(addKinkResult.Entity);
        }

        /// <summary>
        /// Adds a kink to a user's preference list.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <param name="kink">The kink.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<UserKink>> AddUserKinkAsync
        (
            IUser discordUser,
            Kink kink,
            CancellationToken ct = default
        )
        {
            var getUserKinksResult = await GetUserKinksAsync(discordUser, ct: ct);
            if (!getUserKinksResult.IsSuccess)
            {
                return CreateEntityResult<UserKink>.FromError(getUserKinksResult);
            }

            var userKinks = getUserKinksResult.Entity;

            if (userKinks.Any(k => k.Kink.FListID == kink.FListID))
            {
                return CreateEntityResult<UserKink>.FromError("The user already has a preference for that kink.");
            }

            var getUserResult = await _users.GetOrRegisterUserAsync(discordUser, ct);
            if (!getUserResult.IsSuccess)
            {
                return CreateEntityResult<UserKink>.FromError(getUserResult);
            }

            var user = getUserResult.Entity;

            var userKink = _database.CreateProxy<UserKink>(user, kink);
            _database.UserKinks.Update(userKink);

            await _database.SaveChangesAsync(ct);

            return userKink;
        }

        /// <summary>
        /// Gets all available kink categories from the database.
        /// </summary>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A list of kink categories.</returns>
        [Pure]
        public async Task<IEnumerable<KinkCategory>> GetKinkCategoriesAsync(CancellationToken ct = default)
        {
            var categories = await _database.Kinks.ServersideQueryAsync
            (
                q => q.Select(k => k.Category),
                ct
            );

            return categories.OrderBy(k => k.ToString());
        }

        /// <summary>
        /// Gets a kink by its F-list ID.
        /// </summary>
        /// <param name="onlineKinkID">The F-List kink ID.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<Kink>> GetKinkByFListIDAsync
        (
            long onlineKinkID,
            CancellationToken ct = default
        )
        {
            var kink = await _database.Kinks.ServersideQueryAsync
            (
                q => q
                    .Where(k => k.FListID == onlineKinkID)
                    .SingleOrDefaultAsync(ct)
            );

            if (!(kink is null))
            {
                return kink;
            }

            return RetrieveEntityResult<Kink>.FromError("No kink with that ID found.");
        }

        /// <summary>
        /// Gets a list of all kinks in a given category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<IEnumerable<Kink>>> GetKinksByCategoryAsync
        (
            KinkCategory category,
            CancellationToken ct = default
        )
        {
            var kinks = await _database.Kinks.ServersideQueryAsync
            (
                q => q
                    .Where(k => k.Category == category),
                ct
            );

            var enumeratedKinks = kinks
                .OrderBy(g => g.Category.ToString())
                .ToList();

            if (!enumeratedKinks.Any())
            {
                return RetrieveEntityResult<IEnumerable<Kink>>.FromError
                (
                    "There are no kinks in that category."
                );
            }

            return enumeratedKinks;
        }

        /// <summary>
        /// Gets a list of all a user's kinks in a given category.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="category">The category.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<IEnumerable<UserKink>>> GetUserKinksByCategoryAsync
        (
            IUser user,
            KinkCategory category,
            CancellationToken ct = default
        )
        {
            var getUserKinksResult = await GetUserKinksAsync(user, ct: ct);
            if (!getUserKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<IEnumerable<UserKink>>.FromError(getUserKinksResult);
            }

            var userKinks = getUserKinksResult.Entity;

            var group = userKinks.Where(k => k.Kink.Category == category).ToList();
            if (!group.Any())
            {
                return RetrieveEntityResult<IEnumerable<UserKink>>.FromSuccess(new UserKink[] { });
            }

            return RetrieveEntityResult<IEnumerable<UserKink>>.FromSuccess(group.OrderBy(uk => uk.Kink.ToString()));
        }

        /// <summary>
        /// Resets the user's kink preferences.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A task that must be awaited.</returns>
        public async Task<ModifyEntityResult> ResetUserKinksAsync
        (
            IUser discordUser,
            CancellationToken ct = default
        )
        {
            var getUserResult = await _users.GetOrRegisterUserAsync(discordUser, ct);
            if (!getUserResult.IsSuccess)
            {
                return ModifyEntityResult.FromError(getUserResult);
            }

            var user = getUserResult.Entity;

            var kinksToRemove = await _database.UserKinks.ServersideQueryAsync
            (
                q => q.Where(k => k.User == user),
                ct
            );

            _database.UserKinks.RemoveRange(kinksToRemove);
            await _database.SaveChangesAsync(ct);

            return ModifyEntityResult.FromSuccess();
        }

        /// <summary>
        /// Gets the first kink that the given uses does not have a set preference for in the given category.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="category">The category.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<Kink>> GetFirstKinkWithoutPreferenceInCategoryAsync
        (
            IUser user,
            KinkCategory category,
            CancellationToken ct = default
        )
        {
            var getKinksResult = await GetKinksByCategoryAsync(category, ct);
            if (!getKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<Kink>.FromError(getKinksResult);
            }

            var kinks = getKinksResult.Entity;

            var getKinksByCategoryResult = await GetUserKinksByCategoryAsync(user, category, ct);
            if (!getKinksByCategoryResult.IsSuccess)
            {
                return RetrieveEntityResult<Kink>.FromError(getKinksByCategoryResult);
            }

            var userKinks = getKinksByCategoryResult.Entity.ToList();

            // Find the first kink that the user either has in their list with no preference, or does not exist
            // in their list
            var kinkWithoutPreference = kinks.FirstOrDefault
            (
                k =>
                    userKinks.Any
                    (
                        uk =>
                            k.FListID == uk.Kink.FListID && uk.Preference == KinkPreference.NoPreference
                    ) ||
                    userKinks.All
                    (
                        uk =>
                            k.FListID != uk.Kink.FListID
                    )
            );

            if (kinkWithoutPreference is null)
            {
                return RetrieveEntityResult<Kink>.FromError("No kink without a set preference found.");
            }

            return RetrieveEntityResult<Kink>.FromSuccess(kinkWithoutPreference);
        }

        /// <summary>
        /// Gets the first kink in the given category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<Kink>> GetFirstKinkInCategoryAsync
        (
            KinkCategory category,
            CancellationToken ct = default
        )
        {
            var getKinksResult = await GetKinksByCategoryAsync(category, ct);
            if (!getKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<Kink>.FromError(getKinksResult);
            }

            return RetrieveEntityResult<Kink>.FromSuccess(getKinksResult.Entity.First());
        }

        /// <summary>
        /// Gets the next kink in its category by its predecessor's F-List ID.
        /// </summary>
        /// <param name="precedingFListID">The F-List ID of the preceding kink.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        [Pure]
        public async Task<RetrieveEntityResult<Kink>> GetNextKinkByCurrentFListIDAsync
        (
            int precedingFListID,
            CancellationToken ct = default
        )
        {
            var getKinkResult = await GetKinkByFListIDAsync(precedingFListID, ct);
            if (!getKinkResult.IsSuccess)
            {
                return getKinkResult;
            }

            var currentKink = getKinkResult.Entity;
            var getKinksResult = await GetKinksByCategoryAsync(currentKink.Category, ct);
            if (!getKinksResult.IsSuccess)
            {
                return RetrieveEntityResult<Kink>.FromError(getKinksResult);
            }

            var group = getKinksResult.Entity;
            var nextKink = group.SkipUntil(k => k.FListID == precedingFListID).FirstOrDefault();

            if (nextKink is null)
            {
                return RetrieveEntityResult<Kink>.FromError("The current kink was the last one in the category.");
            }

            return RetrieveEntityResult<Kink>.FromSuccess(nextKink);
        }

        /// <summary>
        /// Updates the kink database, adding in new entries. Duplicates are not added.
        /// </summary>
        /// <param name="newKinks">The new kinks.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>The number of updated kinks.</returns>
        public async Task<int> UpdateKinksAsync
        (
            IEnumerable<Kink> newKinks,
            CancellationToken ct = default
        )
        {
            var alteredKinks = 0;
            foreach (var kink in newKinks)
            {
                var entry = _database.Kinks.Update(kink);
                if (entry.State != EntityState.Unchanged)
                {
                    ++alteredKinks;
                }
            }

            await _database.SaveChangesAsync(ct);

            return alteredKinks;
        }
    }
}
