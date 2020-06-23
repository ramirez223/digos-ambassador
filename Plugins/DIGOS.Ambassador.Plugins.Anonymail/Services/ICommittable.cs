//
//  ICommittable.cs
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

using System.Threading;
using System.Threading.Tasks;

namespace DIGOS.Ambassador.Plugins.Anonymail.Services
{
    /// <summary>
    /// Represents an entity with transient state that must be committed, or the state changes will be discarded at the
    /// end of a logical scope.
    /// </summary>
    public interface ICommittable
    {
        /// <summary>
        /// Commits the changes in the entity, persisting them.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CommitAsync(CancellationToken ct = default);
    }
}
