// <copyright file="ContextTests.cs" company="VoxelGame">
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
using System.Collections;
using System.Collections.Generic;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Tests.Controls;
using Xunit;

namespace VoxelGame.GUI.Tests;

public class ContextTests
{
    [Fact]
    public void Context_GetStyling_ShouldReturnStylesInOrderOfSpecificity()
    {
        Style? style1 = null;
        Style? style2 = null;

        Context context = Context.Create(builder =>
        {
            style1 = builder.AddStyle<Control>("", s => s.Set(c => c.MinimumWidth, value: 10f));
            style2 = builder.AddStyle<MockControl>("", s => s.Set(c => c.MinimumWidth, value: 20f));
        });

        IReadOnlyList<IStyle<MockControl>> styles = context.GetStyling<MockControl>();

        Assert.Equal(expected: 2, styles.Count);
        Assert.Same(style1, styles[0]);
        Assert.Same(style2, styles[1]);
    }

    [Fact]
    public void Context_GetStyling_ShouldUseStylesOfParentContextIfLocalContextIsEmpty()
    {
        Style? style = null;

        Context parentContext = Context.Create(builder =>
        {
            style = builder.AddStyle<MockControl>("", s => s.Set(c => c.MinimumWidth, value: 20f));
        });

        Context childContext = Context.Default;

        Context context = new(childContext, parentContext);
        IReadOnlyList<IStyle<MockControl>> styles = context.GetStyling<MockControl>();

        Assert.Single(styles);
        Assert.Same(style, styles[0]);
    }

    [Fact]
    public void Context_GetStyling_ShouldUseLocalStylingOverContextStylingForSpecificTypes1()
    {
        Style? style2 = null;

        Context parentContext = Context.Create(builder =>
        {
            builder.AddStyle<Control>("", s => s.Set(c => c.MinimumWidth, value: 10f));
            style2 = builder.AddStyle<MockControl>("", s => s.Set(c => c.MinimumWidth, value: 20f));
        });

        Style? style1 = null;

        Context childContext = Context.Create(builder =>
        {
            style1 = builder.AddStyle<Control>("", s => s.Set(c => c.MinimumWidth, value: 30f));
        });

        Context context = new(childContext, parentContext);
        IReadOnlyList<IStyle<MockControl>> styles = context.GetStyling<MockControl>();

        Assert.Equal(expected: 2, styles.Count);
        Assert.Same(style1, styles[0]);
        Assert.Same(style2, styles[1]);
    }

    [Fact]
    public void Context_GetStyling_ShouldUseLocalStylingOverContextStylingForSpecificTypes2()
    {
        Style? style1 = null;

        Context parentContext = Context.Create(builder =>
        {
            style1 = builder.AddStyle<Control>("", s => s.Set(c => c.MinimumWidth, value: 10f));
            builder.AddStyle<MockControl>("", s => s.Set(c => c.MinimumWidth, value: 20f));
        });

        Style? style2 = null;

        Context childContext = Context.Create(builder =>
        {
            style2 = builder.AddStyle<MockControl>("", s => s.Set(c => c.MinimumWidth, value: 30f));
        });

        Context context = new(childContext, parentContext);
        IReadOnlyList<IStyle<MockControl>> styles = context.GetStyling<MockControl>();

        Assert.Equal(expected: 2, styles.Count);
        Assert.Same(style1, styles[0]);
        Assert.Same(style2, styles[1]);
    }

    [Fact]
    public void Context_GetStyling_ShouldReturnStylingForDirectInterfacesOfType()
    {
        Style? style = null;

        Context context = Context.Create(builder =>
        {
            style = builder.AddStyle<IButton>("", s => s.Set(c => c.MinimumWidth, value: 15f));
        });

        IReadOnlyList<IStyle<Button<String>>> styles = context.GetStyling<Button<String>>();

        Assert.Single((IEnumerable) styles);
        Assert.Same(style, styles[0]);
    }

    [Fact]
    public void Context_GetStyling_ShouldReturnStylingForInheritedInterfacesOfType()
    {
        Style? style = null;

        Context context = Context.Create(builder =>
        {
            style = builder.AddStyle<IContentControl>("", s => s.Set(c => c.MinimumWidth, value: 15f));
        });

        IReadOnlyList<IStyle<Button<String>>> styles = context.GetStyling<Button<String>>();

        Assert.Single((IEnumerable) styles);
        Assert.Same(style, styles[0]);
    }

    [Fact]
    public void Context_GetStyling_ShouldOrderInterfaceStylingBeforeRespectiveConcreteStyling()
    {
        Style style1 = null!;
        Style style2 = null!;
        Style style3 = null!;

        Context context = Context.Create(builder =>
        {
            style2 = builder.AddStyle<Control>("", s => s.Set(c => c.MinimumWidth, value: 20f));
            style3 = builder.AddStyle<IButton>("", s => s.Set(c => c.MinimumWidth, value: 30f));
            style1 = builder.AddStyle<IControl>("", s => s.Set(c => c.MinimumWidth, value: 10f));
        });

        IReadOnlyList<IStyle<Button<String>>> styles = context.GetStyling<Button<String>>();

        Assert.Equal(expected: 3, styles.Count);
        Assert.Same(style1, styles[0]);
        Assert.Same(style2, styles[1]);
        Assert.Same(style3, styles[2]);
    }

    [Fact]
    public void Context_GetContentTemplate_ShouldReturnTemplateOfContext()
    {
        ContentTemplate<String>? template = null;

        Context context = Context.Create(builder =>
        {
            template = builder.AddContentTemplate<String>("", _ => new MockControl());
        });

        IContentTemplate<String> result = context.GetContentTemplate<String>();

        Assert.Same(template, result);
    }
}
