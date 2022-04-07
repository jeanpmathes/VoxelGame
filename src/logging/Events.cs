// <copyright file="LoggingEvents.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
//
//     Parts of the documentation of this file are from the OpenGL wiki.
//
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Logging;

/// <summary>
///     Event IDs for important logging events.
/// </summary>
public static class Events
{
    /// <summary>
    ///     The default ID, which is also used if no ID is given.
    /// </summary>
    public const int Default = 0;

    /// <summary>
    ///     Events related to the logging system itself.
    /// </summary>
    public const int Meta = 60000;

    #region GENERAL APPLICATION

    /// <summary>
    ///     Information related to the application.
    /// </summary>
    public const int ApplicationInformation = 1;

    /// <summary>
    ///     Events that indicate a change of the general application state.
    /// </summary>
    public const int ApplicationState = 10;

    /// <summary>
    ///     Occurs when the state of the application window has changed.
    /// </summary>
    public const int WindowState = 11;

    /// <summary>
    ///     Events related to file system operations.
    /// </summary>
    public const int FileIO = 50;

    /// <summary>
    ///     Events related to direct interaction with the user.
    /// </summary>
    public const int UserInteraction = 60;

    /// <summary>
    ///     Events related to the in-game console.
    /// </summary>
    public const int Console = 65;

    /// <summary>
    ///     Occurs with events or information connected to the successful loading of resources.
    /// </summary>
    public const int ResourceLoad = 100;

    /// <summary>
    ///     Occurs when a texture or model that is requested could not be loaded and a fallback is used.
    /// </summary>
    public const int MissingResource = 110;

    /// <summary>
    ///     Occurs when entire resource directories or file structures are not available.
    /// </summary>
    public const int MissingDepository = 111;

    /// <summary>
    ///     A general event category for everything input related.
    /// </summary>
    public const int InputSystem = 200;

    /// <summary>
    ///     Occurs when a key bind is registered or the binding is changed.
    /// </summary>
    public const int SetKeyBind = 210;

    /// <summary>
    ///     Occurs when the active scene is changed.
    /// </summary>
    public const int SceneChange = 300;

    #endregion GENERAL APPLICATION

    #region OPENGL

    // Documentation by OpenGL.
    // OpenGL Error. (2020, May 29). OpenGL Wiki, . Retrieved 15:26, July 9, 2020 from http://www.khronos.org/opengl/wiki_opengl/index.php?title=OpenGL_Error&oldid=14669.

    /// <summary>
    ///     Given when an enumeration parameter is not a legal enumeration for that function. This is given only for local
    ///     problems; if the spec allows the enumeration in certain circumstances, where other parameters or state dictate
    ///     those circumstances, then <see cref="GlInvalidOperation" /> is the result instead.
    /// </summary>
    public const int GlInvalidEnum = 500;

    /// <summary>
    ///     Given when a value parameter is not a legal value for that function. This is only given for local problems; if the
    ///     spec allows the value in certain circumstances, where other parameters or state dictate those circumstances, then
    ///     <see cref="GlInvalidOperation" /> is the result instead.
    /// </summary>
    public const int GlInvalidValue = 501;

    /// <summary>
    ///     Given when the set of state for a command is not legal for the parameters given to that command. It is also given
    ///     for commands where combinations of parameters define what the legal parameters are.
    /// </summary>
    public const int GlInvalidOperation = 502;

    /// <summary>
    ///     Given when a stack pushing operation cannot be done because it would overflow the limit of that stack's size.
    /// </summary>
    public const int GlStackOverflow = 503;

    /// <summary>
    ///     Given when a stack popping operation cannot be done because the stack is already at its lowest point.
    /// </summary>
    public const int GlStackUnderflow = 504;

    /// <summary>
    ///     Given when performing an operation that can allocate memory, and the memory cannot be allocated. The results of
    ///     OpenGL functions that return this error are undefined; it is allowable for partial execution of an operation to
    ///     happen in this circumstance.
    /// </summary>
    public const int GlOutOfMemory = 505;

