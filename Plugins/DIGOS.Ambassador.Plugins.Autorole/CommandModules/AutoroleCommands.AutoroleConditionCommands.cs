//
//  AutoroleCommands.AutoroleConditionCommands.cs
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

using System.Threading.Tasks;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Extensions.Results;
using DIGOS.Ambassador.Plugins.Autorole.Model;
using DIGOS.Ambassador.Plugins.Autorole.Permissions;
using DIGOS.Ambassador.Plugins.Autorole.Services;
using DIGOS.Ambassador.Plugins.Permissions.Model;
using DIGOS.Ambassador.Plugins.Permissions.Preconditions;
using Discord.Commands;
using JetBrains.Annotations;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Autorole.CommandModules
{
    public partial class AutoroleCommands
    {
        /// <summary>
        /// Grouping module for condition-specific commands.
        /// </summary>
        [Group("condition")]
        public partial class AutoroleConditionCommands : ModuleBase
        {
            private readonly AutoroleService _autoroles;

            /// <summary>
            /// Initializes a new instance of the <see cref="AutoroleConditionCommands"/> class.
            /// </summary>
            /// <param name="autoroles">The autorole service.</param>
            public AutoroleConditionCommands(AutoroleService autoroles)
            {
                _autoroles = autoroles;
            }

            /// <summary>
            /// Removes a condition from a role.
            /// </summary>
            /// <param name="autorole">The autorole.</param>
            /// <param name="conditionID">The ID of the condition to remove.</param>
            [UsedImplicitly]
            [Alias("remove")]
            [Command("remove")]
            [Summary("Removes the condition from the role.")]
            [RequirePermission(typeof(EditAutorole), PermissionTarget.Self)]
            public async Task<RuntimeResult> RemoveConditionAsync
            (
                AutoroleConfiguration autorole,
                long conditionID
            )
            {
                var removeCondition = await _autoroles.RemoveConditionAsync(autorole, conditionID);
                if (!removeCondition.IsSuccess)
                {
                    return removeCondition.ToRuntimeResult();
                }

                return RuntimeCommandResult.FromSuccess("Condition removed.");
            }
        }
    }
}
