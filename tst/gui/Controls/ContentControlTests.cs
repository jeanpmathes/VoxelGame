// <copyright file="ContentControlTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
using Xunit;

namespace VoxelGame.GUI.Tests.Controls;

[TestSubject(typeof(ContentControl<>))]
public sealed class ContentControlTests() : ControlTestBase<ContentControl<Object>>(() => new ContentControl<Object>()), IDisposable
{
    private readonly Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());
    private readonly ContentTemplate<String> template = ContentTemplate.Create<String>("", c => new MockControl(c));

    public void Dispose()
    {
        canvas.Dispose();
        template.Dispose();
    }

    [Fact]
    public void ContentControl_Content_ShouldNotHaveChildrenWithNoContent()
    {
        ContentControl<Object> control = new();
        canvas.Child = control;

        Assert.Empty(control.Children);
    }

    [Fact]
    public void ContentControl_Content_ShouldHaveChildBasedOnContentTemplate()
    {
        const String content = "Content";

        ContentControl<String> control = new()
        {
            Content = {Value = content},
            ContentTemplate = {Value = template}
        };

        canvas.Child = control;

        Assert.Single(control.Children);
        MockControl child = Assert.IsType<MockControl>(control.Children[0]);

        Assert.Equal(content, child.Tag);
    }

    [Fact]
    public void ContentControl_ContentTemplate_ShouldUseContextTemplateWhenNoLocalTemplateExists()
    {
        const String content = "Content";

        ThemeBuilder builder = new();
        builder.AddContentTemplate<String>("", c => new MockControl(c));

        using Canvas localCanvas = Canvas.Create(new MockRenderer(), builder.BuildTheme());

        ContentControl<String> control = new()
        {
            Content = {Value = content}
        };

        localCanvas.Child = control;

        Assert.Single(control.Children);
        MockControl child = Assert.IsType<MockControl>(control.Children[0]);

        Assert.Equal(content, child.Tag);
    }

    [Fact]
    public void ContentControl_ContentTemplate_ShouldUseLocalTemplateOverContextTemplate()
    {
        const String content = "Content";

        ThemeBuilder builder = new();
        builder.AddContentTemplate<String>("", _ => new MockControl("Context"));

        using Canvas localCanvas = Canvas.Create(new MockRenderer(), builder.BuildTheme());

        ContentControl<String> control = new()
        {
            Content = {Value = content},
            ContentTemplate = {Value = template}
        };

        localCanvas.Child = control;

        Assert.Single(control.Children);
        MockControl child = Assert.IsType<MockControl>(control.Children[0]);

        Assert.Equal(content, child.Tag);
    }

    [Fact]
    public void ContentControl_Content_ShouldUpdateChildWhenContentChanges()
    {
        const String firstContent = "First Content";
        const String secondContent = "Second Content";

        ContentControl<String> control = new()
        {
            ContentTemplate = {Value = template}
        };

        canvas.Child = control;

        control.Content.Value = firstContent;

        Assert.Single(control.Children);
        MockControl mockControl = Assert.IsType<MockControl>(control.Children[0]);
        Assert.Equal(firstContent, mockControl.Tag);

        control.Content.Value = secondContent;

        Assert.Single(control.Children);
        mockControl = Assert.IsType<MockControl>(control.Children[0]);
        Assert.Equal(secondContent, mockControl.Tag);

        control.Content.Value = null;

        Assert.Empty(control.Children);
    }

    [Fact]
    public void ContentControl_ContentTemplate_ShouldUseTemplateContainingTextForStringsByDefault()
    {
        const String content = "Content";

        ContentControl<String> control = new()
        {
            Content = {Value = content}
        };

        canvas.Child = control;

        Assert.Single(control.Children);
        Text text = Assert.IsType<Text>(control.Children[0]);
        Assert.Equal(content, text.Content.GetValue());
    }

    [Fact]
    public void ContentControl_ContentTemplate_ShouldUseTemplateContainingTextFromToStringForAnyObjectsByDefault()
    {
        Object content = 123;

        ContentControl<Object> control = new()
        {
            Content = {Value = content}
        };

        canvas.Child = control;

        Assert.Single(control.Children);
        Text text = Assert.IsType<Text>(control.Children[0]);
        Assert.Equal(content.ToString(), text.Content.GetValue());
    }

    [Fact]
    public void ContentControl_ContentTemplate_ShouldUpdateChildWhenTemplateChanges()
    {
        const String firstContent = "First Content";
        const String secondContent = "Second Content";

        ContentControl<String> control = new()
        {
            Content = {Value = "Test Content"}
        };

        canvas.Child = control;

        control.ContentTemplate.Value = ContentTemplate.Create<String>("", _ => new MockControl(firstContent));

        Assert.Single(control.Children);
        MockControl mockControl = Assert.IsType<MockControl>(control.Children[0]);
        Assert.Equal(firstContent, mockControl.Tag);

        control.ContentTemplate.Value = ContentTemplate.Create<String>("", _ => new MockControl(secondContent));

        Assert.Single(control.Children);
        mockControl = Assert.IsType<MockControl>(control.Children[0]);
        Assert.Equal(secondContent, mockControl.Tag);
    }
}
