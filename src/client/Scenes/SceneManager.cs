// <copyright file="SceneManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Client.Scenes
{
    internal class SceneManager
    {
        private IScene? current;

        public void Load(in IScene scene)
        {
            Unload();

            current = scene;

            Load();
        }

        private void Load()
        {
            current?.Load();
        }

        public void Unload()
        {
            if (current == null) return;

            current.Unload();
            current.Dispose();
        }

        public void Render(float deltaTime)
        {
            current?.Render(deltaTime);
        }

        public void OnResize(Vector2i size)
        {
            current?.OnResize(size);
        }

        public void Update(float deltaTime)
        {
            current?.Update(deltaTime);
        }
    }
}