﻿// <copyright file="IScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Client.Scenes
{
    /// <summary>
    /// The base interface for all scenes.
    /// </summary>
    public interface IScene : IDisposable
    {
        /// <summary>
        ///     Load the scene.
        /// </summary>
        void Load();

        /// <summary>
        ///     Perform an update cycle.
        /// </summary>
        /// <param name="deltaTime">The time since the last update.</param>
        void Update(float deltaTime);

        /// <summary>
        ///     Handle a game resize.
        /// </summary>
        /// <param name="size">The new size.</param>
        void OnResize(Vector2i size);

        /// <summary>
        ///     Perform a render cycle.
        /// </summary>
        /// <param name="deltaTime">The time since the last render.</param>
        void Render(float deltaTime);

        /// <summary>
        ///     Unload this scene.
        /// </summary>
        void Unload();
    }
}