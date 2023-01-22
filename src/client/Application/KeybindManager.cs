// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Collections;
using VoxelGame.Input.Internal;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application;

internal class KeybindManager : ISettingsProvider
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<KeybindManager>();

    private readonly Dictionary<Keybind, Button> keybinds = new();
    private readonly Dictionary<Keybind, PushButton> pushButtons = new();

    private readonly List<Setting> settings = new();
    private readonly Dictionary<Keybind, SimpleButton> simpleButtons = new();

    private readonly Dictionary<Keybind, ToggleButton> toggleButtons = new();

    private readonly KeyMap usageMap = new();

    internal KeybindManager(InputManager input)
    {
        Input = input;

        Keybind.RegisterWithManager(this);

        InitializeStorage();
        InitializeUsages();
        InitializeSettings();

        LookBind = new LookInput(Input.Mouse, Client.Instance.Settings.MouseSensitivity);
        Client.Instance.Settings.MouseSensitivityChanged += (_, args) => LookBind.SetSensitivity(args.NewValue);
    }

    internal InputManager Input { get; }

    internal LookInput LookBind { get; }

    /// <summary>
    ///     All keybinds managed by this class.
    /// </summary>
    internal IEnumerable<Keybind> Binds => keybinds.Keys;

    public string Category => Language.Keybinds;
    public string Description => Language.KeybindsSettingsDescription;

    public IEnumerable<Setting> Settings => settings;

    internal void Add(Keybind bind, ToggleButton button)
    {
        AddKeybind(bind, button);
        toggleButtons.Add(bind, button);
    }

    internal void Add(Keybind bind, SimpleButton button)
    {
        AddKeybind(bind, button);
        simpleButtons.Add(bind, button);
    }

    internal void Add(Keybind bind, PushButton button)
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

            if (!state.Default) button.SetBinding(new KeyOrButton(state));
        }

        Properties.Settings.Default.Save();

        logger.LogInformation(Events.InputSystem, "Finished initializing keybind settings");
    }

    private void InitializeUsages()
    {
        foreach (KeyValuePair<Keybind, Button> pair in keybinds) UpdateAddedBind(pair.Value.KeyOrButton);
    }

    internal ToggleButton GetToggle(Keybind bind)
    {
        Debug.Assert(toggleButtons.ContainsKey(bind), "No toggle associated with this keybind.");

        return toggleButtons[bind];
    }

    internal Button GetButton(Keybind bind)
    {
        Debug.Assert(simpleButtons.ContainsKey(bind), "No simple button associated with this keybind.");

        return simpleButtons[bind];
    }

    internal PushButton GetPushButton(Keybind bind)
    {
        Debug.Assert(pushButtons.ContainsKey(bind), "No push button associated with this keybind.");

        return pushButtons[bind];
    }

    private void Rebind(Keybind bind, KeyOrButton keyOrButton, bool isDefault)
    {
        Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");

        usageMap.RemoveBinding(keybinds[bind].KeyOrButton);
        keybinds[bind].SetBinding(keyOrButton);
        Input.AddPullDown(keyOrButton);

        Properties.Settings.Default[PropertyName(bind)] = keyOrButton.GetSettings(isDefault);
        Properties.Settings.Default.Save();

        logger.LogInformation(Events.SetKeyBind, "Rebind '{Bind}' to: {Key}", bind, keyOrButton);

        UpdateAddedBind(keyOrButton);
    }

    private static string PropertyName(Keybind bind)
    {
        return $"Input_{bind}";
    }

    internal KeyOrButton GetCurrentBind(Keybind bind)
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
                this,
                bind.Name,
                () => GetCurrentBind(bind),
                keyOrButton => Rebind(bind, keyOrButton, isDefault: false),
                () => usageMap.GetUsageCount(GetCurrentBind(bind)) <= 1,
                () => Rebind(bind, bind.Default, isDefault: true));

            settings.Add(setting);
        }
    }

    #region KEYBINDS

    internal Keybind Fullscreen { get; } = Keybind.RegisterToggle("fullscreen", Language.KeyFullscreen, Keys.F11);

    internal Keybind UI { get; } = Keybind.RegisterToggle("ui", Language.KeyToggleUI, Keys.F10);

    internal Keybind Screenshot { get; } =
        Keybind.RegisterPushButton("screenshot", Language.KeyScreenshot, Keys.F12);

    internal Keybind Console { get; } = Keybind.RegisterToggle("console", Language.KeyConsole, Keys.F1);
    internal Keybind DebugView { get; } = Keybind.RegisterPushButton("debug_view", Language.KeyDebugView, Keys.F2);
    internal Keybind Escape { get; } = Keybind.RegisterPushButton("escape", Language.KeyEscape, Keys.Escape);

    internal Keybind Forwards { get; } = Keybind.RegisterButton("forwards", Language.KeyForwards, Keys.W);
    internal Keybind Backwards { get; } = Keybind.RegisterButton("backwards", Language.KeyBackwards, Keys.S);
    internal Keybind StrafeRight { get; } = Keybind.RegisterButton("strafe_right", Language.KeyStrafeRight, Keys.D);
    internal Keybind StrafeLeft { get; } = Keybind.RegisterButton("strafe_left", Language.KeyStrafeLeft, Keys.A);

    internal Keybind Sprint { get; } = Keybind.RegisterButton("sprint", Language.KeySprint, Keys.LeftShift);
    internal Keybind Jump { get; } = Keybind.RegisterButton("jump", Language.KeyJump, Keys.Space);
    internal Keybind Crouch { get; } = Keybind.RegisterButton("crouch", Language.KeyCrouch, Keys.C);

    internal Keybind InteractOrPlace { get; } = Keybind.RegisterButton(
        "interact_or_place",
        Language.KeyInteractOrPlace,
        MouseButton.Right);

    internal Keybind Destroy { get; } = Keybind.RegisterButton("destroy", Language.KeyDestroy, MouseButton.Left);

    internal Keybind BlockInteract { get; } = Keybind.RegisterButton(
        "block_interact",
        Language.KeyForceInteract,
        Keys.LeftControl);

    internal Keybind PlacementMode { get; } =
        Keybind.RegisterToggle("placement_mode", Language.KeyPlacementMode, Keys.R);

    internal Keybind NextPlacement { get; } = Keybind.RegisterPushButton(
        "select_next_placement",
        Language.KeyNextPlacement,
        Keys.KeyPadAdd);

    internal Keybind PreviousPlacement { get; } =
        Keybind.RegisterPushButton("select_previous_placement", Language.KeyPreviousPlacement, Keys.KeyPadSubtract);

    internal Keybind SelectTargeted { get; } = Keybind.RegisterPushButton(
        "select_targeted",
        Language.KeySelectTargeted,
        MouseButton.Button3);

    #endregion KEYBINDS
}

