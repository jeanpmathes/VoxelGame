// <copyright file="KeyButtonPair.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input.Internal
{
    [Serializable]
    public class KeyButtonPair
    {
        public bool Default { get; set; }

        public Key Key { get; set; } = Key.Unknown;
        public MouseButton Button { get; set; }

        public static KeyButtonPair DefaultValue => new KeyButtonPair {Default = true};
    }
}