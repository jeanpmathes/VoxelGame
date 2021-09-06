// <copyright file="KeyMap.cs" company="VoxelGame">
//    MIT License
//    For full license see the repository.
// </copyright>
//<author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Collections
{
    public class KeyMap
    {
        private readonly Dictionary<KeyOrButton, int> usageCount = new Dictionary<KeyOrButton, int>();

        public bool AddBinding(KeyOrButton keyOrButton)
        {
            var unused = true;

            if (usageCount.ContainsKey(keyOrButton))
            {
                unused = false;
            }
            else
            {
                usageCount.Add(keyOrButton, 0);
            }

            usageCount[keyOrButton]++;

            return unused;
        }

        public void RemoveBinding(KeyOrButton keyOrButton)
        {
            usageCount[keyOrButton]--;

            if (usageCount[keyOrButton] == 0)
            {
                usageCount.Remove(keyOrButton);
            }
        }

        public int GetUsageCount(KeyOrButton keyOrButton)
        {
            if (!usageCount.TryGetValue(keyOrButton, out int count))
            {
                count = 0;
            }

            return count;
        }
    }
}
