// <copyright file="LoggingEvents.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
//
//     Parts of the documentation of this file are from the OpenGL wiki.
//
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Core
{
    /// <summary>
    /// Event IDs for important logging events.
    /// </summary>
    public static class LoggingEvents
    {
        /// <summary>
        /// The default ID, which is also used if no ID is given.
        /// </summary>
        public const int Default = 0;

        #region OPENGL

        // Documentation by OpenGL.
        // OpenGL Error. (2020, May 29). OpenGL Wiki, . Retrieved 15:26, July 9, 2020 from http://www.khronos.org/opengl/wiki_opengl/index.php?title=OpenGL_Error&oldid=14669.

        /// <summary>
        /// Given when an enumeration parameter is not a legal enumeration for that function. This is given only for local problems; if the spec allows the enumeration in certain circumstances, where other parameters or state dictate those circumstances, then <see cref="GlInvalidOperation"/> is the result instead.
        /// </summary>
        public const int GlInvalidEnum = 500;

        /// <summary>
        /// Given when a value parameter is not a legal value for that function. This is only given for local problems; if the spec allows the value in certain circumstances, where other parameters or state dictate those circumstances, then <see cref="GlInvalidOperation"/> is the result instead.
        /// </summary>
        public const int GlInvalidValue = 501;

        /// <summary>
        /// Given when the set of state for a command is not legal for the parameters given to that command. It is also given for commands where combinations of parameters define what the legal parameters are.
        /// </summary>
        public const int GlInvalidOperation = 502;

        /// <summary>
        /// Given when a stack pushing operation cannot be done because it would overflow the limit of that stack's size.
        /// </summary>
        public const int GlStackOverflow = 503;

        /// <summary>
        /// Given when a stack popping operation cannot be done because the stack is already at its lowest point.
        /// </summary>
        public const int GlStackUnderflow = 504;

        /// <summary>
        /// Given when performing an operation that can allocate memory, and the memory cannot be allocated. The results of OpenGL functions that return this error are undefined; it is allowable for partial execution of an operation to happen in this circumstance.
        /// </summary>
        public const int GlOutOfMemory = 505;

        /// <summary>
        /// Given when doing anything that would attempt to read from or write/render to a framebuffer that is not complete.
        /// </summary>
        public const int GlInvalidFramebufferOperation = 506;

        /// <summary>
        /// Given if the OpenGL context has been lost, due to a graphics card reset.
        /// </summary>
        public const int GlContextLost = 507;

        #endregion OPENGL

        #region WORLD LOGIC

        /// <summary>
        /// An error that occurs when saving the world.
        /// </summary>
        public const int WorldSavingError = 1000;

        /// <summary>
        /// An error that occurs when loading a world, e.g. when the meta file is damaged.
        /// </summary>
        public const int WorldLoadingError = 1001;

        /// <summary>
        /// An error that occurs when saving a chunk.
        /// </summary>
        public const int ChunkSavingError = 1002;

        /// <summary>
        /// An error that occurs when loading a chunk.
        /// </summary>
        public const int ChunkLoadingError = 1003;

        /// <summary>
        /// An error that occurs when meshing a chunk.
        /// </summary>
        public const int ChunkMeshingError = 1004;

        /// <summary>
        /// Occurs when a chunk is requested.
        /// </summary>
        public const int ChunkRequest = 1050;

        /// <summary>
        /// Occurs when a chunk is released.
        /// </summary>
        public const int ChunkRelease = 1060;

        #endregion WORLD LOGIC

        #region BLOCKS

        /// <summary>
        /// Occurs when loading a block.
        /// </summary>
        public const int BlockLoad = 2000;

        #endregion BLOCKS

        #region LIQUIDS

        /// <summary>
        /// Occurs when loading a liquid.
        /// </summary>
        public const int LiquidLoad = 2500;

        #endregion LIQUIDS

        #region RENDERING AND VISUALS

        /// <summary>
        /// Occurs when setting up shaders.
        /// </summary>
        public const int ShaderError = 3000;

        /// <summary>
        /// Occurs when rendering objects are incorrectly disposed and their buffers are not deleted. This causes memory leaks.
        /// </summary>
        public const int UndeletedBuffers = 3010;

        /// <summary>
        /// Occurs when textures are incorrectly disposed and their storage is not deleted. This causes memory leak.
        /// </summary>
        public const int UndeletedTexture = 3011;

        /// <summary>
        /// Occurs when a texture or model that is requested could not be loaded and a fallback is used.
        /// </summary>
        public const int MissingRessource = 3100;

        #endregion RENDERING AND VISUALS
    }
}