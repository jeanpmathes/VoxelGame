// <copyright file="RenderType.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    /// <summary>
    ///     The render type of a liquid.
    /// </summary>
    public enum RenderType
    {
        /// <summary>
        ///     The liquid is not rendered.
        /// </summary>
        NotRendered,

        /// <summary>
        ///     The liquid is opaque.
        /// </summary>
        Opaque,

        /// <summary>
        ///     The liquid is transparent.
        /// </summary>
        Transparent
    }
}
