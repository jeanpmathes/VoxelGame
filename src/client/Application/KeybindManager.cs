// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenToolkit.Windowing.Common.Input;
using Properties;
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Collections;
using VoxelGame.Input.Internal;
using VoxelGame.Logging;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<KeybindManager>();

        private readonly Dictionary<Keybind, Button> keybinds = new();
        private readonly Dictionary<Keybind, PushButton> pushButtons = new();
        private readonly Dictionary<Keybind, SimpleButton> simpleButtons = new();

        private readonly Dictionary<Keybind, ToggleButton> toggleButtons = new();

        private readonly KeyMap usageMap = new();

        public KeybindManager(InputManager input)
        {
            Input = input;

            Keybind.RegisterWithManager(this);

            InitializeSettings();
            InitializeUsages();

            LookBind = new LookInput(Input.Mouse, client.Default.MouseSensitivity);
        }

        public InputManager Input { get; }

        public IEnumerator<Keybind> Binds => keybinds.Keys.GetEnumerator();

        public LookInput LookBind { get; }

        public void Add(Keybind bind, ToggleButton button)
        {
            AddKeybind(bind, button);
            toggleButtons.Add(bind, button);
        }

        public void Add(Keybind bind, SimpleButton button)
        {
            AddKeybind(bind, button);
            simpleButtons.Add(bind, button);
        }

        public void Add(Keybind bind, PushButton button)
        {
            AddKeybind(bind, button);
            pushButtons.Add(bind, button);
        }

        private void AddKeybind(Keybind bind, Button button)
        {
            if (keybinds.ContainsKey(bind)) Debug.Fail($"The keybind '{bind}' is already associated with an action.");

            keybinds[bind] = button;

            logger.LogDebug(Events.SetKeyBind, "Created keybind: {Bind}", bind);
        }

        private void InitializeSettings()
        {
            foreach (KeyValuePair<Keybind, Button> pair in keybinds)
            {
                string key = PropertyName(pair.Key);

                SettingsProperty property = new(key)
                {
                    PropertyType = typeof(KeyButtonPair),
                    IsReadOnly = false,
                    DefaultValue = KeyButtonPair.DefaultValue,
                    Provider = client.Default.Providers["LocalFileSettingsProvider"],
                    SerializeAs = SettingsSerializeAs.Xml
                };

                property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());

                client.Default.Properties.Add(property);
            }

            client.Default.Reload();

            foreach ((Keybind keybind, Button button) in keybinds)
            {
                string key = PropertyName(keybind);
                var settings = (KeyButtonPair) client.Default[key];

                if (settings.Default) client.Default[key] = button.KeyOrButton.Settings;
                else button.SetBinding(new KeyOrButton(settings));
            }

            client.Default.Save();

            logger.LogInformation(Events.InputSystem, "Finished initializing keybind settings");
        }

        private void InitializeUsages()
        {
            foreach (KeyValuePair<Keybind, Button> pair in keybinds) UpdateAddedBind(pair.Value.KeyOrButton);
        }

        public ToggleButton GetToggle(Keybind bind)
        {
            Debug.Assert(toggleButtons.ContainsKey(bind), "No toggle associated with this keybind.");

            return toggleButtons[bind];
        }

        public Button GetButton(Keybind bind)
        {
            Debug.Assert(simpleButtons.ContainsKey(bind), "No simple button associated with this keybind.");

            return simpleButtons[bind];
        }

        public PushButton GetPushButton(Keybind bind)
        {
            Debug.Assert(pushButtons.ContainsKey(bind), "No push button associated with this keybind.");

            return pushButtons[bind];
        }

        public void Rebind(Keybind bind, KeyOrButton keyOrButton)
        {
            Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");

            usageMap.RemoveBinding(keybinds[bind].KeyOrButton);
            keybinds[bind].SetBinding(keyOrButton);

            client.Default[PropertyName(bind)] = keyOrButton.Settings;
            client.Default.Save();

            logger.LogInformation(Events.SetKeyBind, "Rebind '{Bind}' to: {Key}", bind, keyOrButton);

            UpdateAddedBind(keyOrButton);
        }

        private static string PropertyName(Keybind bind)
        {
            return $"Input_{bind}";
        }

        public KeyOrButton GetCurrentBind(Keybind bind)
        {
            Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");

            return keybinds[bind].KeyOrButton;
        }

        private void UpdateAddedBind(KeyOrButton keyOrButton)
        {
            bool unused = usageMap.AddBinding(keyOrButton);

            if (!unused)
                logger.LogWarning(Events.SetKeyBind, "Key '{KeyOrButton}' is used by multiple bindings", keyOrButton);
        }

        #region KEYBINDS

        public Keybind Fullscreen { get; } = Keybind.RegisterToggle("fullscreen", Key.F11);

        public Keybind Wireframe { get; } = Keybind.RegisterToggle("wireframe", Key.K);
        public Keybind UI { get; } = Keybind.RegisterToggle("ui", Key.J);

        public Keybind Screenshot { get; } = Keybind.RegisterPushButton("screenshot", Key.F12);
        public Keybind Escape { get; } = Keybind.RegisterPushButton("escape", Key.Escape);

        public Keybind Forwards { get; } = Keybind.RegisterButton("forwards", Key.W);
        public Keybind Backwards { get; } = Keybind.RegisterButton("backwards", Key.S);
        public Keybind StrafeRight { get; } = Keybind.RegisterButton("strafe_right", Key.D);
        public Keybind StrafeLeft { get; } = Keybind.RegisterButton("strafe_left", Key.A);

        public Keybind Sprint { get; } = Keybind.RegisterButton("sprint", Key.ShiftLeft);
        public Keybind Jump { get; } = Keybind.RegisterButton("jump", Key.Space);

        public Keybind InteractOrPlace { get; } = Keybind.RegisterButton("interact_or_place", MouseButton.Right);
        public Keybind Destroy { get; } = Keybind.RegisterButton("destroy", MouseButton.Left);
        public Keybind BlockInteract { get; } = Keybind.RegisterButton("block_interact", Key.ControlLeft);

        public Keybind PlacementMode { get; } = Keybind.RegisterToggle("placement_mode", Key.R);

        public Keybind NextPlacement { get; } = Keybind.RegisterPushButton("select_next_placement", Key.KeypadPlus);

        public Keybind PreviousPlacement { get; } =
            Keybind.RegisterPushButton("select_previous_placement", Key.KeypadMinus);

        #endregion KEYBINDS
    }
}