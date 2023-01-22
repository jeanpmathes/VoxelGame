// <copyright file="EmptyControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Control;

namespace VoxelGame.UI.Controls;

/// <summary>
///     An empty control.
/// </summary>
public class EmptyControl : ControlBase
{
    /// <summary>
    ///     Create an empty control.
    /// </summary>
    public EmptyControl(ControlBase parent) : base(parent) {}
}

