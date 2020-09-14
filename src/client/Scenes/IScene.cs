// <copyright file="IScene.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Client.Scenes
{
    /// <summary>
    /// An object that manages UI and update receivers.
    /// </summary>
    public interface IScene : IDisposable
    {
        void Load();

        void Update(float deltaTime);

        void Render(float deltaTime);

        void OnResize(Vector2i size);
    }
}