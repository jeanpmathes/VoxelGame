// <copyright file="LoggingEvents.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
//
//     Parts of the documentation of this file are from the OpenGL wiki.
//
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Logging;

/// <summary>
///     Event IDs for important logging events.
/// </summary>
public static class Events // todo: go trough all events and make sure they are used or removed
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
    ///     Events related to OS interactions.
    /// </summary>
    public const int OS = 20;

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
    ///     Occurs with events or information connected to the successful loading of creations.
    ///     Creations are user-made resources that are loaded during an active game session.
    /// </summary>
    public const int CreationLoad = 102;

    /// <summary>
    ///     Occurs when a texture or model that is requested could not be loaded and a fallback is used.
    /// </summary>
    public const int MissingResource = 110;

    /// <summary>
    ///     Occurs when entire resource directories or file structures are not available.
    /// </summary>
    public const int MissingDepository = 111;

    /// <summary>
    ///     Occurs when a creation is not available or could not be loaded.
    /// </summary>
    public const int MissingCreation = 112;

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

    /// <summary>
    ///     Any event concerning the physics system.
    /// </summary>
    public const int PhysicsSystem = 1200;

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
    ///     Occurs when loading fluids.
    /// </summary>
    public const int FluidLoad = 2500;

    /// <summary>
    ///     Occurs when stored block information refers to unknown blocks.
    /// </summary>
    public const int UnknownFluid = 2510;

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
    ///     Events related to render pipeline loading and compilation.
    /// </summary>
    public const int RenderPipelineSetup = 3005;

    /// <summary>
    ///     Errors that occur during render pipeline setup.
    /// </summary>
    public const int RenderPipelineError = 3006;

    /// <summary>
    ///     Occurs when wrapper objects are incorrectly disposed and their native objects are not deleted. This causes memory
    ///     leaks.
    /// </summary>
    public const int LeakedNativeObject = 3010;

    /// <summary>
    ///     Occurs when rendering objects are incorrectly disposed and their buffers are not deleted. This causes memory leaks.
    /// </summary>
    public const int UndeletedBuffers = 3011; // todo: check if this is still used

    /// <summary>
    ///     Occurs when textures are incorrectly disposed and their storage is not deleted. This causes memory leak.
    /// </summary>
    public const int UndeletedTexture = 3012; // todo: check if this is still used

    /// <summary>
    ///     Occurs when objects are incorrectly disposed and their OpenGL objects are not deleted. This causes memory leak.
    /// </summary>
    public const int UndeletedGlObjects = 3013; // todo: check if this is still used

    /// <summary>
    ///     Occurs when a screenshot is taken.
    /// </summary>
    public const int Screenshot = 3150; // todo: use this

    /// <summary>
    ///     Occurs when DirectX issues a debug message.
    /// </summary>
    public const int DirectXDebug = 3500;

    #endregion RENDERING AND VISUALS
}
