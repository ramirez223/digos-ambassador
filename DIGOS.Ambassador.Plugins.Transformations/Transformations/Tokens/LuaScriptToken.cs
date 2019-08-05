﻿//
//  LuaScriptToken.cs
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
using DIGOS.Ambassador.Core.Services;
using DIGOS.Ambassador.Plugins.Transformations.Extensions;
using DIGOS.Ambassador.Plugins.Transformations.Model.Appearances;
using DIGOS.Ambassador.Plugins.Transformations.Services.Lua;

namespace DIGOS.Ambassador.Plugins.Transformations.Transformations.Tokens
{
    /// <summary>
    /// Represents a token which executes a named lua code script and gets replaced with the result.
    /// </summary>
    [TokenIdentifier("script", "sc")]
    public class LuaScriptToken : ReplacableTextToken<LuaScriptToken>
    {
        private readonly ContentService _content;
        private readonly LuaService _lua;

        /// <summary>
        /// Gets the name of the script to execute.
        /// </summary>
        public string ScriptName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptToken"/> class.
        /// </summary>
        /// <param name="luaService">The lua execution service.</param>
        /// <param name="content">The application's content service.</param>
        public LuaScriptToken(LuaService luaService, ContentService content)
        {
            _lua = luaService;
            _content = content;
        }

        /// <inheritdoc />
        public override string GetText(AppearanceConfiguration appearanceConfiguration, AppearanceComponent component)
        {
            return GetTextAsync(appearanceConfiguration, component).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override async Task<string> GetTextAsync(AppearanceConfiguration appearanceConfiguration, AppearanceComponent component)
        {
            if (component is null)
            {
                return string.Empty;
            }

            var scriptPath = _content.GetLuaScriptPath(component.Transformation, this.ScriptName);
            var result = await _lua.ExecuteScriptAsync
            (
                scriptPath,
                (nameof(appearanceConfiguration), appearanceConfiguration),
                ("character", appearanceConfiguration.Character),
                (nameof(component), component)
            );

            if (!result.IsSuccess)
            {
                return $"[{result.ErrorReason}]";
            }

            return result.Entity;
        }

        /// <inheritdoc />
        protected override LuaScriptToken Initialize(string data)
        {
            this.ScriptName = data ?? string.Empty;
            return this;
        }
    }
}