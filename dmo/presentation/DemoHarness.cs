// <copyright file="DemoHarness.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Linq;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Presentation.Demo;

/// <summary>
///     Hosts all the different showcases.
/// </summary>
public class DemoHarness
{
    private readonly Showcase[] showcases;

    private Int32 currentShowcaseIndex;

    /// <summary>
    ///     Create a new demo harness, which collects all available showcases.
    /// </summary>
    public DemoHarness()
    {
        showcases = [.. Reflections.GetSubclassInstances<Showcase>()];
    }

    /// <summary>
    ///     The currently calculated rendering cycle frequency.
    /// </summary>
    public Slot<Double> RenderFrequency => field ??= new Slot<Double>(value: 0.0, this);

    /// <summary>
    ///     The currently calculated update cycle frequency.
    /// </summary>
    public Slot<Double> UpdateFrequency => field ??= new Slot<Double>(value: 0.0, this);

    /// <summary>
    ///     The currently selected showcase.
    /// </summary>
    public Slot<Showcase?> CurrentShowcase => field ??= new Slot<Showcase?>(value: null, this);

    /// <summary>
    ///     Get the templates of all showcases.
    /// </summary>
    /// <returns>The templates of all showcases.</returns>
    public IEnumerable<ContentTemplate> GetShowcaseTemplates()
    {
        return showcases.Select(showcase => showcase.GetTemplate());
    }

    /// <summary>
    ///     Move <see cref="CurrentShowcase" /> to the next showcase.
    /// </summary>
    public void MoveToNextShowcase()
    {
        if (showcases.Length == 0) return;

        currentShowcaseIndex += 1;

        if (currentShowcaseIndex >= showcases.Length)
            currentShowcaseIndex = 0;

        CurrentShowcase.SetValue(showcases[currentShowcaseIndex]);
    }

    /// <summary>
    ///     Move <see cref="CurrentShowcase" /> to the previous showcase.
    /// </summary>
    public void MoveToPreviousShowcase()
    {
        if (showcases.Length == 0) return;

        currentShowcaseIndex -= 1;

        if (currentShowcaseIndex < 0)
            currentShowcaseIndex = showcases.Length - 1;

        CurrentShowcase.SetValue(showcases[currentShowcaseIndex]);
    }

    /// <summary>
    ///     Write and display a message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void Write(String message)
    {
        Console.WriteLine(message);
    }
}
