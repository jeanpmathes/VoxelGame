// <copyright file="Keybind.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Actions;
using VoxelGame.Input.Internal;

namespace VoxelGame.Client.Application
{
    public readonly struct Keybind : IEquatable<Keybind>
    {
        private readonly string id;
        private readonly Binding type;

        private readonly KeyOrButton defaultKeyOrButton;

        public string Name { get; }

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

            this.defaultKeyOrButton = defaultKeyOrButton;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Keybind other) return this == other;

            return false;
        }

        public bool Equals(Keybind other)
        {
            return id.Equals(other.id, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode(StringComparison.InvariantCulture);
        }

        public static bool operator ==(Keybind left, Keybind right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Keybind left, Keybind right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return id;
        }

        public static Keybind RegisterButton(string id, string name, Key defaultKey)
        {
            return Register(
                id,
                name,
                Binding.SimpleButton,
                new KeyOrButton(defaultKey));
        }

        public static Keybind RegisterButton(string id, string name, MouseButton defaultButton)
        {
            return Register(
                id,
                name,
                Binding.SimpleButton,
                new KeyOrButton(defaultButton));
        }

        public static Keybind RegisterToggle(string id, string name, Key defaultKey)
        {
            return Register(
                id,
                name,
                Binding.ToggleButton,
                new KeyOrButton(defaultKey));
        }

        public static Keybind RegisterToggle(string id, string name, MouseButton defaultButton)
        {
            return Register(
                id,
                name,
                Binding.ToggleButton,
                new KeyOrButton(defaultButton));
        }

        public static Keybind RegisterPushButton(string id, string name, Key defaultKey)
        {
            return Register(
                id,
                name,
                Binding.PushButton,
                new KeyOrButton(defaultKey));
        }

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
                    manager.Add(this, new PushButton(defaultKeyOrButton, manager.Input));

                    break;

                case Binding.ToggleButton:
                    manager.Add(this, new ToggleButton(defaultKeyOrButton, manager.Input));

                    break;

                case Binding.SimpleButton:
                    manager.Add(this, new SimpleButton(defaultKeyOrButton, manager.Input));

                    break;

                default:
                    Debug.Fail("Add missing cases.");

                    break;
            }
        }
    }
}