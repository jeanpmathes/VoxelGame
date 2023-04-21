// <copyright file="ShaderBuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Objects;

/// <summary>
///     Represents a shader constant buffer.
/// </summary>
public class ShaderBuffer<T> : NativeObject where T : unmanaged
{
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
            dirty = true;
        }
    }

    internal override void Synchronize()
    {
        if (!dirty) return;

        Native.SetShaderBufferData(this, data);
        dirty = false;
    }
}
