// <copyright file="Context.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Client.Entities;

namespace VoxelGame.Client.Console;

/// <summary>
///     The command execution context.
/// </summary>
/// <param name="Console">The console in which the command is running.</param>
/// <param name="Player">The player that executed the command.</param>
public record CommandContext(ConsoleWrapper Console, ClientPlayer Player);
