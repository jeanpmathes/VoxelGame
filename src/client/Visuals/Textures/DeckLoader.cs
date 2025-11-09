// <copyright file="DeckLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenTK.Mathematics;
using VoxelGame.Client.Visuals.Textures.Combinators;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Loads decks, combining their layers and applying their modifiers.
/// </summary>
public class DeckLoader : IIssueSource
{
    private readonly Dictionary<String, Combinator> combinators = new();

    private readonly Combinator defaultCombinator = new Blend();
    private readonly Dictionary<String, Modifier> modifiers = new();

    /// <summary>
    ///     The image library to add the decks to and retrieve images from.
    /// </summary>
    public required ImageLibrary Library { get; init; }

    /// <summary>
    ///     The resource context in which the decks are loaded.
    /// </summary>
    public required IResourceContext Context { get; init; }

    /// <summary>
    ///     Initialize the deck loader.
    /// </summary>
    public void Initialize()
    {
        LoadModifiers();
        LoadCombinators();
    }

    private void LoadModifiers()
    {
        foreach (Modifier modifier in Reflections.GetSubclassInstances<Modifier>())
        {
            Boolean added = modifiers.TryAdd(modifier.Type, modifier);

            if (added)
                Context.ReportDiscovery(ResourceTypes.Modifier, RID.Named<Modifier>(modifier.Type));
            else
                Context.ReportWarning(modifier, "Duplicate modifier type");
        }
    }

    private void LoadCombinators()
    {
        foreach (Combinator combinator in Reflections.GetSubclassInstances<Combinator>())
        {
            Boolean added = combinators.TryAdd(combinator.Type, combinator);

            if (added)
                Context.ReportDiscovery(ResourceTypes.Combinator, RID.Named<Combinator>(combinator.Type));
            else
                Context.ReportWarning(combinator, "Duplicate combinator type");
        }
    }

    /// <summary>
    ///     Load all decks in the given files.
    /// </summary>
    /// <param name="files">The files to load the decks from.</param>
    public void LoadDecks(IEnumerable<FileInfo> files)
    {
        List<Deck> decks = files.Select(CreateDeck).WhereNotNull().ToList();

        IEnumerable<Deck> workload = decks;
        List<(Deck decks, String[] missing)>? unresolvable = null;

        Int32 unresolved;

        do
        {
            unresolved = unresolvable?.Count ?? decks.Count;
            unresolvable = ProcessDecks(workload);
            workload = unresolvable.Select(e => e.decks);
        } while (unresolvable.Count < unresolved);

        if (unresolvable.Count <= 0) return;

        foreach ((Deck deck, String[] missing) in unresolvable)
            Context.ReportWarning(this,
                $"Unresolvable deck {ImageLibrary.GetName(deck.File)}, missing: {String.Join(", ", missing)}",
                path: deck.File);
    }

    private List<(Deck decks, String[] missing)> ProcessDecks(IEnumerable<Deck> decks)
    {
        List<(Deck decks, String[] missing)> unresolvable = [];

        foreach (Deck deck in decks)
        {
            ResolvedDeck? resolved = ResolveDeck(deck, out String[] missingCombinators, out String[] missingModifiers, out String[] missingSources);

            ReportWarnings(deck, missingCombinators, missingModifiers);

            if (missingSources.Length > 0)
            {
                // Missing source might be another deck, so trying later could resolve this.
                unresolvable.Add((deck, missingSources));
            }
            else if (resolved != null)
            {
                Sheet? sheet = BuildDeck(resolved, deck.File);

                if (sheet == null)
                    continue;

                Boolean added = Library.AddSheet(deck.File, sheet, part: false);

                if (!added)
                    Context.ReportWarning(this, $"Name '{ImageLibrary.GetName(deck.File)}' is already in use", path: deck.File);
            }
        }

        return unresolvable;
    }

    private Sheet? BuildDeck(ResolvedDeck deck, FileInfo file)
    {
        CombinatorContext combinatorContext = new(deck.Combinator, file, Context);
        ModifierContext modifierContext = new(file, Context);

        ResolvedLayer first = deck.Layers.First();
        ResolvedLayer[] rest = deck.Layers.Skip(count: 1).ToArray();

        Sheet? currentSheet = BuildLayer(first, combinatorContext, modifierContext);

        if (currentSheet == null)
            return null;

        foreach (ResolvedLayer layer in rest)
        {
            if (currentSheet == null)
                return null;

            Sheet? newSheet = BuildLayer(layer, combinatorContext, modifierContext);

            if (newSheet == null)
                return null;

            currentSheet = deck.Combinator.Combine(currentSheet, newSheet, combinatorContext);
        }

        foreach (ResolvedModifier modifier in deck.Modifiers)
        {
            if (currentSheet == null)
                return null;

            Sheet[,]? modified = ApplyModifier(currentSheet, modifier, modifierContext, out (Byte w, Byte h)? dimensions);

            if (modified == null || dimensions == null)
            {
                modifierContext.ReportWarning(this,
                    $"Modifier '{modifier.Modifier.Type}' with index '{modifier.Index}' produced inconsistent dimensions");

                return null;
            }

            currentSheet = MergeSheets(modified, combinatorContext, (currentSheet.Width, currentSheet.Height), dimensions.Value);
        }

        return currentSheet;
    }

