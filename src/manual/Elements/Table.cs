// <copyright file="Table.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A table element with a configurable column specification.
/// </summary>
public class Table(String columnsSpec) : Chainable, IElement
{
    private readonly List<IElement> others = [];
    private readonly List<TableRow> rows = [];

    void IElement.Generate(StreamWriter writer)
    {
        writer.WriteLine(@$"\begin{{tabular}}{{{columnsSpec}}}");

        foreach (TableRow row in rows)
        {
            row.Generate(writer);
            writer.WriteLine(@"\\");
        }

        writer.WriteLine(@"\end{tabular}");

        foreach (IElement element in others)
            element.Generate(writer);
    }

    internal override void AddElement(IElement element)
    {
        others.Add(element);
    }

    /// <summary>
    ///     Add a new row to the table.
    /// </summary>
    public Table Row(Action<TableRow> builder)
    {
        TableRow row = new();
        rows.Add(row);
        builder(row);

        return this;
    }

    /// <summary>
    ///     Represents a row in a table.
    /// </summary>
    public class TableRow : Chainable
    {
        private readonly List<TableCell> cells = [];

        internal override void AddElement(IElement element)
        {
            if (element is TableCell cell) cells.Add(cell);
        }

        /// <summary>
        ///     Add a cell to the row.
        /// </summary>
        public TableRow Cell(Action<TableCell> builder)
        {
            TableCell cell = new();
            builder(cell);
            cells.Add(cell);

            return this;
        }

        internal void Generate(StreamWriter writer)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (i > 0) writer.Write(" & ");
                cells[i].Generate(writer);
            }
        }
    }

    /// <summary>
    ///     Represents a cell in a table row.
    /// </summary>
    public class TableCell : Chainable, IElement
    {
        private readonly List<IElement> elements = [];

        /// <summary>
        ///     Generate the LaTeX code for the cell.
        /// </summary>
        public void Generate(StreamWriter writer)
        {
            foreach (IElement element in elements) element.Generate(writer);
        }

        internal override void AddElement(IElement element)
        {
            elements.Add(element);
        }
    }
}
