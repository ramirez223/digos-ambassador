﻿//
//  DossierService.cs
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DIGOS.Ambassador.Core.Database.Extensions;
using DIGOS.Ambassador.Core.Extensions;
using DIGOS.Ambassador.Core.Services;
using DIGOS.Ambassador.Plugins.Dossiers.Extensions;
using DIGOS.Ambassador.Plugins.Dossiers.Model;
using DIGOS.Ambassador.Plugins.Dossiers.Signatures;
using Discord;
using Humanizer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Zio;

namespace DIGOS.Ambassador.Plugins.Dossiers.Services
{
    /// <summary>
    /// Handles dossier management.
    /// </summary>
    [PublicAPI]
    public sealed class DossierService
    {
        private readonly DossiersDatabaseContext _database;
        private readonly ContentService _content;

        /// <summary>
        /// Gets the base dossier path.
        /// </summary>
        private UPath BaseDossierPath => UPath.Combine(UPath.Root, "Dossiers");

        /// <summary>
        /// Initializes a new instance of the <see cref="DossierService"/> class.
        /// </summary>
        /// <param name="content">The content service.</param>
        /// <param name="database">The dossier database context.</param>
        public DossierService
        (
            ContentService content,
            DossiersDatabaseContext database
        )
        {
            _content = content;
            _database = database;
        }

        /// <summary>
        /// Gets the available dossiers.
        /// </summary>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public Task<IReadOnlyList<Dossier>> GetDossiersAsync(CancellationToken ct = default)
        {
            return _database.Dossiers.ServersideQueryAsync(q => q, ct);
        }

        /// <summary>
        /// Gets a given dossier's data.
        /// </summary>
        /// <param name="dossier">The dossier to get the data for.</param>
        /// <returns>A <see cref="FileStream"/> containing the dossier data.</returns>
        public RetrieveEntityResult<Stream> GetDossierStream(Dossier dossier)
        {
            var dataPath = GetDossierDataPath(dossier);

            if (!_content.FileSystem.FileExists(dataPath))
            {
                return RetrieveEntityResult<Stream>.FromError("No file data set.");
            }

            return _content.OpenLocalStream(dataPath);
        }

        /// <summary>
        /// Deletes the content data associated with a given dossier.
        /// </summary>
        /// <param name="dossier">The dossier.</param>
        /// <returns>A deletion result which may or may not have succeeded.</returns>
        public Task<DeleteEntityResult> DeleteDossierDataAsync
        (
            Dossier dossier
        )
        {
            var dataPath = GetDossierDataPath(dossier);
            if (!_content.FileSystem.FileExists(dataPath))
            {
                return Task.FromResult(DeleteEntityResult.FromSuccess());
            }

            try
            {
                _content.FileSystem.DeleteFile(dataPath);
            }
            catch (Exception e)
            {
                return Task.FromResult(DeleteEntityResult.FromError(e.Message));
            }

            return Task.FromResult(DeleteEntityResult.FromSuccess());
        }

        /// <summary>
        /// Gets the absolute path to where the data of the dossier is stored.
        /// </summary>
        /// <param name="dossier">The dossier.</param>
        /// <returns>The path.</returns>
        public UPath GetDossierDataPath(Dossier dossier)
        {
            return UPath.Combine(this.BaseDossierPath, $"{dossier.Title}.pdf");
        }

        /// <summary>
        /// Creates a new dossier with the given title, summary, and data.
        /// </summary>
        /// <param name="title">The title of the dossier.</param>
        /// <param name="summary">The summary of the dossier.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A creation task which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<Dossier>> CreateDossierAsync
        (
            string title,
            string summary,
            CancellationToken ct = default
        )
        {
            var dossier = _database.CreateProxy<Dossier>(title, summary);
            _database.Dossiers.Update(dossier);

            var setTitleResult = await SetDossierTitleAsync(dossier, title, ct);
            if (!setTitleResult.IsSuccess)
            {
                return CreateEntityResult<Dossier>.FromError(setTitleResult);
            }

            var setSummary = await SetDossierSummaryAsync(dossier, summary, ct);
            if (!setSummary.IsSuccess)
            {
                return CreateEntityResult<Dossier>.FromError(setSummary);
            }

            await _database.SaveChangesAsync(ct);

            return dossier;
        }

        /// <summary>
        /// Deletes a dossier from the database.
        /// </summary>
        /// <param name="dossier">The dossier to delete.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A deletion result which may or may not have succeeded.</returns>
        public async Task<DeleteEntityResult> DeleteDossierAsync
        (
            Dossier dossier,
            CancellationToken ct = default
        )
        {
            var deleteContentResult = await DeleteDossierDataAsync(dossier);
            if (!deleteContentResult.IsSuccess)
            {
                return deleteContentResult;
            }

            _database.Dossiers.Remove(dossier);
            await _database.SaveChangesAsync(ct);

            return DeleteEntityResult.FromSuccess();
        }

        /// <summary>
        /// Determines whether or not the given dossier title is unique. This method is case-insensitive.
        /// </summary>
        /// <param name="dossierTitle">The title of the dossier.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns><value>true</value> if the title is unique; otherwise,<value>false</value>.</returns>
        public async Task<bool> IsDossierTitleUniqueAsync
        (
            string dossierTitle,
            CancellationToken ct = default
        )
        {
            var hasDossier = await _database.Dossiers.ServersideQueryAsync
            (
                q => q
                    .Where(d => d.Title.ToLowerInvariant() == dossierTitle.ToLowerInvariant())
                    .AnyAsync(ct)
            );

            return !hasDossier;
        }