    private static Sheet[,]? ApplyModifier(Sheet source, ResolvedModifier modifier, ModifierContext context, out (Byte w, Byte h)? dimensions)
    {
        context.Modifier = modifier.Modifier;
        context.Size = (source.Width, source.Height);

        var results = new Sheet[source.Width, source.Height];
        dimensions = null;

        for (Byte x = 0; x < source.Width; x++)
        for (Byte y = 0; y < source.Height; y++)
        {
            context.Position = (x, y);

            Sheet? result = modifier.Modifier.Modify(source[x, y], modifier.Parameters, context);

            if (result == null)
                return null;

            dimensions ??= (result.Width, result.Height);

            if (dimensions != (result.Width, result.Height))
                return null;

            results[x, y] = result;
        }

        return results;
    }

    private Sheet? MergeSheets(Sheet[,] sheets, CombinatorContext combinatorContext, (Byte w, Byte h) sourceSize, (Byte w, Byte h) modifierSize)
    {
        Int32 width = sourceSize.w * modifierSize.w;
        Int32 height = sourceSize.h * modifierSize.h;

        if (!Sheet.IsSizeValid(width, height))
        {
            combinatorContext.ReportWarning(this,
                $"Resulting sheet size is invalid: {width}x{height}");

            return null;
        }

        Sheet merged = new((Byte) width, (Byte) height);

        for (Byte x = 0; x < width; x += modifierSize.w)
        for (Byte y = 0; y < height; y += modifierSize.h)
            merged.Place(sheets[x / modifierSize.w, y / modifierSize.h], x, y);

        return merged;
    }

    private Sheet? BuildLayer(ResolvedLayer layer, CombinatorContext combinatorContext, ModifierContext modifierContext)
    {
        Sheet? source = layer.Source;

        foreach (ResolvedModifier modifier in layer.Modifiers)
        {
            if (source == null)
                return null;

            Sheet[,]? modified = ApplyModifier(source, modifier, modifierContext, out (Byte w, Byte h)? modifierSize);

            if (modified == null || modifierSize == null)
            {
                modifierContext.ReportWarning(this,
                    $"Modifier '{modifier.Modifier.Type}' with index '{modifier.Index}' produced inconsistent dimensions");

                return null;
            }

            Int32 width = source.Width * modifierSize.Value.w;
            Int32 height = source.Height * modifierSize.Value.h;

            if (!Sheet.IsSizeValid(width, height))
            {
                modifierContext.ReportWarning(this,
                    $"Resulting sheet size in layer with index '{layer.Index}' is invalid: {width}x{height}");

                return null;
            }

            source = MergeSheets(modified, combinatorContext, (source.Width, source.Height), modifierSize.Value);
        }

        return source;
    }

    private void ReportWarnings(Deck deck, String[] missingCombinators, String[] missingModifiers)
    {
        if (missingCombinators.Length > 0)
            Context.ReportWarning(this, $"Missing combinators: {String.Join(", ", missingCombinators)}", path: deck.File);

        if (missingModifiers.Length > 0)
            Context.ReportWarning(this, $"Missing modifiers: {String.Join(", ", missingModifiers)}", path: deck.File);
    }

    private ResolvedDeck? ResolveDeck(Deck deck, out String[] missingCombinators, out String[] missingModifiers, out String[] missingSources)
    {
        missingCombinators = deck.Combinator is null || combinators.ContainsKey(deck.Combinator) ? [] : [deck.Combinator];

        String[] requiredModifiers = deck.Layers
            .SelectMany(layer => layer.Modifiers)
            .Concat(deck.Modifiers)
            .Select(modifier => modifier.Type)
            .ToArray();

        missingModifiers = requiredModifiers.Except(modifiers.Keys).ToArray();

        missingSources = [];

        if (missingCombinators.Length > 0 || missingModifiers.Length > 0)
            return null;

        String[] requiredSources = deck.Layers
            .Select(layer => layer.Source)
            .WhereNotNull()
            .ToArray();

        missingSources = requiredSources.Where(source => !Library.HasSheet(source)).ToArray();

        if (missingSources.Length > 0)
            return null;

        return new ResolvedDeck
        {
            Combinator = deck.Combinator is null ? defaultCombinator : combinators[deck.Combinator],
            Layers = deck.Layers.Select((layer, i) => new ResolvedLayer
            {
                Index = i,
                Source = Library.GetSheet(layer.Source)!,
                Modifiers = layer.Modifiers.Select((modifier, j) => new ResolvedModifier
                {
                    Index = j,
                    Modifier = modifiers[modifier.Type],
                    Parameters = modifier.Parameters
                }).ToArray()
            }).ToArray(),
            Modifiers = deck.Modifiers.Select((modifier, i) => new ResolvedModifier
            {
                Index = i,
                Modifier = modifiers[modifier.Type],
                Parameters = modifier.Parameters
            }).ToArray()
        };
    }

