// <copyright file="SectionGrid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Wraps a section and allows to access it as a grid.
/// </summary>
public class SectionGrid : IGrid
{
    private readonly Section section;
    private readonly SectionPosition sectionPosition;

    /// <summary>
    ///     Creates a new grid for the given section.
    /// </summary>
    /// <param name="section">The section to wrap.</param>
    /// <param name="sectionPosition">The position of the section.</param>
    public SectionGrid(Section section, SectionPosition sectionPosition)
    {
        this.section = section;
        this.sectionPosition = sectionPosition;
    }

    /// <inheritdoc />
    public Content? GetContent(Vector3i position)
    {
        if (!sectionPosition.Contains(position)) return null;

        (Int32 x, Int32 y, Int32 z) localPosition = Section.ToLocalPosition(position);

        if (!Section.IsInBounds(localPosition)) return null;

        UInt32 val = section.GetContent(localPosition.x, localPosition.y, localPosition.z);
        Section.Decode(val, out Content content);

        return content;
    }

    /// <inheritdoc />
    public void SetContent(Content content, Vector3i position)
    {
        if (!sectionPosition.Contains(position)) return;

        (Int32 x, Int32 y, Int32 z) localPosition = Section.ToLocalPosition(position);

        if (!Section.IsInBounds(localPosition)) return;

        section.SetContent(localPosition.x, localPosition.y, localPosition.z, Section.Encode(content));
    }
}
