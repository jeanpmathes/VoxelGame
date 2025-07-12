// <copyright file="PropertyBasedListControl.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;
using Color = VoxelGame.Core.Collections.Properties.Color;

namespace VoxelGame.UI.Controls.Common;

/// <summary>
///     A list-like control that is built from a <see cref="Property" />.
/// </summary>
public class PropertyBasedListControl : ControlBase
{
    /// <summary>
    ///     Create a <see cref="PropertyBasedListControl" /> from a <see cref="Property" />.
    /// </summary>
    internal PropertyBasedListControl(ControlBase parent, Property property, Context context) : base(parent)
    {
        PropertyControlBuilder builder = new(this, context);
        builder.Visit(property);
    }

    private sealed class PropertyControlBuilder : Visitor
    {
        private const Int32 KeyColumn = 0;
        private const Int32 ValueColumn = 1;

        private readonly Stack<(String prefix, Int32 number, VerticalLayout parent, ListBox? list)> stack = new();

        private readonly Context context;

        public PropertyControlBuilder(ControlBase parent, Context context)
        {
            this.context = context;

            stack.Push(("", 0, new VerticalLayout(parent), null));
        }

        public override void Visit(Group group)
        {
            (String prefix, Int32 number, VerticalLayout parent, ListBox? list) current = stack.Pop();

            Int32 number = current.number + 1;
            var prefix = $"{current.prefix}{number}";
            var header = $"{prefix} {group.Name}";

            VerticalLayout layout = new(current.parent);

            Separator separator = new(layout)
            {
                Text = header,
                TextFont = context.Fonts.GetHeader(stack.Count + 1)
            };

            Control.Used(separator);

            stack.Push(current with {number = number, list = null});
            stack.Push(($"{prefix}.", 0, layout, null));

            base.Visit(group);

            stack.Pop();
        }

        public override void Visit(Error error)
        {
            ListBoxRow row = AddRow(error.Name, error.Message);

            row.SetCellFont(ValueColumn, context.Fonts.ConsoleError);
        }

        public override void Visit(Message message)
        {
            AddRow(message.Name, message.Text);
        }

        public override void Visit(Integer integer)
        {
            AddRow(integer.Name, integer.Value.ToString(CultureInfo.InvariantCulture));
        }

        public override void Visit(FileSystemPath path)
        {
            ListBoxRow row = AddRow(path.Name, path.Path.FullName);

            row.SetCellFont(ValueColumn, context.Fonts.Path);
            row.SetCellText(ValueColumn, path.Path.FullName, Alignment.Center);
        }

        public override void Visit(Measure measure)
        {
            AddRow(measure.Name, measure.Value.ToString() ?? "null");
        }

        public override void Visit(Truth truth)
        {
            AddRow(truth.Name, truth.Value ? "True" : "False");
        }

        public override void Visit(Color color)
        {
            var value = color.Value.ToString();

            ListBoxRow row = AddRow(color.Name, value);
            row.SetCellText(ValueColumn, value, Alignment.Center);

            var c = color.Value.ToColor();
            row.SetTextColor(new Gwen.Net.Color(c.A, c.R, c.G, c.B));
        }

        private void EnsureList()
        {
            (String prefix, Int32 number, VerticalLayout parent, ListBox? list) current = stack.Pop();

            if (current.list == null)
            {
                current.list = new ListBox(current.parent)
                {
                    ColumnCount = 2,
                    AlternateColor = true,

                    CanScrollH = false,
                    CanScrollV = false
                };

                current.list.SetColumnWidth(KeyColumn, width: 50);
            }

            stack.Push(current);
        }

        private ListBoxRow AddRow(String key, String value)
        {
            EnsureList();

            (String prefix, Int32 number, VerticalLayout parent, ListBox? list) current = stack.Peek();

            ListBoxRow row = new(current.list!);

            row.SetCellText(KeyColumn, key);
            row.SetCellText(ValueColumn, value);

            row.SetTextColor(Colors.Secondary);

            current.list!.AddRow(row);

            return row;
        }
    }
}
