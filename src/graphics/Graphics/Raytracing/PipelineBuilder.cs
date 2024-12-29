// <copyright file="PipelineBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Graphics.Raytracing;

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

    private readonly List<MaterialConfig> materials = [];
    private readonly List<ShaderFile> shaderFiles = [];

    private TextureArray? firstTextureSlot;
    private TextureArray? secondTextureSlot;

    private UInt32 customDataBufferSize;

    private UInt32 meshSpoolCount;
    private UInt32 effectSpoolCount;

    /// <summary>
    ///     Add a shader file to the pipeline.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="groups">The hit groups in the file.</param>
    /// <param name="names">The ungrouped symbols in the file.</param>
    public void AddShaderFile(FileInfo file, HitGroup[]? groups = null, String[]? names = null)
    {
        List<String> exports = [..names ?? []];

        void AddIfNotEmpty(String? name)
        {
            if (!String.IsNullOrEmpty(name)) exports.Add(name);
        }

        foreach (HitGroup group in groups ?? [])
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

        return new Animation((UInt32) (shaderFiles.Count - 1));
    }

    private static String CleanUpName(String name)
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
    public Material AddMaterial(String name, Groups groups, Boolean isOpaque, HitGroup normal, HitGroup shadow, Animation? animation = null)
    {
        Int32 index = materials.Count;

        materials.Add(new MaterialConfig(CleanUpName(name), groups, isOpaque, animation?.ShaderFileIndex, normal, shadow));

        return new Material((UInt32) index);
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
        customDataBufferSize = (UInt32) Marshal.SizeOf<T>();
    }

    /// <summary>
    ///     Set the number of instances to be spooled up for meshes and effects initially.
    /// </summary>
    /// <param name="mesh">The number of mesh instances to spool up.</param>
    /// <param name="effect">The number of effect instances to spool up.</param>
    public void SetSpoolCounts(UInt32 mesh, UInt32 effect)
    {
        meshSpoolCount = mesh;
        effectSpoolCount = effect;
    }

    /// <summary>
    ///     Build the pipeline, without a custom data buffer.
    /// </summary>
    /// <param name="client">The client that will use the pipeline.</param>
    /// <param name="context">The context in which loading is happening.</param>
    /// <returns>An error, if any.</returns>
    public ResourceIssue? Build(Client client, IResourceContext context)
    {
        Debug.Assert(customDataBufferSize == 0);

        return Build<Byte>(client, context, out _);
    }

    /// <summary>
    ///     Build the pipeline.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the custom data buffer, must be the same as provided in
    ///     <see cref="SetCustomDataBufferType{T}" />.
    /// </typeparam>
    /// <param name="client">The client that will use the pipeline.</param>
    /// <param name="context">The context in which loading is happening.</param>
    /// <param name="buffer">Will be set to the created buffer if the pipeline produced one.</param>
    /// <returns>An error, if any.</returns>
    public ResourceIssue? Build<T>(Client client, IResourceContext context, out ShaderBuffer<T>? buffer) where T : unmanaged, IEquatable<T>
    {
        (ShaderFileDescription[] files, String[] symbols, MaterialDescription[] materialDescriptions, Texture[] textures) = BuildDescriptions();

        Debug.Assert((customDataBufferSize > 0).Implies(Marshal.SizeOf<T>() == customDataBufferSize));

        StringBuilder errors = new();
        var anyError = false;

        buffer = client.InitializeRaytracing<T>(new SpacePipelineDescription
        {
            shaderFiles = files,
            symbols = symbols,
            materials = materialDescriptions,
            textures = textures,
            textureCountFirstSlot = (UInt32) (firstTextureSlot?.Count ?? 0),
            textureCountSecondSlot = (UInt32) (secondTextureSlot?.Count ?? 0),
            customDataBufferSize = customDataBufferSize,
            meshSpoolCount = meshSpoolCount,
            effectSpoolCount = effectSpoolCount,
            onShaderLoadingError = (_, message) =>
            {
                errors.AppendLine(message);
                anyError = true;

                Debugger.Break();
            }
        });

        if (!anyError)
            return ResourceIssue.FromMessage(Level.Error, errors.ToString());

        foreach (ShaderFile shader in shaderFiles)
            context.ReportDiscovery(ResourceTypes.Shader, RID.Path(shader.File));

        return null;
    }

    private (ShaderFileDescription[], String[], MaterialDescription[], Texture[]) BuildDescriptions()
    {
        List<String> symbols = [];
        List<ShaderFileDescription> shaderFileDescriptions = [];

        foreach (ShaderFile shaderFile in shaderFiles)
        {
            symbols.AddRange(shaderFile.Exports);

            shaderFileDescriptions.Add(new ShaderFileDescription
            {
                path = shaderFile.File.FullName,
                symbolCount = (UInt32) shaderFile.Exports.Length
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

        IEnumerable<Texture> firstSlot = firstTextureSlot ?? Enumerable.Empty<Texture>();
        IEnumerable<Texture> secondSlot = secondTextureSlot ?? Enumerable.Empty<Texture>();

        return (shaderFileDescriptions.ToArray(), symbols.ToArray(), materialDescriptions, firstSlot.Concat(secondSlot).ToArray());
    }

    private sealed record ShaderFile(FileInfo File, String[] Exports);

    private sealed record MaterialConfig(String Name, Groups Groups, Boolean IsOpaque, UInt32? Animation, HitGroup Normal, HitGroup Shadow);

    /// <summary>
    ///     Defines a hit group which is a combination of shaders that are executed when a ray hits a geometry.
    /// </summary>
    /// <param name="ClosestHitSymbol">The name of the closest hit shader.</param>
    /// <param name="AnyHitSymbol">The name of the any-hit shader, or empty if there is none.</param>
    /// <param name="IntersectionSymbol">The name of the intersection shader, or empty if there is none.</param>
    public sealed record HitGroup(String ClosestHitSymbol, String AnyHitSymbol = "", String IntersectionSymbol = "");

    /// <summary>
    ///     Defines an animation, which is a compute shader that is executed before the raytracing.
    /// </summary>
    /// <param name="ShaderFileIndex">The index of the shader file that contains the animation.</param>
    public sealed record Animation(UInt32 ShaderFileIndex);
}
