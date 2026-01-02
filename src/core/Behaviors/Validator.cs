// <copyright file="Validator.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors;

/// <summary>
///     Tracks the validation of a behavior system.
/// </summary>
public interface IValidator
{
    /// <summary>
    ///     Report a warning during validation.
    ///     Will not abort creation and not fail the validation.
    /// </summary>
    void ReportWarning(String message);

    /// <summary>
    ///     Report an error during validation.
    ///     This might abort further creation of the system.
    /// </summary>
    void ReportError(String message);
}

/// <summary>
///     Implements <see cref="IValidator" />.
/// </summary>
public class Validator(IResourceContext context) : IValidator
{
    private IIssueSource? source;
    private String sourceInfo = "";

    /// <summary>
    ///     Whether there is at least one error reported.
    /// </summary>
    public Boolean HasError { get; private set; }

    /// <inheritdoc />
    public void ReportWarning(String message)
    {
        Debug.Assert(source != null);

        context.ReportWarning(source, $"{sourceInfo} {message}");
    }

    /// <inheritdoc />
    public void ReportError(String message)
    {
        Debug.Assert(source != null);

        context.ReportError(source, $"{sourceInfo} {message}");

        HasError = true;
    }

    /// <summary>
    ///     Set the current scope of the validator.
    /// </summary>
    public void SetScope(IHasBehaviors behaviorContainer)
    {
        source = behaviorContainer;
        sourceInfo = $"[in {behaviorContainer}]";
    }

    /// <summary>
    ///     Set the current scope of the validator.
    /// </summary>
    public void SetScope(IBehavior behavior)
    {
        source = behavior;
        sourceInfo = $"[in {behavior.Subject}]";
    }
}
