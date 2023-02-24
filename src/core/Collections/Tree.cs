// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections;
using System.Collections.Generic;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Represents a tree structure that can contain any type of value.
/// </summary>
/// <typeparam name="T">The type of the tree's values.</typeparam>
public class Tree<T>
{
    private readonly Node root;

    /// <summary>
    ///     Creates a new tree with the given root value.
    /// </summary>
    /// <param name="rootValue">The root value.</param>
    public Tree(T rootValue)
    {
        root = new Node(rootValue, Parent: null, new List<Node>());
    }

    /// <summary>
    ///     Gets the root node of the tree.
    /// </summary>
    public INode Root => root;

    /// <summary>
    ///     The node interface.
    /// </summary>
    public interface INode : IEnumerable<INode>
    {
        /// <summary>
        ///     Get the value of the node.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Gets the parent of the node.
        /// </summary>
        public INode? Parent { get; }

        /// <summary>
        ///     Adds a child to the node.
        /// </summary>
        /// <param name="value">The value of the child.</param>
        /// <returns>The child node.</returns>
        public INode AddChild(T value);
    }

    /// <summary>
    ///     The node container type.
    /// </summary>
    private sealed record Node(T Value, INode? Parent, List<Node> ChildNodes) : INode
    {
        public IEnumerator<INode> GetEnumerator()
        {
            return ChildNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public INode AddChild(T value)
        {
            Node child = new(value, this, new List<Node>());
            ChildNodes.Add(child);

            return child;
        }
    }
}
