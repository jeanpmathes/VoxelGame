// <copyright file="FollowUp.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.UI.UserInterfaces;

/// <summary>
///     A follow-up action that can be attached to a command.
/// </summary>
/// <param name="Description">The description of the follow-up action.</param>
/// <param name="Action">The action to execute.</param>
public sealed record FollowUp(string Description, Action Action);

