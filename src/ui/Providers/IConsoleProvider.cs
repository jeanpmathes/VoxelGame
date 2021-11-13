﻿// <copyright file="IConsoleProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.UI.Providers
{
    public interface IConsoleProvider
    {
        (string response, bool isError) ProcessInput(string input);
    }
}