    private Deck? CreateDeck(FileInfo file)
    {
        (Exception? exception, String message)? error = null;

        Deck? deck = null;

        try
        {
            deck = CreateDeckFromFile(file);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error = (exception, "Failed to read deck file");
        }
        catch (Exception exception) when (exception is XmlException or InvalidOperationException)
        {
            error = (exception, "Failed to parse deck file");
        }

        Context.ReportDiscovery(ResourceTypes.TextureBundleXML, RID.Path(file), error?.exception, error?.message);

        return deck;
    }

    private static Deck CreateDeckFromFile(FileInfo file)
    {
        using Stream stream = file.OpenRead();
        XElement root = XElement.Load(stream);

        if (!root.Descendants("Layer").Any())
            throw Exceptions.InvalidOperation("At least one layer is required.");

        return new Deck
        {
            File = file,
            Combinator = root.Elements("Layers").SingleOrDefault()?.Attribute("mode")?.Value,
            Layers = root.Elements("Layers").SingleOrDefault()?.Elements("Layer").Select(layer => new Deck.Layer
            {
                Source = GetRequiredAttribute(layer, "source"),
                Modifiers = layer.Elements("Modifier").Select(modifier => new Deck.Modifier
                {
                    Type = GetRequiredAttribute(modifier, "type"),
                    Parameters = GetParameters(modifier)
                }).ToArray()
            }).ToArray() ?? [],
            Modifiers = root.Elements("Modifiers").SingleOrDefault()?.Elements("Modifier").Select(modifier => new Deck.Modifier
            {
                Type = GetRequiredAttribute(modifier, "type"),
                Parameters = GetParameters(modifier)
            }).ToArray() ?? []
        };
    }

    private static IReadOnlyDictionary<String, String> GetParameters(XElement element)
    {
        return element.Attributes().Where(attribute => attribute.Name.LocalName != "type").ToDictionary(attribute => attribute.Name.LocalName, attribute => attribute.Value);
    }

    private static String GetRequiredAttribute(XElement element, String name)
    {
        String? value = element.Attribute(name)?.Value;

        if (String.IsNullOrWhiteSpace(value))
            throw Exceptions.InvalidOperation($"Attribute '{name}' is required.");

        return value;
    }

    private sealed class CombinatorContext(Combinator combinator, FileInfo file, IResourceContext context) : Combinator.IContext
    {
        void Combinator.IContext.ReportWarning(String message)
        {
            ReportWarning(combinator, message);
        }

        public void ReportWarning(IIssueSource source, String message)
        {
            context.ReportWarning(source, message, path: file);
        }
    }

    private sealed class ModifierContext(FileInfo file, IResourceContext context) : Modifier.IContext, IIssueSource
    {
        public Modifier? Modifier { get; set; }

        public Vector2i Position { get; set; }

        public Vector2i Size { get; set; }

        void Modifier.IContext.ReportWarning(String message)
        {
            if (Modifier != null)
                ReportWarning(Modifier, message);
            else
                ReportWarning(this, message);
        }

        public void ReportWarning(IIssueSource source, String message)
        {
            context.ReportWarning(source, message, path: file);
        }
    }

    private sealed class ResolvedDeck
    {
        public required Combinator Combinator { get; init; }

        public required IReadOnlyCollection<ResolvedLayer> Layers { get; init; }

        public required IReadOnlyCollection<ResolvedModifier> Modifiers { get; init; }
    }

    private sealed class ResolvedLayer
    {
        public required Int32 Index { get; init; }

        public required Sheet Source { get; init; }

        public required IReadOnlyCollection<ResolvedModifier> Modifiers { get; init; }
    }

    private sealed class ResolvedModifier
    {
        public required Int32 Index { get; init; }
        public required Modifier Modifier { get; init; }

        public required IReadOnlyDictionary<String, String> Parameters { get; init; }
    }
}
