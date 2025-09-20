// <copyright file="Attribution.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Gwen.Net.RichText;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI;

/// <summary>
///     An attribution text.
/// </summary>
public sealed class Attribution : IResource, ILocated
{
    /// <summary>
    ///     The resource type of all attributions.
    /// </summary>
    public static readonly ResourceType ResourceType = ResourceTypes.Text;

    /// <summary>
    ///     Creates a new attribution.
    /// </summary>
    /// <param name="identifier">The identifier of the attribution.</param>
    /// <param name="name">The name of the attribution.</param>
    /// <param name="text">The text of the attribution.</param>
    public Attribution(RID identifier, String name, String text)
    {
        Identifier = identifier;

        Name = name;
        Text = text;
    }

    /// <summary>
    ///     The name of the attribution.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     The text of the attribution.
    /// </summary>
    public String Text { get; }

    /// <inheritdoc />
    public static String[] Path { get; } = ["Attribution"];

    /// <inheritdoc />
    public static String FileExtension => "txt";

    /// <inheritdoc />
    public RID Identifier { get; }

    /// <inheritdoc />
    public ResourceType Type => ResourceType;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Creates a document from the attribution.
    /// </summary>
    /// <param name="context">The UI context.</param>
    /// <returns>The document and the name of the attribution.</returns>
    internal (Document document, String name) CreateDocument(Context context)
    {
        Document credits = new();

        Paragraph paragraph = new Paragraph()
            .Font(context.Fonts.Title).Text(Name).LineBreak().LineBreak()
            .Font(context.Fonts.Default)
            .Text(Text).LineBreak();

        credits.Paragraphs.Add(paragraph);

        return (credits, Name);
    }
}
