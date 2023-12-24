// <copyright file="Concepts.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

template <typename T>
concept UnsignedNativeSizedInteger = requires(T x, size_t y)
{
    static_cast<size_t>(x);
    static_cast<T>(y);
    sizeof(T) == sizeof(size_t);
};

template <typename T>
concept Nullable = requires(T t)
{
    { t == nullptr } -> std::convertible_to<bool>;
    { t != nullptr } -> std::convertible_to<bool>;
    t = nullptr;
};
