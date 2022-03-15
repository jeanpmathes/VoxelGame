// <copyright file="Keybind.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Internal;

namespace VoxelGame.Client.Application
{
    /// <summary>
    ///     Represents a keybind, that associates a key with a binding.
    /// </summary>
    public readonly struct Keybind : IEquatable<Keybind>
    {
        private readonly string id;
        private readonly Binding type;

        /// <summary>
        ///     Get the name of the keybind.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Get the key used by the keybind per default.
        /// </summary>
        public KeyOrButton Default { get; }

        private enum Binding
        {
            PushButton,
            ToggleButton,
            SimpleButton
        }

        private Keybind(string id, string name, Binding type, KeyOrButton defaultKeyOrButton)
        {
            this.id = id;
            Name = name;
            this.type = type;

            Default = defaultKeyOrButton;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Keybind other) return this == other;

            return false;
        }

        /// <inheritdoc />
        public bool Equals(Keybind other)
        {
            return id.Equals(other.id, StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return id.GetHashCode(StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Check equality of two keybinds.
        /// </summary>
        /// <param name="left">The first keybind.</param>
        /// <param name="right">The second keybind.</param>
        /// <returns>True if both keybinds are equal.</returns>
        public static bool operator ==(Keybind left, Keybind right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check inequality of two keybinds.
        /// </summary>
        /// <param name="left">The first keybind.</param>
        /// <param name="right">The second keybind.</param>
        /// <returns>True if both keybinds are not equal.</returns>
        public static bool operator !=(Keybind left, Keybind right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return id;
        }

        /// <summary>
        ///     Register a keybind that is bound to a button.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultKey">The default key to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterButton(string id, string name, Keys defaultKey)
        {
            return Register(
                id,
                name,
                Binding.SimpleButton,
                new KeyOrButton(defaultKey));
        }

        /// <summary>
        ///     Register a keybind that is bound to a button.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultButton">The default button to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterButton(string id, string name, MouseButton defaultButton)
        {
            return Register(
                id,
                name,
                Binding.SimpleButton,
                new KeyOrButton(defaultButton));
        }

        /// <summary>
        ///     Register a keybind that is bound to a toggle.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultKey">The default key to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterToggle(string id, string name, Keys defaultKey)
        {
            return Register(
                id,
                name,
                Binding.ToggleButton,
                new KeyOrButton(defaultKey));
        }

        /// <summary>
        ///     Register a keybind that is bound to a toggle.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultButton">The default button to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterToggle(string id, string name, MouseButton defaultButton)
        {
            return Register(
                id,
                name,
                Binding.ToggleButton,
                new KeyOrButton(defaultButton));
        }

        /// <summary>
        ///     Register a keybind that is bound to a push button.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultKey">The default key to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterPushButton(string id, string name, Keys defaultKey)
        {
            return Register(
                id,
                name,
                Binding.PushButton,
                new KeyOrButton(defaultKey));
        }

        /// <summary>
        ///     Register a keybind that is bound to a push button.
        /// </summary>
        /// <param name="id">The id of the keybind. Must be unique.</param>
        /// <param name="name">The display name of the keybind. Can be localized.</param>
        /// <param name="defaultButton">The default button to use initially.</param>
        /// <returns>The registered keybind.</returns>
        public static Keybind RegisterPushButton(string id, string name, MouseButton defaultButton)
        {
            return Register(
                id,
                name,
                Binding.PushButton,
                new KeyOrButton(defaultButton));
        }

        private static Keybind Register(string id, string name, Binding type, KeyOrButton defaultKeyOrButton)
        {
            var bind = new Keybind(id, name, type, defaultKeyOrButton);

            Debug.Assert(!bindings.Contains(bind), $"The binding '{bind.id}' is already defined.");
            bindings.Add(bind);

            return bind;
        }

        private static readonly HashSet<Keybind> bindings = new();

        internal static void RegisterWithManager(KeybindManager manager)
        {
            foreach (Keybind bind in bindings) bind.AddToManager(manager);

            bindings.Clear();
        }

        private void AddToManager(KeybindManager manager)
        {
            switch (type)
            {
                case Binding.PushButton:
                    manager.Add(this, new PushButton(Default, manager.Input));

                    break;

                case Binding.ToggleButton:
                    manager.Add(this, new ToggleButton(Default, manager.Input));

                    break;

                case Binding.SimpleButton:
                    manager.Add(this, new SimpleButton(Default, manager.Input));

                    break;

                default:
                    Debug.Fail("Add missing cases.");

                    break;
            }
        }
    }
}
