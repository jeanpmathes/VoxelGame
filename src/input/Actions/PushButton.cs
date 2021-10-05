﻿// <copyright file="PushButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class PushButton : Button
    {
        private bool hasReleased;
        private bool pushed;

        public bool Pushed
        {
            get => pushed;
            private set
            {
                pushed = value;
                IsDown = value;
            }
        }

        public PushButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input) {}

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;

            Pushed = false;

            if (hasReleased && state.IsKeyOrButtonDown(KeyOrButton))
            {
                hasReleased = false;
                Pushed = true;
            }
            else if (state.IsKeyOrButtonUp(KeyOrButton))
            {
                hasReleased = true;
            }
        }
    }
}