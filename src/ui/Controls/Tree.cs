// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net.Control;
using VoxelGame.Core.Collections;

namespace VoxelGame.UI.Controls;

using StringTree = Tree<string>;
using StringNode = Tree<string>.INode;

/// <summary>
///     Displays the content of a string tree structure.
/// </summary>
public class Tree : ControlBase
{
    private readonly TreeControl control;

    /// <summary>
    ///     Creates a new tree control.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    public Tree(ControlBase parent) : base(parent)
    {
        control = new TreeControl(this);
    }

    /// <summary>
    ///     Updates the tree control with the given tree.
    /// </summary>
    /// <param name="tree">The tree to display.</param>
    public void SetContent(StringTree tree)
    {
        control.RemoveAllNodes();

        TreeNode root = control.AddNode(tree.Root.Value);

        void AddChildren(TreeNode parent, StringNode node)
        {
            foreach (StringNode child in node)
            {
                TreeNode childNode = parent.AddNode(child.Value);
                AddChildren(childNode, child);
            }
        }

        AddChildren(root, tree.Root);

        control.ExpandAll();
    }
}
