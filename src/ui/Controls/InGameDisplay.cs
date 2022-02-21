// <copyright file="InGameDisplay.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using VoxelGame.Core.Logic;
using VoxelGame.UI.Providers;

namespace VoxelGame.UI.Controls
{
    /// <summary>
    ///     A display that is shown while playing the game, as a form of HUD.
    /// </summary>
    [SuppressMessage("ReSharper", "CA2000", Justification = "Controls are disposed by their parent.")]
    [SuppressMessage("ReSharper", "UnusedVariable", Justification = "Controls are used by their parent.")]
    internal class InGameDisplay : ControlBase
    {
        private readonly ControlBase debugViewContainer;

        private readonly Label headPosition;
        private readonly Label performance;
        private readonly Label playerSelection;
        private readonly Label targetBlock;
        private readonly Label targetLiquid;
        private readonly Label targetPosition;

        private bool debugMode;

        internal InGameDisplay(ControlBase parent) : base(parent)
        {
            Dock = Dock.Fill;

            DockLayout top = new(this)
            {
                Dock = Dock.Top,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = Margin.Ten
            };

            playerSelection = new Label(top)
            {
                Text = "Block: _____",
                Dock = Dock.Left
            };

            VerticalLayout right = new(top)
            {
                Dock = Dock.Right,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            performance = new Label(right)
            {
                Text = "FPS/UPS: 000/000",
                Alignment = Alignment.Right
            };

            debugViewContainer = new VerticalLayout(right)
            {
                Dock = Dock.Right,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            headPosition = new Label(debugViewContainer) { Alignment = Alignment.Right };
            targetPosition = new Label(debugViewContainer) { Alignment = Alignment.Right };
            targetBlock = new Label(debugViewContainer) { Alignment = Alignment.Right };
            targetLiquid = new Label(debugViewContainer) { Alignment = Alignment.Right };

            debugViewContainer.Hide();
        }

        internal void SetUpdateRate(double fps, double ups)
        {
            performance.Text = $"FPS/UPS: {fps:000}/{ups:000}";
        }

        internal void SetPlayerData(IPlayerDataProvider playerDataProvider)
        {
            playerSelection.Text = $"{playerDataProvider.Mode}: {playerDataProvider.Selection}";
        }

        public void SetPlayerDebugData(IPlayerDataProvider playerDataProvider)
        {
            if (!debugMode) return;

            headPosition.Text = $"Head: {playerDataProvider.HeadPosition}";
            targetPosition.Text = $"Target: {playerDataProvider.TargetPosition}";

            (var block, uint data) = playerDataProvider.TargetBlock;

            targetBlock.Text =
                $"B: {block.NamedId}[{block.Id}], {Convert.ToString(data, toBase: 2).PadLeft(totalWidth: 6, paddingChar: '0')}";

            (var liquid, LiquidLevel level, bool isStatic) = playerDataProvider.TargetLiquid;
            targetLiquid.Text = $"L: {liquid.NamedId}[{liquid.Id}], {level}, {isStatic}";
        }

        public void ToggleDebugDataView()
        {
            debugMode = !debugMode;
            debugViewContainer.IsHidden = !debugMode;
        }
    }
}
