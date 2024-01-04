// <copyright file="ShaderBuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     Represents a shader constant buffer.
/// </summary>
public class ShaderBuffer<T> : NativeObject where T : unmanaged, IEquatable<T>
{
    /// <summary>
    ///     Delegate for modifying the data of the buffer.
    /// </summary>
    public delegate void ModifyDelegate(ref T data);

    private T data;
    private bool dirty = true;

    /// <summary>
    ///     Creates a new <see cref="ShaderBuffer{T}" />.
    /// </summary>
    public ShaderBuffer(IntPtr nativePointer, Client client) : base(nativePointer, client) {}

    /// <summary>
    ///     Get or set the data of the buffer.
    /// </summary>
    public T Data
    {
        get => data;
        set
        {
            if (EqualityComparer<T>.Default.Equals(Data, value)) return;

            data = value;

            if (Client.IsOutOfCycle) Write();
            else dirty = true;
        }
    }

    /// <summary>
    ///     Modifies the data of the buffer.
    /// </summary>
    /// <param name="modifier">The modifier.</param>
    public void Modify(ModifyDelegate modifier)
    {
        T copy = data;
        modifier(ref copy);
        Data = copy;
    }

    internal override void Synchronize()
    {
        if (!dirty) return;

        Write();

        dirty = false;
    }

    private void Write()
    {
        Native.SetShaderBufferData(this, data);
    }
}
