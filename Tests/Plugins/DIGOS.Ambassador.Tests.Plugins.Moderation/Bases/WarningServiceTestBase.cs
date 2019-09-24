//
//  WarningServiceTestBase.cs
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
using DIGOS.Ambassador.Plugins.Core.Model;
using DIGOS.Ambassador.Plugins.Core.Services.Servers;
using DIGOS.Ambassador.Plugins.Core.Services.Users;
using DIGOS.Ambassador.Plugins.Moderation.Model;
using DIGOS.Ambassador.Plugins.Moderation.Services;
using DIGOS.Ambassador.Tests.Extensions;
using DIGOS.Ambassador.Tests.TestBases;
using Microsoft.Extensions.DependencyInjection;

namespace DIGOS.Ambassador.Tests.Plugins.Moderation.Bases
{
    /// <summary>
    /// Serves as a test base for warning service tests.
    /// </summary>
    public class WarningServiceTestBase : DatabaseProvidingTestBase
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        protected ModerationDatabaseContext Database { get; private set; }

        /// <summary>
        /// Gets the warning service.
        /// </summary>
        protected WarningService Warnings { get; private set; }

        /// <inheritdoc />
        protected override void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddDbContext<CoreDatabaseContext>(ConfigureOptions<CoreDatabaseContext>)
                .AddDbContext<ModerationDatabaseContext>(ConfigureOptions<ModerationDatabaseContext>);

            serviceCollection
                .AddScoped<ServerService>()
                .AddScoped<UserService>()
                .AddScoped<WarningService>();
        }

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceProvider serviceProvider)
        {
            var coreDatabase = serviceProvider.GetRequiredService<CoreDatabaseContext>();
            coreDatabase.Database.Create();

            var warningDatabase = serviceProvider.GetRequiredService<ModerationDatabaseContext>();
            warningDatabase.Database.Create();

            this.Database = warningDatabase;
            this.Warnings = serviceProvider.GetRequiredService<WarningService>();
        }
    }
}