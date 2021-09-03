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
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Internal;
using VoxelGame.Logging;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<KeybindManager>();

        public InputManager Input { get; }

        public KeybindManager(InputManager input)
        {
            Input = input;

            Keybind.RegisterWithManager(this);
            InitializeSettings();

            LookBind = new LookInput(Input.Mouse, Properties.client.Default.MouseSensitivity);
        }

        private readonly Dictionary<Keybind, Button> keybinds = new Dictionary<Keybind, Button>();

        private readonly Dictionary<Keybind, ToggleButton> toggleButtons = new Dictionary<Keybind, ToggleButton>();
        private readonly Dictionary<Keybind, SimpleButton> simpleButtons = new Dictionary<Keybind, SimpleButton>();
        private readonly Dictionary<Keybind, PushButton> pushButtons = new Dictionary<Keybind, PushButton>();

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
            if (keybinds.ContainsKey(bind))
            {
                Debug.Fail($"The keybind '{bind}' is already associated with an action.");
            }

            keybinds[bind] = button;

            Logger.LogDebug(Events.SetKeyBind, $"Created keybind: {bind}");
        }

        private void InitializeSettings()
        {
            foreach (KeyValuePair<Keybind, Button> pair in keybinds)
            {
                string key = PropertyName(pair.Key);

                SettingsProperty property = new SettingsProperty(key)
                {
                    PropertyType = typeof(KeyButtonPair),
                    IsReadOnly = false,
                    DefaultValue = KeyButtonPair.DefaultValue,
                    Provider = Properties.client.Default.Providers["LocalFileSettingsProvider"],
                    SerializeAs = SettingsSerializeAs.Xml
                };

                property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());

                Properties.client.Default.Properties.Add(property);
            }

            Properties.client.Default.Reload();

            foreach (KeyValuePair<Keybind, Button> pair in keybinds)
            {
                string key = PropertyName(pair.Key);
                var settings = (KeyButtonPair) Properties.client.Default[key];

                if (settings.Default)
                {
                    Properties.client.Default[key] = pair.Value.KeyOrButton.Settings;
                }
                else
                {
                    pair.Value.SetBinding(new KeyOrButton(settings));
                }
            }

            Properties.client.Default.Save();

            Logger.LogInformation("Finished initializing up keybind settings.");
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
            keybinds[bind].SetBinding(keyOrButton);

            Properties.client.Default[PropertyName(bind)] = keyOrButton.Settings;
            Properties.client.Default.Save();

            Logger.LogInformation(Events.SetKeyBind, $"Rebind '{bind}' to: {keyOrButton}");
        }

        private static string PropertyName(Keybind bind) => $"Input_{bind}";

        public KeyOrButton GetCurrentBind(Keybind bind)
        {
            Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");
            return keybinds[bind].KeyOrButton;
        }

        public IEnumerator<Keybind> Binds => keybinds.Keys.GetEnumerator();

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
        public Keybind PreviousPlacement { get; } = Keybind.RegisterPushButton("select_previous_placement", Key.KeypadMinus);

        #endregion KEYBINDS

        public LookInput LookBind { get; }

        private sealed class KeyButtonSettings : ApplicationSettingsBase
        {
            [UserScopedSetting]
            [SettingsSerializeAs(SettingsSerializeAs.Xml)]
            [DefaultSettingValue("")]
            public KeyButtonPair KeyButtonPair
            {
                get => (KeyButtonPair) this[nameof(KeyButtonPair)];
                set => this[nameof(KeyButtonPair)] = value;
            }
        }
    }
}