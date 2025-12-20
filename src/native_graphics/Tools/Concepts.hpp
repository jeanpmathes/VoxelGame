// <copyright file="Concepts.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

#pragma once

template <typename T>concept UnsignedNativeSizedInteger = requires(T x, size_t y)
{
    static_cast<size_t>(x); static_cast<T>(y); sizeof(T) == sizeof(size_t);
};

template <typename T>concept Nullable = requires(T t)
{
    { t == nullptr } -> std::convertible_to<bool>; { t != nullptr } -> std::convertible_to<bool>; t = nullptr;
};
