// <copyright file="QualitySetting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.Core.Visuals;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI.Settings
{
    /// <summary>
    ///     Settings that allow to pick a quality level.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class QualitySetting : Setting
    {
        private readonly Func<Quality> get;

        private readonly MenuItem[] items = new MenuItem[Qualities.Count];
        private readonly Action<Quality> set;

        public QualitySetting(string name, Func<Quality> get, Action<Quality> set)
        {
            this.get = get;
            this.set = set;

            Name = name;
        }

        protected override string Name { get; }

        private protected override void FillControl(ControlBase control, Context context)
        {
            ComboBox qualitySelection = new(control);

            foreach (Quality quality in Qualities.All())
                items[(int) quality] = qualitySelection.AddItem(quality.Name(), "", quality);

            qualitySelection.SelectedItem = items[(int) get()];

            qualitySelection.ItemSelected += (_, args) => { set((Quality) ((MenuItem) args.SelectedItem).UserData); };
        }
    }
}