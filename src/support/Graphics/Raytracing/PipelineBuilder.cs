// <copyright file="PipelineBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Graphics.Raytracing;

/// <summary>
///     Helps with initialization of the raytracing pipeline.
/// </summary>
public class PipelineBuilder
{
    private readonly List<MaterialConfig> materials = new();
    private readonly List<ShaderFile> shaderFiles = new();

    private TextureArray? firstTextureSlot;
    private TextureArray? secondTextureSlot;

    /// <summary>
    ///     Add a shader file to the pipeline.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="exports">The exports in the file.</param>
    public void AddShaderFile(FileInfo file, string[] exports)
    {
        shaderFiles.Add(new ShaderFile(file, exports));
    }

    /// <summary>
    ///     Add a material to the pipeline.
    /// </summary>
    /// <param name="name">The name of the material, for debugging purposes.</param>
    /// <param name="closestHitSymbol">The name of the symbol for the closest hit shader.</param>
    /// <param name="shadowSymbol">The name of the symbol for the shadow (closest hit) shader.</param>
    /// <returns>The material.</returns>
    public Material AddMaterial(string name, string closestHitSymbol, string shadowSymbol)
    {
        int index = materials.Count;
        materials.Add(new MaterialConfig(name, closestHitSymbol, shadowSymbol));

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
    ///     Build the pipeline.
    /// </summary>
    /// <param name="client">The client that will use the pipeline.</param>
    /// <param name="loadingContext">The loading context, used to report shader compilation and loading errors.</param>
    public bool Build(Client client, LoadingContext loadingContext)
    {
        (ShaderFileDescription[] files, string[] symbols, MaterialDescription[] materialDescriptions, IntPtr[] texturePointers) = BuildDescriptions();

        var success = true;

        client.InitializeRaytracing(new SpacePipeline
        {
            ShaderFiles = files,
            Symbols = symbols,
            Materials = materialDescriptions,
            TexturePointers = texturePointers,
            Description = new SpacePipelineDescription
            {
                shaderCount = (uint) files.Length,
                materialCount = (uint) materialDescriptions.Length,
                textureCountFirstSlot = firstTextureSlot?.PartCount ?? 0,
                textureCountSecondSlot = secondTextureSlot?.PartCount ?? 0,
                onShaderLoadingError = message =>
                {
                    ReportFailure(loadingContext, message);
                    success = false;
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
            debugName = material.Name,
            closestHitSymbol = material.ClosestHitSymbol,
            shadowHitSymbol = material.ShadowHitSymbol
        }).ToArray();

        IEnumerable<IntPtr> firstSlot = firstTextureSlot?.GetTexturePointers() ?? Enumerable.Empty<IntPtr>();
        IEnumerable<IntPtr> secondSlot = secondTextureSlot?.GetTexturePointers() ?? Enumerable.Empty<IntPtr>();

        return (shaderFileDescriptions.ToArray(), symbols.ToArray(), materialDescriptions, firstSlot.Concat(secondSlot).ToArray());
    }

    private static void ReportFailure(LoadingContext loadingContext, string message)
    {
        loadingContext.ReportFailure(Events.ShaderError, nameof(SpacePipeline), "RT_Pipeline", message);
    }

    private void ReportSuccess(LoadingContext loadingContext)
    {
        foreach (ShaderFile shader in shaderFiles) loadingContext.ReportSuccess(Events.ShaderSetup, nameof(SpacePipeline), shader.File);
    }

    private sealed record ShaderFile(FileInfo File, string[] Exports);

    private sealed record MaterialConfig(string Name, string ClosestHitSymbol, string ShadowHitSymbol);
}
