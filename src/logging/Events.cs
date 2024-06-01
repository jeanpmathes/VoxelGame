// <copyright file="LoggingEvents.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Logging;

/// <summary>
///     Event IDs for important logging events.
/// </summary>
public static class Events
{
    /// <summary>
    ///     The default ID, which is also used if no ID is given.
    /// </summary>
    public const Int32 Default = 0;

    /// <summary>
    ///     Events related to the logging system itself.
    /// </summary>
    public const Int32 Meta = 60000;

    #region GENERAL APPLICATION

    /// <summary>
    ///     Information related to the application.
    /// </summary>
    public const Int32 ApplicationInformation = 1;

    /// <summary>
    ///     Information related to the application settings.
    /// </summary>
    public const Int32 ApplicationSettings = 2;

    /// <summary>
    ///     Events that indicate a change of the general application state.
    /// </summary>
    public const Int32 ApplicationState = 10;

    /// <summary>
    ///     Occurs when the state of the application window has changed.
    /// </summary>
    public const Int32 WindowState = 11;

    /// <summary>
    ///     Events related to OS interactions.
    /// </summary>
    public const Int32 OS = 20;

    /// <summary>
    ///     Events related to the integrated profiler.
    /// </summary>
    public const Int32 Profiling = 30;

    /// <summary>
    ///     Events related to file system operations.
    /// </summary>
    public const Int32 FileIO = 50;

    /// <summary>
    ///     Events related to the scene system.
    /// </summary>
    public const Int32 Scene = 60;

    /// <summary>
    ///     Events related to the in-game console.
    /// </summary>
    public const Int32 Console = 70;

    /// <summary>
    ///     Events related to the disposal of objects.
    /// </summary>
    public const Int32 Dispose = 99;

    /// <summary>
    ///     Occurs with events or information connected to the successful loading of resources.
    /// </summary>
    public const Int32 ResourceLoad = 100;

    /// <summary>
    ///     Occurs with events or information connected to the successful loading of creations.
    ///     Creations are user-made resources that are loaded during an active game session.
    /// </summary>
    public const Int32 CreationLoad = 101;

    /// <summary>
    ///     Occurs when a texture or model that is requested could not be loaded and a fallback is used.
    /// </summary>
    public const Int32 MissingResource = 110;
    
    /// <summary>
    ///     Occurs when a creation is not available or could not be loaded.
    /// </summary>
    public const Int32 MissingCreation = 111;

    /// <summary>
    ///     A general event category for everything input related.
    /// </summary>
    public const Int32 InputSystem = 200;

    /// <summary>
    ///     Occurs when a key bind is registered or the binding is changed.
    /// </summary>
    public const Int32 SetKeyBind = 210;

    /// <summary>
    ///     Occurs when the active scene is changed.
    /// </summary>
    public const Int32 SceneChange = 300;

    #endregion GENERAL APPLICATION

    #region WORLD LOGIC

    /// <summary>
    ///     Events indicating a change of the world state.
    /// </summary>
    public const Int32 WorldState = 1000;

    /// <summary>
    ///     Events related to saving and loading worlds.
    /// </summary>
    public const Int32 WorldIO = 1001;

    /// <summary>
    ///     An error that occurs when saving the world.
    /// </summary>
    public const Int32 WorldSavingError = 1010;

    /// <summary>
    ///     An error that occurs when loading a world, e.g. when the meta file is damaged.
    /// </summary>
    public const Int32 WorldLoadingError = 1011;

    /// <summary>
    ///     An error that occurs when saving a chunk.
    /// </summary>
    public const Int32 ChunkSavingError = 1012;

    /// <summary>
    ///     An error that occurs when loading a chunk.
    /// </summary>
    public const Int32 ChunkLoadingError = 1013;

    /// <summary>
    ///     An error that occurs when meshing a chunk.
    /// </summary>
    public const Int32 ChunkMeshingError = 1014;

    /// <summary>
    ///     Different chunk operations like loading, saving or generating.
    /// </summary>
    public const Int32 ChunkOperation = 1030;

    /// <summary>
    ///     Occurs when a chunk is requested.
    /// </summary>
    public const Int32 ChunkRequest = 1031;

    /// <summary>
    ///     Occurs when a chunk is released.
    /// </summary>
    public const Int32 ChunkRelease = 1032;

    /// <summary>
    ///     Events related to world generation.
    /// </summary>
    public const Int32 WorldGeneration = 1060;

    /// <summary>
    ///     Any event concerning the physics system.
    /// </summary>
    public const Int32 PhysicsSystem = 1200;
    
    /// <summary>
    ///     Any event concerning general simulation systems.
    /// </summary>
    public const Int32 Simulation = 1201;
    
    /// <summary>
    ///     Any event related to player actions.
    /// </summary>
    public const Int32 Player = 1400;

    #endregion WORLD LOGIC

    #region CONTENT
    
    /// <summary>
    ///     Occurs when stored block information refers to unknown blocks.
    /// </summary>
    public const Int32 UnknownBlock = 2000;
    
    /// <summary>
    ///     Occurs when stored block information refers to unknown blocks.
    /// </summary>
    public const Int32 UnknownFluid = 2500;

    #endregion CONTENT

    #region RENDERING AND VISUALS
    
    /// <summary>
    ///     Events related to the render pipeline and other graphics-related operations.
    /// </summary>
    public const Int32 Graphics = 3000;

    /// <summary>
    ///     Occurs when a screenshot is taken.
    /// </summary>
    public const Int32 Screenshot = 3150;

    /// <summary>
    ///     Occurs when DirectX issues a debug message.
    /// </summary>
    public const Int32 DirectXDebug = 3500;

    #endregion RENDERING AND VISUALS
}
