//
//  MessageCountInGuildConditionCommands.cs
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
using DIGOS.Ambassador.Plugins.Autorole.Model.Conditions;
using DIGOS.Ambassador.Plugins.Autorole.Permissions;
using DIGOS.Ambassador.Plugins.Autorole.Services;
using DIGOS.Ambassador.Plugins.Permissions.Preconditions;
using Discord.Commands;
using JetBrains.Annotations;
using PermissionTarget = DIGOS.Ambassador.Plugins.Permissions.Model.PermissionTarget;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Autorole.CommandModules
{
    public partial class AutoroleCommands
    {
        public partial class AutoroleConditionCommands
        {
            /// <summary>
            /// Contains commands for adding or modifying a condition based on a certain number of messages in a guild.
            /// </summary>
            [Group("total-messages")]
            public class MessageCountInGuildConditionCommands : ModuleBase
            {
                private readonly AutoroleService _autoroles;

                /// <summary>
                /// Initializes a new instance of the <see cref="MessageCountInGuildConditionCommands"/> class.
                /// </summary>
                /// <param name="autoroles">The autorole service.</param>
                public MessageCountInGuildConditionCommands(AutoroleService autoroles)
                {
                    _autoroles = autoroles;
                }

                /// <summary>
                /// Adds an instance of the condition to the role.
                /// </summary>
                /// <param name="autorole">The autorole configuration.</param>
                /// <param name="count">The message count.</param>
                [UsedImplicitly]
                [Command]
                [Summary("Adds an instance of the condition to the role.")]
                [RequireContext(ContextType.Guild)]
                [RequirePermission(typeof(EditAutorole), PermissionTarget.Self)]
                public async Task<RuntimeResult> AddConditionAsync(AutoroleConfiguration autorole, long count)
                {
                    var condition = _autoroles.CreateConditionProxy<MessageCountInGuildCondition>
                    (
                        this.Context.Guild,
                        count
                    );

                    if (condition is null)
                    {
                        return RuntimeCommandResult.FromError("Failed to create a condition object. Yikes!");
                    }

                    var addCondition = await _autoroles.AddConditionAsync(autorole, condition);
                    if (!addCondition.IsSuccess)
                    {
                        return addCondition.ToRuntimeResult();
                    }

                    return RuntimeCommandResult.FromSuccess("Condition added.");
                }

                /// <summary>
                /// Modifies an instance of the condition on the role.
                /// </summary>
                /// <param name="autorole">The autorole configuration.</param>
                /// <param name="conditionID">The ID of the condition.</param>
                /// <param name="count">The message count.</param>
                [UsedImplicitly]
                [Command]
                [Summary("Modifies an instance of the condition on the role.")]
                [RequireContext(ContextType.Guild)]
                [RequirePermission(typeof(EditAutorole), PermissionTarget.Self)]
                public async Task<RuntimeResult> ModifyConditionAsync
                (
                    AutoroleConfiguration autorole,
                    long conditionID,
                    long count
                )
                {
                    var getCondition = _autoroles.GetCondition<MessageCountInGuildCondition>
                    (
                        autorole,
                        conditionID
                    );

                    if (!getCondition.IsSuccess)
                    {
                        return getCondition.ToRuntimeResult();
                    }

                    var condition = getCondition.Entity;
                    var modifyResult = await _autoroles.ModifyConditionAsync
                    (
                        condition,
                        c =>
                        {
                            condition.RequiredCount = count;
                            condition.SourceID = (long)this.Context.Guild.Id;
                        }
                    );

                    if (!modifyResult.IsSuccess)
                    {
                        return modifyResult.ToRuntimeResult();
                    }

                    return RuntimeCommandResult.FromSuccess("Condition updated.");
                }
            }
        }
    }
}