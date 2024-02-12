// <copyright file="PipelineBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.InteropServices;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Graphics.Raytracing;

/// <summary>
///     Helps with initialization of the raytracing pipeline.
/// </summary>
public class PipelineBuilder
{
    /// <summary>
    ///     Groups in which objects with a material can be.
    /// </summary>
    [Flags]
    public enum Groups
    {
        /// <summary>
        ///     The group of objects that are visible.
        /// </summary>
        Visible = 1 << 0,

        /// <summary>
        ///     The group of objects that cast shadows.
        /// </summary>
        ShadowCaster = 1 << 1,

        /// <summary>
        ///     The default group.
        /// </summary>
        Default = Visible | ShadowCaster,

        /// <summary>
        ///     The group of objects that do not cast shadows but are otherwise like <see cref="Default" />.
        /// </summary>
        NoShadow = Visible
    }

    private readonly List<MaterialConfig> materials = new();
    private readonly List<ShaderFile> shaderFiles = new();

    private TextureArray? firstTextureSlot;
    private TextureArray? secondTextureSlot;

    private uint customDataBufferSize;

    /// <summary>
    ///     Add a shader file to the pipeline.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="groups">The hit groups in the file.</param>
    /// <param name="names">The ungrouped symbols in the file.</param>
    public void AddShaderFile(FileInfo file, HitGroup[]? groups = null, string[]? names = null)
    {
        List<string> exports = new(names ?? Array.Empty<string>());

        void AddIfNotEmpty(string? name)
        {
            if (!string.IsNullOrEmpty(name)) exports.Add(name);
        }

        foreach (HitGroup group in groups ?? Array.Empty<HitGroup>())
        {
            AddIfNotEmpty(group.ClosestHitSymbol);
            AddIfNotEmpty(group.AnyHitSymbol);
            AddIfNotEmpty(group.IntersectionSymbol);
        }

        shaderFiles.Add(new ShaderFile(file, exports.ToArray()));
    }

    /// <summary>
    ///     Add an animation shader to the pipeline.
    /// </summary>
    /// <param name="file">The file defining the animation.</param>
    /// <returns>The animation.</returns>
    public Animation AddAnimation(FileInfo file)
    {
        AddShaderFile(file);

        return new Animation((uint) shaderFiles.Count - 1);
    }

    private static string CleanUpName(string name)
    {
        return name.Replace(nameof(Material), "", StringComparison.InvariantCulture);
    }

    /// <summary>
    ///     Add a material to the pipeline.
    /// </summary>
    /// <param name="name">The name of the material, for debugging purposes.</param>
    /// <param name="groups">The groups in which objects with this material should be.</param>
    /// <param name="isOpaque">Whether the material is opaque.</param>
    /// <param name="normal">The hit group for normal rendering.</param>
    /// <param name="shadow">The hit group for shadows.</param>
    /// <param name="animation">An optional animation to be executed before the raytracing.</param>
    /// <returns>The material.</returns>
    public Material AddMaterial(string name, Groups groups, bool isOpaque, HitGroup normal, HitGroup shadow, Animation? animation = null)
    {
        int index = materials.Count;

        materials.Add(new MaterialConfig(CleanUpName(name), groups, isOpaque, animation?.ShaderFileIndex, normal, shadow));

        return new Material((uint) index);
    }

    /// <summary>
    ///     Set which textures should be used in the first texture slot.
    /// </summary>
    /// <param name="texture">The texture array.</param>
    public void SetFirstTextureSlot(TextureArray texture)
    {
        firstTextureSlot = texture;
    }

    /// <summary>
    ///     Set which textures should be used in the second texture slot.
    /// </summary>
    /// <param name="texture">The texture array.</param>
    public void SetSecondTextureSlot(TextureArray texture)
    {
        secondTextureSlot = texture;
    }

    /// <summary>
    ///     Set the type of the custom data buffer.
    ///     Using this will enable the creation of a custom data buffer.
    /// </summary>
    /// <typeparam name="T">The type of the custom data buffer.</typeparam>
    public void SetCustomDataBufferType<T>() where T : unmanaged
    {
        customDataBufferSize = (uint) Marshal.SizeOf<T>();
    }

    /// <summary>
    ///     Build the pipeline, without a custom data buffer.
    /// </summary>
    /// <param name="client">The client that will use the pipeline.</param>
    /// <param name="loadingContext">The loading context, used to report shader compilation and loading errors.</param>
    public bool Build(Client client, LoadingContext loadingContext)
    {
        Debug.Assert(customDataBufferSize == 0);

        return Build<byte>(client, loadingContext, out _);
    }

