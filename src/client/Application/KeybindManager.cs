// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input;
using VoxelGame.Input.Actions;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager
    {
        private readonly InputManager input;

        public KeyboardState Keyboard => input.CurrentKeyboardState;
        public MouseState Mouse => input.CurrentMouseState;

        public KeybindManager(InputManager input)
        {
            this.input = input;
        }

        private readonly Dictionary<string, Toggle> toggles = new Dictionary<string, Toggle>();

        public Toggle GetToggle(string id, Key key)
        {
            if (toggles.TryGetValue(id, out Toggle? toggle))
            {
                toggle.Clear();
                return toggle;
            }

            toggle = new Toggle(key, input);
            toggles[id] = toggle;

            return toggle;
        }

        public Toggle GetToggle(string id, MouseButton button)
        {
            if (toggles.TryGetValue(id, out Toggle? toggle))
            {
                toggle.Clear();
                return toggle;
            }

            toggle = new Toggle(button, input);
            toggles[id] = toggle;

            return toggle;
        }
    }
}