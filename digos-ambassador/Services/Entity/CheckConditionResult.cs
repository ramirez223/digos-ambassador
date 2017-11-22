﻿//
//  CheckConditionResult.cs
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

using Discord.Commands;
using JetBrains.Annotations;

namespace DIGOS.Ambassador.Services
{
	/// <summary>
	/// Represents a condition check with an accompanying failure reason.
	/// </summary>
	public struct CheckConditionResult : IResult
	{
		/// <inheritdoc />
		public CommandError? Error { get; }

		/// <inheritdoc />
		public string ErrorReason { get; }

		/// <inheritdoc />
		public bool IsSuccess => !this.Error.HasValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckConditionResult"/> struct.
		/// </summary>
		/// <param name="error">The error (if any).</param>
		/// <param name="errorReason">A more detailed error description.</param>
		public CheckConditionResult([CanBeNull] CommandError? error, [CanBeNull] string errorReason)
		{
			this.Error = error;
			this.ErrorReason = errorReason;
		}

		/// <summary>
		/// Creates a new successful result.
		/// </summary>
		/// <returns>A successful result.</returns>
		public static CheckConditionResult FromSuccess()
		{
			return new CheckConditionResult(null, null);
		}

		/// <summary>
		/// Creates a failed result.
		/// </summary>
		/// <param name="error">The error that caused the failure.</param>
		/// <param name="reason">A more detailed error reason.</param>
		/// <returns>A failed result.</returns>
		public static CheckConditionResult FromError(CommandError error, [NotNull] string reason)
		{
			return new CheckConditionResult(error, reason);
		}

		/// <summary>
		/// Creates a failed result based on another result.
		/// </summary>
		/// <param name="result">The result to base this result off of.</param>
		/// <returns>A failed result.</returns>
		public static CheckConditionResult FromError([NotNull] IResult result)
		{
			return new CheckConditionResult(result.Error, result.ErrorReason);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return this.IsSuccess ? "Success" : $"{this.Error}: {this.ErrorReason}";
		}
	}
}