    /// <summary>
    ///     Given when doing anything that would attempt to read from or write/render to a framebuffer that is not complete.
    /// </summary>
    public const int GlInvalidFramebufferOperation = 506;

    /// <summary>
    ///     Given if the OpenGL context has been lost, due to a graphics card reset.
    /// </summary>
    public const int GlContextLost = 507;

    #endregion OPENGL

    #region WORLD LOGIC

    /// <summary>
    ///     Events indicating a change of the world state.
    /// </summary>
    public const int WorldState = 1000;

    /// <summary>
    ///     Events related to saving and loading worlds.
    /// </summary>
    public const int WorldIO = 1001;

    /// <summary>
    ///     Events related to setting secondary world data.
    /// </summary>
    public const int WorldData = 1002;

    /// <summary>
    ///     An error that occurs when saving the world.
    /// </summary>
    public const int WorldSavingError = 1010;

    /// <summary>
    ///     An error that occurs when loading a world, e.g. when the meta file is damaged.
    /// </summary>
    public const int WorldLoadingError = 1011;

    /// <summary>
    ///     An error that occurs when saving a chunk.
    /// </summary>
    public const int ChunkSavingError = 1012;

    /// <summary>
    ///     An error that occurs when loading a chunk.
    /// </summary>
    public const int ChunkLoadingError = 1013;

    /// <summary>
    ///     An error that occurs when meshing a chunk.
    /// </summary>
    public const int ChunkMeshingError = 1014;

    /// <summary>
    ///     Different chunk operations like loading, saving or generating.
    /// </summary>
    public const int ChunkOperation = 1030;

    /// <summary>
    ///     Occurs when a chunk is requested.
    /// </summary>
    public const int ChunkRequest = 1031;

    /// <summary>
    ///     Occurs when a chunk is released.
    /// </summary>
    public const int ChunkRelease = 1032;

    /// <summary>
    ///     Events related to world generation.
    /// </summary>
    public const int WorldGeneration = 1060;

    #endregion WORLD LOGIC

    #region BLOCKS

    /// <summary>
    ///     Occurs when loading blocks.
    /// </summary>
    public const int BlockLoad = 2000;

    /// <summary>
    ///     Occurs when stored block information refers to unknown blocks.
    /// </summary>
    public const int UnknownBlock = 2010;

    #endregion BLOCKS

    #region LIQUIDS

    /// <summary>
    ///     Occurs when loading liquids.
    /// </summary>
    public const int LiquidLoad = 2500;

    /// <summary>
    ///     Occurs when stored block information refers to unknown blocks.
    /// </summary>
    public const int UnknownLiquid = 2510;

    #endregion LIQUIDS

    #region RENDERING AND VISUALS

    /// <summary>
    ///     Events related to setting up the rendering environment.
    /// </summary>
    public const int VisualsSetup = 3000;

    /// <summary>
    ///     Occurs when graphics quality settings or configurations are changed.
    /// </summary>
    public const int VisualQuality = 3001;

    /// <summary>
    ///     Events related to shader loading and compilation.
    /// </summary>
    public const int ShaderSetup = 3005;

    /// <summary>
    ///     Errors that occur during shader setup.
    /// </summary>
    public const int ShaderError = 3006;

    /// <summary>
    ///     Occurs when objects are incorrectly disposed and their OpenGL objects are not deleted. This causes memory leak.
    /// </summary>
    public const int UndeletedGlObjects = 3010;

    /// <summary>
    ///     Occurs when rendering objects are incorrectly disposed and their buffers are not deleted. This causes memory leaks.
    /// </summary>
    public const int UndeletedBuffers = 3011;

    /// <summary>
    ///     Occurs when textures are incorrectly disposed and their storage is not deleted. This causes memory leak.
    /// </summary>
    public const int UndeletedTexture = 3012;

    /// <summary>
    ///     Occurs when a screenshot is taken.
    /// </summary>
    public const int Screenshot = 3150;

    #endregion RENDERING AND VISUALS
}
