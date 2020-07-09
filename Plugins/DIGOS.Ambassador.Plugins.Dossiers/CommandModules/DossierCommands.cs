﻿//
//  DossierCommands.cs
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
using System.Threading.Tasks;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Extensions.Results;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Discord.Interactivity;
using DIGOS.Ambassador.Discord.Pagination;
using DIGOS.Ambassador.Plugins.Dossiers.Model;
using DIGOS.Ambassador.Plugins.Dossiers.Services;

using Discord;
using Discord.Commands;
using JetBrains.Annotations;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Dossiers.CommandModules
{
    /// <summary>
    /// Commands for viewing, adding, and editing dossier entries.
    /// </summary>
    [Group("dossier")]
    [Summary("Commands for viewing, adding, and editing dossier entries.")]
    public class DossierCommands : ModuleBase
    {
        private readonly UserFeedbackService _feedback;
        private readonly DossierService _dossiers;
        private readonly InteractivityService _interactivity;

        /// <summary>
        /// Initializes a new instance of the <see cref="DossierCommands"/> class.
        /// </summary>
        /// <param name="feedback">The feedback service.</param>
        /// <param name="dossiers">The dossier service.</param>
        /// <param name="interactivity">The interactivity service.</param>
        public DossierCommands
        (
            UserFeedbackService feedback,
            DossierService dossiers,
            InteractivityService interactivity
        )
        {
            _feedback = feedback;
            _dossiers = dossiers;
            _interactivity = interactivity;
        }

        /// <summary>
        /// Lists the available dossiers.
        /// </summary>
        [UsedImplicitly]
        [Command("list")]
        [Summary("Lists the available dossiers.")]
        public async Task<RuntimeResult> ListDossiersAsync()
        {
            var appearance = PaginatedAppearanceOptions.Default;
            appearance.Title = "Dossier Database";

            var dossiers = await _dossiers.GetDossiersAsync();

            var paginatedEmbed = PaginatedEmbedFactory.SimpleFieldsFromCollection
            (
                _feedback,
                _interactivity,
                this.Context.User,
                dossiers,
                d => d.Title,
                d => d.Summary,
                "There are no dossiers available.",
                appearance
            );

            await _interactivity.SendInteractiveMessageAndDeleteAsync
            (
                this.Context.Channel,
                paginatedEmbed,
                TimeSpan.FromMinutes(5.0)
            );

            return RuntimeCommandResult.FromSuccess();
        }

        /// <summary>
        /// Views the named dossier.
        /// </summary>
        /// <param name="title">The title of the dossier to view.</param>
        [UsedImplicitly]
        [Alias("view", "show")]
        [Command("view")]
        [Summary("Views the named dossier.")]
        public async Task<RuntimeResult> ViewDossierAsync(string title)
        {
            var getDossierResult = await _dossiers.GetDossierByTitleAsync(title);
            if (!getDossierResult.IsSuccess)
            {
                return getDossierResult.ToRuntimeResult();
            }

            var dossier = getDossierResult.Entity;

            var eb = BuildDossierEmbed(dossier);
            await _feedback.SendEmbedAsync(this.Context.Channel, eb);

            var dossierDataResult = _dossiers.GetDossierStream(dossier);
            if (!dossierDataResult.IsSuccess)
            {
                return dossierDataResult.ToRuntimeResult();
            }

            await using var dossierData = dossierDataResult.Entity;
            await this.Context.Channel.SendFileAsync(dossierData, $"{dossier.Title}.pdf");

            return RuntimeCommandResult.FromSuccess();
        }

        private Embed BuildDossierEmbed(Dossier dossier)
        {
            var eb = _feedback.CreateEmbedBase();
            eb.WithTitle(dossier.Title);
            eb.WithDescription(dossier.Summary);

            return eb.Build();
        }

        /// <summary>
        /// Adds a new dossier with the given title and summary. A PDF with the full dossier can be attached.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="summary">The summary.</param>
        [UsedImplicitly]
        [Alias("add", "create")]
        [Command("add")]
        [Summary("Adds a new dossier with the given title and summary. A PDF with the full dossier can be attached.")]
        [RequireOwner]
        public async Task<RuntimeResult> AddDossierAsync(string title, string summary = "No summary set.")
        {
            var dossierCreationResult = await _dossiers.CreateDossierAsync(title, summary);
            if (!dossierCreationResult.IsSuccess)
            {
                return dossierCreationResult.ToRuntimeResult();
            }

            var dossier = dossierCreationResult.Entity;

            var modifyResult = await _dossiers.SetDossierDataAsync(dossier, this.Context.Message);
            if (!modifyResult.IsSuccess)
            {
                return modifyResult.ToRuntimeResult();
            }

            return RuntimeCommandResult.FromSuccess($"Dossier \"{dossier.Title}\" added.");
        }

        /// <summary>
        /// Removes the dossier with the given title.
        /// </summary>
        /// <param name="title">The title.</param>
        [UsedImplicitly]
        [Alias("remove", "delete")]
        [Command("remove")]
        [Summary("Removes the dossier with the given title.")]
        [RequireOwner]
        public async Task<RuntimeResult> RemoveDossierAsync(string title)
        {
            var getDossierResult = await _dossiers.GetDossierByTitleAsync(title);
            if (!getDossierResult.IsSuccess)
            {
                return getDossierResult.ToRuntimeResult();
            }

            var dossier = getDossierResult.Entity;
            var deleteDossierResult = await _dossiers.DeleteDossierAsync(dossier);
            if (!deleteDossierResult.IsSuccess)
            {
                return getDossierResult.ToRuntimeResult();
            }

            return RuntimeCommandResult.FromSuccess($"Dossier \"{dossier.Title}\" deleted.");
        }

        /// <summary>
        /// Setters for dossier properties.
        /// </summary>
        [Group("set")]
        public class SetCommands : ModuleBase
        {
            private readonly DossierService _dossiers;

            /// <summary>
            /// Initializes a new instance of the <see cref="SetCommands"/> class.
            /// </summary>
            /// <param name="dossiers">The dossier service.</param>
            public SetCommands(DossierService dossiers)
            {
                _dossiers = dossiers;
            }

            /// <summary>
            /// Sets the title of the given dossier.
            /// </summary>
            /// <param name="title">The title of the dossier to edit.</param>
            /// <param name="newTitle">The new title of the dossier.</param>
            [UsedImplicitly]
            [Command("title")]
            [Summary("Sets the title of the given dossier.")]
            [RequireOwner]
            public async Task<RuntimeResult> SetTitleAsync(string title, string newTitle)
            {
                var getDossierResult = await _dossiers.GetDossierByTitleAsync(title);
                if (!getDossierResult.IsSuccess)
                {
                    return getDossierResult.ToRuntimeResult();
                }

                var dossier = getDossierResult.Entity;

                var modifyResult = await _dossiers.SetDossierTitleAsync(dossier, newTitle);
                if (!modifyResult.IsSuccess)
                {
                    return modifyResult.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("New dossier title set.");
            }

            /// <summary>
            /// Sets the summary of the given dossier.
            /// </summary>
            /// <param name="title">The title of the dossier to edit.</param>
            /// <param name="newSummary">The new summary of the dossier.</param>
            [UsedImplicitly]
            [Command("summary")]
            [Summary("Sets the summary of the given dossier.")]
            [RequireOwner]
            public async Task<RuntimeResult> SetSummaryAsync(string title, string newSummary)
            {
                var getDossierResult = await _dossiers.GetDossierByTitleAsync(title);
                if (!getDossierResult.IsSuccess)
                {
                    return getDossierResult.ToRuntimeResult();
                }

                var dossier = getDossierResult.Entity;

                var modifyResult = await _dossiers.SetDossierSummaryAsync(dossier, newSummary);
                if (!modifyResult.IsSuccess)
                {
                    return modifyResult.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("New dossier summary set.");
            }

            /// <summary>
            /// Sets the data of the given dossier. Attach a PDF to the command.
            /// </summary>
            /// <param name="title">The title of the dossier to edit.</param>
            [UsedImplicitly]
            [Command("data")]
            [Summary("Sets the data of the given dossier. Attach a PDF to the command.")]
            [RequireOwner]
            public async Task<RuntimeResult> SetFileAsync(string title)
            {
                var getDossierResult = await _dossiers.GetDossierByTitleAsync(title);
                if (!getDossierResult.IsSuccess)
                {
                    return getDossierResult.ToRuntimeResult();
                }

                var dossier = getDossierResult.Entity;

                var modifyResult = await _dossiers.SetDossierDataAsync(dossier, this.Context.Message);
                if (!modifyResult.IsSuccess)
                {
                    return modifyResult.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("Dossier data set.");
            }
        }
    }
}
