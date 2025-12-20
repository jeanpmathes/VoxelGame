// <copyright file="MockBlock.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Tests.Logic.Elements;

public class MockBlock : Block
{
    public MockBlock() : base(blockID: 0, new CID(nameof(MockBlock)), "Mock Block")
    {
        States = new StateSet(this, setOffset: 0, stateCount: 1, placementDefault: 0, generationDefault: 0, []);

        DefineEvents(new MockEventRegistry());
    }

    public override Meshable Meshable => Meshable.Unmeshed;

    protected override void OnValidate(IValidator validator) {}

    protected override void BuildMeshes(ITextureIndexProvider textureIndexProvider, IModelProvider modelProvider, VisualConfiguration visuals, IValidator validator) {}

    public override void Mesh(Vector3i position, State state, MeshingContext context) {}

    public override ColorS GetDominantColor(State state, ColorS positionTint)
    {
        return ColorS.White;
    }

    private sealed class MockEventRegistry : IEventRegistry
    {
        public IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single)
        {
            return new MockEvent<TEventMessage>();
        }

        private sealed class MockEvent<TEventMessage> : IEvent<TEventMessage>
        {
            public Boolean HasSubscribers => false;

            public void Publish(TEventMessage message) {}
        }
    }
}