        /// <summary>
        /// Gets a dossier by its title.
        /// </summary>
        /// <param name="title">The title of the dossier.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>A retrieval task that may or may not have succeeded.</returns>
        public async Task<RetrieveEntityResult<Dossier>> GetDossierByTitleAsync
        (
            string title,
            CancellationToken ct = default
        )
        {
            var dossier = await _database.Dossiers.ServersideQueryAsync
            (
                q => q
                    .Where(d => d.Title.ToLowerInvariant() == title.ToLowerInvariant())
                    .SingleOrDefaultAsync(ct)
            );

            if (!(dossier is null))
            {
                return dossier;
            }

            return RetrieveEntityResult<Dossier>.FromError("No dossier with that title found.");
        }

        /// <summary>
        /// Sets the title of the dossier.
        /// </summary>
        /// <param name="dossier">The dossier to modify.</param>
        /// <param name="newTitle">The new title.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>An entity modification result which may or may not have succeeded.</returns>
        public async Task<ModifyEntityResult> SetDossierTitleAsync
        (
            Dossier dossier,
            string newTitle,
            CancellationToken ct = default
        )
        {
            var isNewNameUnique = await IsDossierTitleUniqueAsync(newTitle, ct);

            // If the only thing that has changed is casing, let it through
            if (!isNewNameUnique)
            {
                var isOnlyCaseChange = string.Equals(dossier.Title, newTitle, StringComparison.OrdinalIgnoreCase);

                if (!isOnlyCaseChange)
                {
                    return ModifyEntityResult.FromError("A dossier with that title already exists.");
                }
            }

            if (newTitle.Contains("\""))
            {
                return ModifyEntityResult.FromError("The title may not contain double quotes.");
            }

            if (newTitle.IndexOfAny(Path.GetInvalidPathChars()) > -1)
            {
                return ModifyEntityResult.FromError
                (
                    "The title contains one or more of invalid characters " +
                    $"({Path.GetInvalidPathChars().Humanize("or")})"
                );
            }

            var updateDataResult = await UpdateDossierDataLocationAsync(dossier, newTitle, ct);
            if (!updateDataResult.IsSuccess)
            {
                return updateDataResult;
            }

            dossier.Title = newTitle;
            await _database.SaveChangesAsync(ct);

            return ModifyEntityResult.FromSuccess();
        }

        /// <summary>
        /// Sets the summary of the dossier.
        /// </summary>
        /// <param name="dossier">The dossier to modify.</param>
        /// /// <param name="newSummary">The new summary.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>An entity modification result which may or may not have succeeded.</returns>
        public async Task<ModifyEntityResult> SetDossierSummaryAsync
        (
            Dossier dossier,
            string newSummary,
            CancellationToken ct = default
        )
        {
            if (newSummary.IsNullOrWhitespace())
            {
                return ModifyEntityResult.FromError("You need to provide a summary.");
            }

            dossier.Summary = newSummary;
            await _database.SaveChangesAsync(ct);

            return ModifyEntityResult.FromSuccess();
        }

        /// <summary>
        /// Updates the location of the dossier data, matching it to the dossier's name.
        /// </summary>
        /// <param name="dossier">The dossier to update.</param>
        /// <param name="newTitle">The new dossier title.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>An entity modification result which may or may not have succeeded.</returns>
        public async Task<ModifyEntityResult> UpdateDossierDataLocationAsync
        (
            Dossier dossier,
            string newTitle,
            CancellationToken ct = default
        )
        {
            var originalDossierPath = GetDossierDataPath(dossier);

            var newDossierPath = UPath.Combine(this.BaseDossierPath, $"{newTitle}.pdf");

            if (!_content.FileSystem.FileExists(originalDossierPath) || originalDossierPath == newDossierPath)
            {
                return ModifyEntityResult.FromSuccess();
            }

            try
            {
                _content.FileSystem.MoveFile(originalDossierPath, newDossierPath);
            }
            catch (Exception e)
            {
                return ModifyEntityResult.FromError(e.Message);
            }

            await _database.SaveChangesAsync(ct);

            return ModifyEntityResult.FromSuccess();
        }

        /// <summary>
        /// Sets the PDF data of a given dossier. This overwrites existing data.
        /// </summary>
        /// <param name="dossier">The dosser for which to set the data.</param>
        /// <param name="message">The message containing the PDF data.</param>
        /// <param name="ct">The cancellation token in use.</param>
        /// <returns>An entity modification result which may or may not have succeeded.</returns>
        public async Task<ModifyEntityResult> SetDossierDataAsync
        (
            Dossier dossier,
            IUserMessage message,
            CancellationToken ct = default
        )
        {
            if (message.Attachments.Count <= 0)
            {
                return ModifyEntityResult.FromError("No file provided. Please attach a PDF with the dossier data.");
            }

            var dossierAttachment = message.Attachments.First();
            if (!dossierAttachment.Filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ModifyEntityResult.FromError("Invalid dossier format. PDF files are accepted.");
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            try
            {
                var dossierPath = GetDossierDataPath(dossier);
                await using var dataStream = await client.GetStreamAsync(dossierAttachment.Url);

                try
                {
                    await using var dataFile = _content.FileSystem.CreateFile(dossierPath);
                    await dataStream.CopyToAsync(dataFile, ct);

                    if (!await dataFile.HasSignatureAsync(FileSignatures.PDF))
                    {
                        return ModifyEntityResult.FromError
                        (
                            "Invalid dossier format. PDF files are accepted."
                        );
                    }
                }
                catch (Exception e)
                {
                    return ModifyEntityResult.FromError(e);
                }
            }
            catch (TaskCanceledException)
            {
                return ModifyEntityResult.FromError
                (
                    "The download operation timed out. The data file was not added."
                );
            }

            return ModifyEntityResult.FromSuccess();
        }
    }
}
