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
using VoxelGame.Core.Resources.Language;
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Collections;
using VoxelGame.Input.Internal;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager : ISettingsProvider
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<KeybindManager>();

        private readonly Dictionary<Keybind, Button> keybinds = new();
        private readonly Dictionary<Keybind, PushButton> pushButtons = new();

        private readonly List<Setting> settings = new();
        private readonly Dictionary<Keybind, SimpleButton> simpleButtons = new();

        private readonly Dictionary<Keybind, ToggleButton> toggleButtons = new();

        private readonly KeyMap usageMap = new();

        public KeybindManager(InputManager input)
        {
            Input = input;

            Keybind.RegisterWithManager(this);

            InitializeStorage();
            InitializeUsages();
            InitializeSettings();

            LookBind = new LookInput(Input.Mouse, Client.Instance.Settings.MouseSensitivity);
            Client.Instance.Settings.MouseSensitivityChanged += (_, args) => LookBind.SetSensitivity(args.NewValue);
        }

        public InputManager Input { get; }

        private IEnumerable<Keybind> Binds => keybinds.Keys;

        public LookInput LookBind { get; }

        public string Category => Language.Keybinds;
        public string Description => Language.KeybindsSettingsDescription;

        public IEnumerable<Setting> Settings => settings;

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

        private void InitializeStorage()
        {
            foreach (KeyValuePair<Keybind, Button> pair in keybinds)
            {
                string key = PropertyName(pair.Key);

                SettingsProperty property = new(key)
                {
                    PropertyType = typeof(KeyButtonPair),
                    IsReadOnly = false,
                    DefaultValue = KeyButtonPair.DefaultValue,
                    Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"],
                    SerializeAs = SettingsSerializeAs.Xml
                };

                property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());

                Properties.Settings.Default.Properties.Add(property);
            }

            Properties.Settings.Default.Reload();

            foreach ((Keybind keybind, Button button) in keybinds)
            {
                string key = PropertyName(keybind);
                var state = (KeyButtonPair) Properties.Settings.Default[key];

                if (state.Default) Properties.Settings.Default[key] = button.KeyOrButton.Settings;
                else button.SetBinding(new KeyOrButton(state));
            }

            Properties.Settings.Default.Save();

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
            Input.AddPullDown(keyOrButton);

            Properties.Settings.Default[PropertyName(bind)] = keyOrButton.Settings;
            Properties.Settings.Default.Save();

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

        private void InitializeSettings()
        {
            foreach (Keybind bind in Binds)
            {
                var setting = Setting.CreateKeyOrButtonSetting(
                    bind.Name,
                    () => GetCurrentBind(bind),
                    keyOrButton => Rebind(bind, keyOrButton));

                settings.Add(setting);
            }
        }

        #region KEYBINDS

        public Keybind Fullscreen { get; } = Keybind.RegisterToggle("fullscreen", Language.KeyFullscreen, Key.F11);

        public Keybind Wireframe { get; } = Keybind.RegisterToggle("wireframe", Language.KeyWireframe, Key.K);
        public Keybind UI { get; } = Keybind.RegisterToggle("ui", Language.KeyToggleUI, Key.J);

        public Keybind Screenshot { get; } = Keybind.RegisterPushButton("screenshot", Language.KeyScreenshot, Key.F12);
        public Keybind Escape { get; } = Keybind.RegisterPushButton("escape", Language.KeyEscape, Key.Escape);

        public Keybind Forwards { get; } = Keybind.RegisterButton("forwards", Language.KeyForwards, Key.W);
        public Keybind Backwards { get; } = Keybind.RegisterButton("backwards", Language.KeyBackwards, Key.S);
        public Keybind StrafeRight { get; } = Keybind.RegisterButton("strafe_right", Language.KeyStrafeRight, Key.D);
        public Keybind StrafeLeft { get; } = Keybind.RegisterButton("strafe_left", Language.KeyStrafeLeft, Key.A);

        public Keybind Sprint { get; } = Keybind.RegisterButton("sprint", Language.KeySprint, Key.ShiftLeft);
        public Keybind Jump { get; } = Keybind.RegisterButton("jump", Language.KeyJump, Key.Space);

        public Keybind InteractOrPlace { get; } = Keybind.RegisterButton(
            "interact_or_place",
            Language.KeyInteractOrPlace,
            MouseButton.Right);

        public Keybind Destroy { get; } = Keybind.RegisterButton("destroy", Language.KeyDestroy, MouseButton.Left);

        public Keybind BlockInteract { get; } = Keybind.RegisterButton(
            "block_interact",
            Language.KeyForceInteract,
            Key.ControlLeft);

        public Keybind PlacementMode { get; } =
            Keybind.RegisterToggle("placement_mode", Language.KeyPlacementMode, Key.R);

        public Keybind NextPlacement { get; } = Keybind.RegisterPushButton(
            "select_next_placement",
            Language.KeyNextPlacement,
            Key.KeypadPlus);

        public Keybind PreviousPlacement { get; } =
            Keybind.RegisterPushButton("select_previous_placement", Language.KeyPreviousPlacement, Key.KeypadMinus);

        #endregion KEYBINDS
    }
}
