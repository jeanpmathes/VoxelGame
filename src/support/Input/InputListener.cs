// <copyright file="InputListener.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input;

/// <summary>
///     Listens for requested inputs. Can then notify or absorb the input.
/// </summary>
public class InputListener
{
    private readonly List<Action<VirtualKeys>> callbackListForAnyPress = new();

    private readonly InputManager manager;

    internal InputListener(InputManager manager)
    {
        this.manager = manager;
    }

    internal void ProcessInput(KeyState state)
    {
        if (state.IsAnyKeyDown && callbackListForAnyPress.Count > 0)
        {
            VirtualKeys any = state.Any;

            foreach (Action<VirtualKeys> callback in callbackListForAnyPress) callback(any);

            callbackListForAnyPress.Clear();
        }
    }

    /// <summary>
    ///     Listen for the next mouse press (left mouse button) and absorb it.
    /// </summary>
    public void AbsorbMousePress()
    {
        manager.AddPullDown(VirtualKeys.LeftButton);
    }

    /// <summary>
    ///     Listen for any key or button press and notify the callback.
    /// </summary>
    /// <param name="callback">The callback to call.</param>
    public void ListenForAnyKeyOrButton(Action<VirtualKeys> callback)
    {
        callbackListForAnyPress.Add(callback);
    }
}

