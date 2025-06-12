// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Application.Settings;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.Graphics.Input.Actions;
using VoxelGame.Graphics.Input.Collections;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Inputs;

internal sealed partial class KeybindManager : ISettingsProvider, IDisposable
{
    private readonly Dictionary<Keybind, Button> keybinds = new();

    private readonly Dictionary<Keybind, PushButton> pushButtons = new();
    private readonly Dictionary<Keybind, SimpleButton> simpleButtons = new();
    private readonly Dictionary<Keybind, ToggleButton> toggleButtons = new();

    private readonly KeyMap usageMap = new();

    private readonly List<Setting> settings = [];

    private readonly IDisposable binding;

    /// <summary>
    ///     Creates a new instance of the <see cref="KeybindManager" /> class.
    /// </summary>
    /// <param name="settings">The general settings.</param>
    /// <param name="input">The input system.</param>
    internal KeybindManager(GeneralSettings settings, Input input)
    {
        Input = input;

        Keybind.RegisterWithManager(this);

        InitializeStorage();
        InitializeUsages();
        InitializeSettings();

        LookBind = new LookInput(Input.Mouse, settings.MouseSensitivity);
        binding = settings.MouseSensitivity.Bind(args => LookBind.SetSensitivity(args.NewValue));
    }

    /// <summary>
    ///     Get the input system used by this manager.
    /// </summary>
    internal Input Input { get; }

    /// <summary>
    ///     Get the look input provided by this manager.
    /// </summary>
    internal LookInput LookBind { get; }

    /// <summary>
    ///     All keybinds managed by this class.
    /// </summary>
    internal IEnumerable<Keybind> Binds => keybinds.Keys;

    /// <inheritdoc />
    static String ISettingsProvider.Category => Language.Keybinds;

    /// <inheritdoc />
    static String ISettingsProvider.Description => Language.KeybindsSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => settings;

    /// <summary>
    ///     Bind a button to a keybind.
    /// </summary>
    internal void Add(Keybind bind, ToggleButton button)
    {
        AddKeybind(bind, button);
        toggleButtons.Add(bind, button);
    }

    /// <summary>
    ///     Bind a button to a keybind.
    /// </summary>
    internal void Add(Keybind bind, SimpleButton button)
    {
        AddKeybind(bind, button);
        simpleButtons.Add(bind, button);
    }

    /// <summary>
    ///     Bind a button to a keybind.
    /// </summary>
    internal void Add(Keybind bind, PushButton button)
    {
        AddKeybind(bind, button);
        pushButtons.Add(bind, button);
    }

    private void AddKeybind(Keybind bind, Button button)
    {
        if (keybinds.ContainsKey(bind)) Debug.Fail($"The keybind '{bind}' is already associated with an action.");

        keybinds[bind] = button;

        LogCreatedKeybind(logger, bind);
    }

    private void InitializeStorage()
    {
        foreach (KeyValuePair<Keybind, Button> pair in keybinds)
        {
            String key = PropertyName(pair.Key);

            SettingsProperty property = new(key)
            {
                PropertyType = typeof(OptionalKey),
                IsReadOnly = false,
                DefaultValue = "",
                Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"],
                SerializeAs = SettingsSerializeAs.Xml
            };

            property.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());

            Properties.Settings.Default.Properties.Add(property);
        }

        Properties.Settings.Default.Reload();

        foreach ((Keybind keybind, Button button) in keybinds)
        {
            String key = PropertyName(keybind);
            var state = (OptionalKey) Properties.Settings.Default[key];

            if (!state.Default) button.SetBinding(state.Key);
        }

        Properties.Settings.Default.Save();

        LogFinishedInitializingKeybindSettings(logger);
    }

    private void InitializeUsages()
    {
        foreach (KeyValuePair<Keybind, Button> pair in keybinds) UpdateAddedBind(pair.Value.Key);
    }

    internal ToggleButton GetToggle(Keybind bind)
    {
        Debug.Assert(toggleButtons.ContainsKey(bind));

        return toggleButtons[bind];
    }

    internal Button GetButton(Keybind bind)
    {
        Debug.Assert(simpleButtons.ContainsKey(bind));

        return simpleButtons[bind];
    }

    internal PushButton GetPushButton(Keybind bind)
    {
        Debug.Assert(pushButtons.ContainsKey(bind));

        return pushButtons[bind];
    }

    private void Rebind(Keybind bind, VirtualKeys key, Boolean isDefault)
    {
        Debug.Assert(keybinds.ContainsKey(bind));

        usageMap.RemoveBinding(keybinds[bind].Key);
        keybinds[bind].SetBinding(key);

        Input.IgnoreKeyUntilRelease(key);

        Properties.Settings.Default[PropertyName(bind)] = key.GetSettings(isDefault);
        Properties.Settings.Default.Save();

        LogRebindKeybind(logger, bind, key);

        UpdateAddedBind(key);
    }

    private static String PropertyName(Keybind bind)
    {
        return $"input_{bind}";
    }

    private VirtualKeys GetCurrentBind(Keybind bind)
    {
        Debug.Assert(keybinds.ContainsKey(bind));

        return keybinds[bind].Key;
    }

    private void UpdateAddedBind(VirtualKeys key)
    {
        Boolean unused = usageMap.AddBinding(key);

        if (!unused)
            LogKeyUsedByMultipleBindings(logger, key);
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
    internal Keybind UnlockMouse { get; } = Keybind.RegisterPushButton("unlock_mouse", Language.KeyUnlockMouse, VirtualKeys.F3);
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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<KeybindManager>();

    [LoggerMessage(EventId = LogID.KeybindManager + 0, Level = LogLevel.Debug, Message = "Created keybind: {Bind}")]
    private static partial void LogCreatedKeybind(ILogger logger, Keybind bind);

    [LoggerMessage(EventId = LogID.KeybindManager + 1, Level = LogLevel.Information, Message = "Finished initializing keybind settings")]
    private static partial void LogFinishedInitializingKeybindSettings(ILogger logger);

    [LoggerMessage(EventId = LogID.KeybindManager + 2, Level = LogLevel.Warning, Message = "Key '{Key}' is used by multiple bindings")]
    private static partial void LogKeyUsedByMultipleBindings(ILogger logger, VirtualKeys key);

    [LoggerMessage(EventId = LogID.KeybindManager + 3, Level = LogLevel.Information, Message = "Rebind '{Bind}' to: {Key}")]
    private static partial void LogRebindKeybind(ILogger logger, Keybind bind, VirtualKeys key);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) binding.Dispose();

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~KeybindManager()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
