// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Logging;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<KeybindManager>();

        private readonly InputManager input;

        public KeyboardState Keyboard => input.CurrentKeyboardState;
        public MouseState Mouse => input.CurrentMouseState;

        public KeybindManager(InputManager input)
        {
            this.input = input;
        }

        private readonly Dictionary<string, InputAction> keybinds = new Dictionary<string, InputAction>();

        private void AddKeybind(string id, InputAction action)
        {
            if (keybinds.ContainsKey(id))
            {
                Debug.Fail($"The id '{id}' is already in use for a keybind.");
            }

            keybinds[id] = action;

            Logger.LogDebug($"Created keybind: {id}");
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

            AddKeybind(id, toggle);

            return toggle;
        }

        private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();

        public Button GetButton(string id, Key key)
        {
            if (buttons.TryGetValue(id, out Button? button))
            {
                return button;
            }

            button = new Button(key, input);
            buttons[id] = button;

            AddKeybind(id, button);

            return button;
        }

        public Button GetButton(string id, MouseButton key)
        {
            if (buttons.TryGetValue(id, out Button? button))
            {
                return button;
            }

            button = new Button(key, input);
            buttons[id] = button;

            AddKeybind(id, button);

            return button;
        }

        private readonly Dictionary<string, PushButton> pushButtons = new Dictionary<string, PushButton>();

        public PushButton GetPushButton(string id, Key key)
        {
            if (pushButtons.TryGetValue(id, out PushButton? button))
            {
                return button;
            }

            button = new PushButton(key, input);
            pushButtons[id] = button;

            AddKeybind(id, button);

            return button;
        }
    }
}