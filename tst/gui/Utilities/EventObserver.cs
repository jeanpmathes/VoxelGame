// <copyright file="EventObserver.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.GUI.Tests.Utilities;

public sealed class EventObserver
{
    public Int32 InvocationCount { get; private set; }

    public Object? LastSender { get; private set; }
    public Object? LastArgs { get; private set; }

    public void Reset()
    {
        InvocationCount = 0;
        LastSender = null;
        LastArgs = null;
    }

    public void OnEvent(Object? sender, EventArgs args)
    {
        LastSender = sender;
        LastArgs = args;

        InvocationCount++;
    }

    public void OnAction(Object? args)
    {
        LastSender = null;
        LastArgs = args;

        InvocationCount++;
    }

    public void OnAction()
    {
        LastSender = null;
        LastArgs = null;

        InvocationCount++;
    }
}
