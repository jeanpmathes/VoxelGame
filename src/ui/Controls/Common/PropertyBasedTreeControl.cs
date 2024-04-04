// <copyright file="PropertyBasedTreeControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net;
using Gwen.Net.Control;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A <see cref="TreeControl" /> that is built from a <see cref="Property" />.
/// </summary>
public class PropertyBasedTreeControl : TreeControl
{
    private readonly Context context;

    /// <summary>
    ///     Create a <see cref="PropertyBasedTreeControl" /> from a <see cref="Property" />.
    /// </summary>
    internal PropertyBasedTreeControl(ControlBase parent, Property property, Context context) : base(parent)
    {
        this.context = context;

        TreeControlBuilder builder = new(this, context);
        builder.Visit(property);
    }

    /// <summary>
    ///     Update the <see cref="PropertyBasedTreeControl" /> from a <see cref="Property" />.
    /// </summary>
    /// <param name="property">The <see cref="Property" /> to update the <see cref="PropertyBasedTreeControl" /> with.</param>
    public void Update(Property property)
    {
        TreeControlBuilder builder = new(this, context);
        builder.Visit(property);
    }

    private sealed class TreeControlBuilder(TreeControl tree, Context context) : Visitor
    {
        private TreeNode current = tree.RootNode;
        private Int32 index;

        private Boolean top = true;

        private TreeNode? FindNode(String name)
        {
            while (current.NodeCount > index)
            {
                ControlBase target = current.Children[index];

                if (target is TreeNode node && node.Name == name)
                    return node;

                current.RemoveChild(target, dispose: true);
            }

            return null;
        }

        private TreeNode FindOrCreateNode(String name, String text)
        {
            TreeNode? node = FindNode(name);

            index++;

            if (node != null)
            {
                if (node.Text != text)
                    node.Text = text;
            }
            else
            {
                node = current.AddNode(text, name);
            }

            return node;
        }

        public override void Visit(Group group)
        {
            TreeNode previous = current;
            current = FindOrCreateNode(group.Name, group.Name);

            if (top)
            {
                current.Open();
                top = false;
            }

            Int32 previousIndex = index;
            index = 0;

            base.Visit(group);

            while (current.NodeCount > index)
            {
                ControlBase target = current.Children[index];
                current.RemoveChild(target, dispose: true);
            }

            current = previous;
            index = previousIndex;
        }

        public override void Visit(Error error)
        {
            TreeNode node = FindOrCreateNode(error.Name, $"{error.Name}: {error.Message}");

            String icon = error.IsCritical ? context.Resources.ErrorIcon : context.Resources.WarningIcon;
            Color color = error.IsCritical ? Colors.Error : Colors.Warning;

            node.SetImage(icon, Context.SmallIconSize, color);
        }

        public override void Visit(Message message)
        {
            FindOrCreateNode(message.Name, $"{message.Name}: {message.Text}");
        }

        public override void Visit(Integer integer)
        {
            FindOrCreateNode(integer.Name, $"{integer.Name}: {integer.Value}");
        }

        public override void Visit(FileSystemPath path)
        {
            FindOrCreateNode(path.Name, $"{path.Name}: {path.Path.FullName}");
        }

        public override void Visit(Measure measure)
        {
            FindOrCreateNode(measure.Name, $"{measure.Name}: {measure.Value}");
        }
    }
}
