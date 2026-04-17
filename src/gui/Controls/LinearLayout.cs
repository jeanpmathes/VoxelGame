// <copyright file="LinearLayout.cs" company="VoxelGame">
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

using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Bases;
using VoxelGame.GUI.Controls.Templates;

namespace VoxelGame.GUI.Controls;

/// <summary>
///     A linear layout arranges its children in a single line, either horizontally or vertically.
/// </summary>
/// <seealso cref="Visuals.LinearLayout" />
public class LinearLayout : LinearLayoutBase<LinearLayout>
{
    /// <inheritdoc />
    protected override ControlTemplate<LinearLayout> CreateDefaultTemplate()
    {
        return ControlTemplate.Create<LinearLayout>(control => new Visuals.LinearLayout
        {
            Orientation = {Binding = Binding.To(control.Orientation)}
        });
    }
}
