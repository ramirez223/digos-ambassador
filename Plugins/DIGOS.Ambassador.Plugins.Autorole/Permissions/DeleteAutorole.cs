//
//  DeleteAutorole.cs
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
using DIGOS.Ambassador.Plugins.Permissions;

namespace DIGOS.Ambassador.Plugins.Autorole.Permissions
{
    /// <summary>
    /// Represents a permission that allows a user to delete autoroles.
    /// </summary>
    public class DeleteAutorole : Permission
    {
        /// <inheritdoc />
        public override Guid UniqueIdentifier { get; } = new Guid("177728EE-4DD6-4F01-8D0A-B29E4ED84501");

        /// <inheritdoc/>
        public override string FriendlyName { get; } = nameof(DeleteAutorole);

        /// <inheritdoc/>
        public override string Description { get; } = "Allows you to delete autoroles.";
    }
}
