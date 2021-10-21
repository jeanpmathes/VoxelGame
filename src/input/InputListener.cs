// <copyright file="InputListener.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input
{
    public class InputListener
    {
        private readonly List<Action<KeyOrButton>> callbackListForAnyPress = new();

        internal InputListener() {}

        internal void ProcessInput(CombinedState state)
        {
            if (state.IsAnyKeyOrButtonDown && callbackListForAnyPress.Count > 0)
            {
                KeyOrButton any = state.Any;

                foreach (Action<KeyOrButton> callback in callbackListForAnyPress) callback(any);

                callbackListForAnyPress.Clear();
            }
        }

        public void ListenForAnyKeyOrButton(Action<KeyOrButton> callback)
        {
            callbackListForAnyPress.Add(callback);
        }
    }
}