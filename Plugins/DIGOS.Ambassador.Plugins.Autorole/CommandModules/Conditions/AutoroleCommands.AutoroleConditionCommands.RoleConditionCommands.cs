//
//  AutoroleCommands.AutoroleConditionCommands.RoleConditionCommands.cs
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
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Autorole.CommandModules
{
    public partial class AutoroleCommands
    {
        public partial class AutoroleConditionCommands
        {
            /// <summary>
            /// Contains commands for adding or modifying a condition based on having a certain role.
            /// </summary>
            [Group("role")]
            public class RoleConditionCommands : ModuleBase
            {
                /// <summary>
                /// Adds the condition to the role, or modifies the existing condition.
                /// </summary>
                /// <param name="role">The role.</param>
                [UsedImplicitly]
                [Command]
                [Summary("Adds the condition to the role, or modifies the existing condition.")]
                public async Task AddOrModifyConditionAsync(IRole role)
                {
                }

                /// <summary>
                /// Removes the condition from the role.
                /// </summary>
                [UsedImplicitly]
                [Alias("remove")]
                [Command("remove")]
                [Summary("Removes the condition from the role.")]
                public async Task RemoveConditionAsync()
                {
                }
            }
        }
    }
}
