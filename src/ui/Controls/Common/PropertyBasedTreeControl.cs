// <copyright file="PropertyBasedTreeControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    /// <summary>
    ///     Create a <see cref="PropertyBasedTreeControl" /> from a <see cref="Property" />.
    /// </summary>
    internal PropertyBasedTreeControl(ControlBase parent, Property property, Context context) : base(parent)
    {
        TreeControlBuilder builder = new(this, context);
        builder.Visit(property);
    }

    private sealed class TreeControlBuilder : Visitor
    {
        private readonly Context context;

        private TreeNode current;

        public TreeControlBuilder(TreeControl tree, Context context)
        {
            current = tree.RootNode;
            this.context = context;
        }

        public override void Visit(Group group)
        {
            TreeNode previous = current;
            current = current.AddNode(group.Name);

            base.Visit(group);

            current = previous;
        }

        public override void Visit(Error error)
        {
            TreeNode node = current.AddNode($"{error.Name}: {error.Message}");

            string icon = error.IsCritical ? context.Resources.ErrorIcon : context.Resources.WarningIcon;
            Color color = error.IsCritical ? Colors.Error : Colors.Warning;

            node.SetImage(icon, Context.SmallIconSize, color);
        }
    }
}