    /// <summary>
    ///     Build the pipeline.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the custom data buffer, must be the same as provided in
    ///     <see cref="SetCustomDataBufferType{T}" />.
    /// </typeparam>
    /// <param name="client">The client that will use the pipeline.</param>
    /// <param name="loadingContext">The loading context, used to report shader compilation and loading errors.</param>
    /// <param name="buffer">Will be set to the created buffer if the pipeline produced one.</param>
    public bool Build<T>(Client client, LoadingContext loadingContext, out ShaderBuffer<T>? buffer) where T : unmanaged, IEquatable<T>
    {
        (ShaderFileDescription[] files, string[] symbols, MaterialDescription[] materialDescriptions, IntPtr[] texturePointers) = BuildDescriptions();

        Debug.Assert((customDataBufferSize > 0).Implies(Marshal.SizeOf<T>() == customDataBufferSize));

        var success = true;

        buffer = client.InitializeRaytracing<T>(new SpacePipeline
        {
            ShaderFiles = files,
            Symbols = symbols,
            Materials = materialDescriptions,
            TexturePointers = texturePointers,
            Description = new SpacePipelineDescription
            {
                shaderCount = (uint) files.Length,
                materialCount = (uint) materialDescriptions.Length,
                textureCountFirstSlot = (uint) (firstTextureSlot?.Count ?? 0),
                textureCountSecondSlot = (uint) (secondTextureSlot?.Count ?? 0),
                customDataBufferSize = customDataBufferSize,
                onShaderLoadingError = (_, message) =>
                {
                    ReportFailure(loadingContext, message);
                    success = false;

                    Debugger.Break();
                }
            }
        });

        if (!success) return false;

        ReportSuccess(loadingContext);

        return true;
    }

    private (ShaderFileDescription[], string[], MaterialDescription[], IntPtr[]) BuildDescriptions()
    {
        List<string> symbols = new();
        List<ShaderFileDescription> shaderFileDescriptions = new();

        foreach (ShaderFile shaderFile in shaderFiles)
        {
            symbols.AddRange(shaderFile.Exports);

            shaderFileDescriptions.Add(new ShaderFileDescription
            {
                path = shaderFile.File.FullName,
                symbolCount = (uint) shaderFile.Exports.Length
            });
        }

        MaterialDescription[] materialDescriptions = materials.Select(material => new MaterialDescription
        {
            name = material.Name,
            isVisible = material.Groups.HasFlag(Groups.Visible),
            isShadowCaster = material.Groups.HasFlag(Groups.ShadowCaster),
            isOpaque = material.IsOpaque,
            isAnimated = material.Animation.HasValue,
            animationShaderIndex = material.Animation ?? 0,
            normalClosestHitSymbol = material.Normal.ClosestHitSymbol,
            normalAnyHitSymbol = material.Normal.AnyHitSymbol,
            normalIntersectionSymbol = material.Normal.IntersectionSymbol,
            shadowClosestHitSymbol = material.Shadow.ClosestHitSymbol,
            shadowAnyHitSymbol = material.Shadow.AnyHitSymbol,
            shadowIntersectionSymbol = material.Shadow.IntersectionSymbol
        }).ToArray();

        IEnumerable<IntPtr> firstSlot = firstTextureSlot?.GetTexturePointers() ?? Enumerable.Empty<IntPtr>();
        IEnumerable<IntPtr> secondSlot = secondTextureSlot?.GetTexturePointers() ?? Enumerable.Empty<IntPtr>();

        return (shaderFileDescriptions.ToArray(), symbols.ToArray(), materialDescriptions, firstSlot.Concat(secondSlot).ToArray());
    }

    private static void ReportFailure(LoadingContext loadingContext, string message)
    {
        loadingContext.ReportFailure(Events.RenderPipelineError, nameof(SpacePipeline), "RT_Pipeline", message);
    }

    private void ReportSuccess(LoadingContext loadingContext)
    {
        foreach (ShaderFile shader in shaderFiles) loadingContext.ReportSuccess(Events.RenderPipelineSetup, nameof(SpacePipeline), shader.File);
    }

    private sealed record ShaderFile(FileInfo File, string[] Exports);

    private sealed record MaterialConfig(string Name, Groups Groups, bool IsOpaque, uint? Animation, HitGroup Normal, HitGroup Shadow);

    /// <summary>
    ///     Defines a hit group which is a combination of shaders that are executed when a ray hits a geometry.
    /// </summary>
    /// <param name="ClosestHitSymbol">The name of the closest hit shader.</param>
    /// <param name="AnyHitSymbol">The name of the any hit shader, or empty if there is none.</param>
    /// <param name="IntersectionSymbol">The name of the intersection shader, or empty if there is none.</param>
    public sealed record HitGroup(string ClosestHitSymbol, string AnyHitSymbol = "", string IntersectionSymbol = "");

    /// <summary>
    ///     Defines an animation, which is a compute shader that is executed before the raytracing.
    /// </summary>
    /// <param name="ShaderFileIndex">The index of the shader file that contains the animation.</param>
    public sealed record Animation(uint ShaderFileIndex);
}
