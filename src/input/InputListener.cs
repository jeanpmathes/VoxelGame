// <copyright file="InputListener.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input
{
    /// <summary>
    ///     Listens for requested inputs. Can then notify or absorb the input.
    /// </summary>
    public class InputListener
    {
        private readonly List<Action<KeyOrButton>> callbackListForAnyPress = new();

        private readonly InputManager manager;

        internal InputListener(InputManager manager)
        {
            this.manager = manager;
        }

        internal void ProcessInput(CombinedState state)
        {
            if (state.IsAnyKeyOrButtonDown && callbackListForAnyPress.Count > 0)
            {
                KeyOrButton any = state.Any;

                foreach (Action<KeyOrButton> callback in callbackListForAnyPress) callback(any);

                callbackListForAnyPress.Clear();
            }
        }

        /// <summary>
        ///     Listen for the next mouse press and absorb it.
        /// </summary>
        public void AbsorbMousePress()
        {
            manager.AddPullDown(new KeyOrButton(MouseButton.Button1));
        }

        /// <summary>
        ///     Listen for any key or button press and notify the callback.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        public void ListenForAnyKeyOrButton(Action<KeyOrButton> callback)
        {
            callbackListForAnyPress.Add(callback);
        }
    }
}