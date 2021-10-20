// <copyright file="SettingsMenu.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.Control;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI.Controls
{
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    public class SettingsMenu : VoxelMenu
    {
        public SettingsMenu(ControlBase parent, FontHolder fonts) : base(parent, fonts) {}

        protected override void CreateMenu(ControlBase menu)
        {
            Button back = new(menu)
            {
                Text = Language.Back
            };

            back.Clicked += (_, _) => Cancel?.Invoke();
        }

        protected override void CreateDisplay(ControlBase display) {}

        public event Action? Cancel;
    }
}
