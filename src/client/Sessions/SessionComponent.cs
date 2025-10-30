// <copyright file="SessionComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Toolkit.Components;

namespace VoxelGame.Client.Sessions;

/// <summary>
///     Base class for all components used in the <see cref="Session" /> class.
/// </summary>
public partial class SessionComponent(Session subject) : Component<Session>(subject);
