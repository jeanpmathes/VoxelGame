// <copyright file="UnionFind.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Collections;

/// <summary>
///     A simple short-based union find structure.
/// </summary>
public class UnionFind
{
    private readonly short[] id;
    private readonly short[] size;

    /// <summary>
    ///     Creates a new union find structure with the given number of elements.
    /// </summary>
    /// <param name="n">The number of elements.</param>
    public UnionFind(short n)
    {
        Count = n;

        id = new short[n];
        size = new short[n];

        for (short i = 0; i < n; i++)
        {
            id[i] = i;
            size[i] = 1;
        }
    }

    /// <summary>
    ///     Gets the number of elements in the union find structure.
    /// </summary>
    public short Count { get; private set; }

    /// <summary>
    ///     Finds the root of the given element.
    /// </summary>
    /// <param name="p">The element to find the root of.</param>
    /// <returns>The root of the given element.</returns>
    public short Find(short p)
    {
        short root = p;

        while (root != id[root]) root = id[root];

        while (p != root)
        {
            short next = id[p];
            id[p] = root;
            p = next;
        }

        return root;
    }

    /// <summary>
    ///     Unions the two given elements.
    /// </summary>
    /// <param name="p">The first element.</param>
    /// <param name="q">The second element.</param>
    /// <returns>The root of the new union.</returns>
    public short Union(short p, short q)
    {
        short i = Find(p);
        short j = Find(q);

        if (i == j) return i;

        short root;

        if (size[i] < size[j])
        {
            id[i] = j;
            size[j] += size[i];

            root = j;
        }
        else
        {
            id[j] = i;
            size[i] += size[j];

            root = i;
        }

        Count--;

        return root;
    }

    /// <summary>
    ///     Checks if the two given elements are in the same union.
    /// </summary>
    /// <param name="p">The first element. </param>
    /// <param name="q">The second element. </param>
    /// <returns>True if the two given elements are in the same union.</returns>
    public bool Connected(short p, short q)
    {
        return Find(p) == Find(q);
    }

    /// <summary>
    ///     Gets the size of the given element's union.
    /// </summary>
    /// <param name="p">The element to get the size of.</param>
    /// <returns>The size of the given element's union.</returns>
    public int GetSize(short p)
    {
        return size[Find(p)];
    }
}
