// <copyright file="CombinationMap.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Collections
{
    public class CombinationMap<TE, TV> where TE : IIdentifiable<uint>
    {
        private readonly bool[][] flags;
        private readonly TV[][] table;

        public CombinationMap(int range)
        {
            flags = new bool[range][];
            table = new TV[range][];

            for (var i = 0; i < range; i++)
            {
                flags[i] = new bool[i];
                table[i] = new TV[i];
            }
        }

        public void AddCombination(TE e, TV v, params TE[] others)
        {
            foreach (TE other in others)
            {
                this[e, other] = v;
            }
        }

        public TV Resolve(TE a, TE b)
        {
            return this[a, b];
        }

        private TV this[TE a, TE b]
        {
            get
            {
                Debug.Assert(a.Id != b.Id);

                var i = (int) Math.Max(a.Id, b.Id);
                var j = (int) Math.Min(a.Id, b.Id);

                return table[i][j];
            }

            set
            {
                Debug.Assert(a.Id != b.Id);

                var i = (int) Math.Max(a.Id, b.Id);
                var j = (int) Math.Min(a.Id, b.Id);

                Debug.Assert(!flags[i][j], "This combination is already set.");

                table[i][j] = value;
            }
        }
    }
}