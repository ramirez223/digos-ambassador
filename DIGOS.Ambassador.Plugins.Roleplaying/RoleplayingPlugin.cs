//
//  RoleplayingPlugin.cs
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
using System.Reflection;
using System.Threading.Tasks;
using DIGOS.Ambassador.Database.Abstractions.Extensions;
using DIGOS.Ambassador.Discord.Behaviours.Services;
using DIGOS.Ambassador.Discord.TypeReaders;
using DIGOS.Ambassador.Plugins.Abstractions;
using DIGOS.Ambassador.Plugins.Abstractions.Attributes;
using DIGOS.Ambassador.Plugins.Roleplaying;
using DIGOS.Ambassador.Plugins.Roleplaying.CommandModules;
using DIGOS.Ambassador.Plugins.Roleplaying.Model;
using DIGOS.Ambassador.Plugins.Roleplaying.Services;
using DIGOS.Ambassador.Plugins.Roleplaying.TypeReaders;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

[assembly: AmbassadorPlugin(typeof(RoleplayingPlugin))]

namespace DIGOS.Ambassador.Plugins.Roleplaying
{
    /// <summary>
    /// Describes the roleplay plugin.
    /// </summary>
    public class RoleplayingPlugin : PluginDescriptor
    {
        /// <inheritdoc />
        public override string Name => "Roleplays";

        /// <inheritdoc />
        public override string Description => "Provides user-managed roleplay libraries.";

        /// <inheritdoc />
        public override Task<bool> RegisterServicesAsync(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<RoleplayService>()
                .AddSchemaAwareDbContextPool<RoleplayingDatabaseContext>();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override async Task<bool> InitializeAsync(IServiceProvider serviceProvider)
        {
            var commands = serviceProvider.GetRequiredService<CommandService>();
            await commands.AddModuleAsync<RoleplayCommands>(serviceProvider);

            commands.AddTypeReader<IMessage>(new UncachedMessageTypeReader<IMessage>());
            commands.AddTypeReader<Roleplay>(new RoleplayTypeReader());

            var behaviours = serviceProvider.GetRequiredService<BehaviourService>();
            await behaviours.AddBehavioursAsync(Assembly.GetExecutingAssembly(), serviceProvider);

            return true;
        }
    }
}