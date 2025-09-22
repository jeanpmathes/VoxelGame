// <copyright file="LateInitializationGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Microsoft.CodeAnalysis;
using VoxelGame.Annotations;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
/// Generates a non-nullable property over a *nullable* field marked with <see cref="LateInitializationAttribute"/>.
/// </summary>
[Generator]
public sealed class LateInitializationGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
            
    }
}
