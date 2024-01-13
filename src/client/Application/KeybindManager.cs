// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Input;
using VoxelGame.Support.Input.Actions;
using VoxelGame.Support.Input.Collections;
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

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Category => Language.Keybinds;

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
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
        // todo: check that the keybind saving, resetting etc. still works as intended

        foreach (KeyValuePair<Keybind, Button> pair in keybinds)
        {
            string key = PropertyName(pair.Key);

            SettingsProperty property = new(key)
            {
                PropertyType = typeof(OptionalKey),
                IsReadOnly = false,
                DefaultValue = OptionalKey.DefaultValue,
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
            var state = (OptionalKey) Properties.Settings.Default[key];

            if (!state.Default) button.SetBinding(state.Key);
        }

        Properties.Settings.Default.Save();

        logger.LogInformation(Events.InputSystem, "Finished initializing keybind settings");
    }

    private void InitializeUsages()
    {
        foreach (KeyValuePair<Keybind, Button> pair in keybinds) UpdateAddedBind(pair.Value.Key);
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

    private void Rebind(Keybind bind, VirtualKeys key, bool isDefault)
    {
        Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");

        usageMap.RemoveBinding(keybinds[bind].Key);
        keybinds[bind].SetBinding(key);
        Input.AddPullDown(key);

        Properties.Settings.Default[PropertyName(bind)] = key.GetSettings(isDefault);
        Properties.Settings.Default.Save();

        logger.LogInformation(Events.SetKeyBind, "Rebind '{Bind}' to: {Key}", bind, key);

        UpdateAddedBind(key);
    }

    private static string PropertyName(Keybind bind)
    {
        return $"Input_{bind}";
    }

    private VirtualKeys GetCurrentBind(Keybind bind)
    {
        Debug.Assert(keybinds.ContainsKey(bind), "No keybind associated with this keybind.");

        return keybinds[bind].Key;
    }

    private void UpdateAddedBind(VirtualKeys key)
    {
        bool unused = usageMap.AddBinding(key);

        if (!unused)
            logger.LogWarning(Events.SetKeyBind, "Key '{Key}' is used by multiple bindings", key);
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

    internal Keybind Fullscreen { get; } = Keybind.RegisterToggle("fullscreen", Language.KeyFullscreen, VirtualKeys.F11);

    internal Keybind UI { get; } = Keybind.RegisterToggle("ui", Language.KeyToggleUI, VirtualKeys.F10);

    internal Keybind Screenshot { get; } =
        Keybind.RegisterPushButton("screenshot", Language.KeyScreenshot, VirtualKeys.F12);

    internal Keybind Console { get; } = Keybind.RegisterToggle("console", Language.KeyConsole, VirtualKeys.F1);
    internal Keybind DebugView { get; } = Keybind.RegisterPushButton("debug_view", Language.KeyDebugView, VirtualKeys.F2);
    internal Keybind Escape { get; } = Keybind.RegisterPushButton("escape", Language.KeyEscape, VirtualKeys.Escape);

    internal Keybind Forwards { get; } = Keybind.RegisterButton("forwards", Language.KeyForwards, VirtualKeys.W);
    internal Keybind Backwards { get; } = Keybind.RegisterButton("backwards", Language.KeyBackwards, VirtualKeys.S);
    internal Keybind StrafeRight { get; } = Keybind.RegisterButton("strafe_right", Language.KeyStrafeRight, VirtualKeys.D);
    internal Keybind StrafeLeft { get; } = Keybind.RegisterButton("strafe_left", Language.KeyStrafeLeft, VirtualKeys.A);

    internal Keybind Sprint { get; } = Keybind.RegisterButton("sprint", Language.KeySprint, VirtualKeys.LeftShift);
    internal Keybind Jump { get; } = Keybind.RegisterButton("jump", Language.KeyJump, VirtualKeys.Space);
    internal Keybind Crouch { get; } = Keybind.RegisterButton("crouch", Language.KeyCrouch, VirtualKeys.C);

    internal Keybind InteractOrPlace { get; } = Keybind.RegisterButton(
        "interact_or_place",
        Language.KeyInteractOrPlace,
        VirtualKeys.RightButton);

    internal Keybind Destroy { get; } = Keybind.RegisterButton("destroy", Language.KeyDestroy, VirtualKeys.LeftButton);

    internal Keybind BlockInteract { get; } = Keybind.RegisterButton(
        "block_interact",
        Language.KeyForceInteract,
        VirtualKeys.LeftControl);

    internal Keybind PlacementMode { get; } =
        Keybind.RegisterToggle("placement_mode", Language.KeyPlacementMode, VirtualKeys.R);

    internal Keybind NextPlacement { get; } = Keybind.RegisterPushButton(
        "select_next_placement",
        Language.KeyNextPlacement,
        VirtualKeys.Add);

    internal Keybind PreviousPlacement { get; } =
        Keybind.RegisterPushButton("select_previous_placement", Language.KeyPreviousPlacement, VirtualKeys.Subtract);

    internal Keybind SelectTargeted { get; } = Keybind.RegisterPushButton(
        "select_targeted",
        Language.KeySelectTargeted,
        VirtualKeys.MiddleButton);

    #endregion KEYBINDS
}
