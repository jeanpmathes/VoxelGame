// <copyright file="BlockBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     The main class for defining blocks. It creates, configures and registers blocks.
/// </summary>
public class BlockBuilder
{
    private readonly BlockFactory factory;

    private BlockBuilder()
    {
        Registry = ContentRegistry.Create();
        factory = new BlockFactory();
    }

    private BlockBuilder(BlockBuilder parent)
    {
        Registry = parent.Registry.CreateScoped();
        factory = parent.factory;
    }

    /// <summary>
    ///     Get a list mapping the (numerical) block IDs to the blocks.
    /// </summary>
    public IReadOnlyList<Block> BlocksByID => factory.BlocksByID;

    /// <summary>
    ///     Get a dictionary mapping the named block IDs to the blocks.
    /// </summary>
    public IReadOnlyDictionary<String, Block> BlocksByNamedID => factory.BlocksByNamedID;
    
    /// <summary>
    ///     Get a set of blocks that had a collision on their named ID during creation.
    /// </summary>
    public IReadOnlySet<Block> BlocksWithCollisionOnID => factory.BlocksWithCollisionOnID;

    /// <summary>
    ///     Get the registry in which all content is registered, which is mainly the blocks but also conventions and other
    ///     content.
    /// </summary>
    public ContentRegistry Registry { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="BlockBuilder" /> class.
    ///     Generally not needed to be called, as the <see cref="Blocks" /> class will handle that.
    /// </summary>
    /// <returns>The created <see cref="BlockBuilder" /> instance.</returns>
    public static BlockBuilder Create()
    {
        return new BlockBuilder();
    }

    /// <summary>
    ///     Create a new scoped builder.
    ///     The registry of the scoped builder will only contain the blocks defined in this scope.
    /// </summary>
    /// <returns>The scoped <see cref="BlockBuilder" /> instance.</returns>
    public BlockBuilder CreateScoped()
    {
        return new BlockBuilder(this);
    }

    /// <summary>
    ///     Build a new convention.
    ///     This must be used in convention builder extensions, as it performs scoping of the builder and also registers the
    ///     convention itself.
    /// </summary>
    /// <param name="supplier">The supplier function that will create the convention.</param>
    /// <typeparam name="TConvention">The type of the convention to build, must implement <see cref="IConvention" />.</typeparam>
    /// <returns>The created convention.</returns>
    public TConvention BuildConvention<TConvention>(Func<BlockBuilder, TConvention> supplier) where TConvention : IConvention
    {
        return Registry.RegisterContent(supplier(CreateScoped()));
    }

    /// <summary>
    ///     Defines a new <see cref="Meshable.Simple" /> block.
    /// </summary>
    public SimpleBlockDefinition BuildSimpleBlock(String namedID, String name)
    {
        Block block = factory.Create(namedID, name, Meshable.Simple);

        return new SimpleBlockDefinition(block, Registry);
    }

    /// <summary>
    ///     Defines a new <see cref="Meshable.Complex" /> block.
    /// </summary>
    public FoliageBlockDefinition BuildFoliageBlock(String namedID, String name)
    {
        Block block = factory.Create(namedID, name, Meshable.Foliage);

        return new FoliageBlockDefinition(block, Registry);
    }

    /// <summary>
    ///     Defines a new <see cref="Meshable.Complex" /> block.
    /// </summary>
    public ComplexBlockDefinition BuildComplexBlock(String namedID, String name)
    {
        Block block = factory.Create(namedID, name, Meshable.Complex);

        return new ComplexBlockDefinition(block, Registry);
    }

    /// <summary>
    ///     Defines a new <see cref="Meshable.PartialHeight" /> block.
    /// </summary>
    public PartialHeightBlockDefinition BuildPartialHeightBlock(String namedID, String name)
    {
        Block block = factory.Create(namedID, name, Meshable.PartialHeight);

        return new PartialHeightBlockDefinition(block, Registry);
    }

    /// <summary>
    ///     Defines a new <see cref="Meshable.Unmeshed" /> block.
    /// </summary>
    public UnmeshedBlockDefinition BuildUnmeshedBlock(String namedID, String name)
    {
        Block block = factory.Create(namedID, name, Meshable.Unmeshed);

        return new UnmeshedBlockDefinition(block, Registry);
    }

    /// <summary>
    ///     An intermediate step in the block creation process.
    /// </summary>
    public class BlockDefinition(Block block, ContentRegistry registry)
    {
        /// <summary>
        ///     The block being defined. Do not use this until <see cref="Complete" /> is called.
        /// </summary>
        protected readonly Block block = block;

        /// <summary>
        ///     The content registry to register the block in.
        /// </summary>
        protected readonly ContentRegistry registry = registry;

        /// <summary>
        ///     Require a behavior to be present on the block.
        /// </summary>
        /// <param name="initializer">
        ///     An optional initializer for the behavior which will be called immediately after the behavior
        ///     is created or retrieved.
        /// </param>
        /// <typeparam name="TBehavior">The type of the behavior to require.</typeparam>
        public BlockDefinition WithBehavior<TBehavior>(Action<TBehavior>? initializer = null)
            where TBehavior : BlockBehavior, IBehavior<TBehavior, BlockBehavior, Block>
        {
            var behavior = block.Require<TBehavior>();

            initializer?.Invoke(behavior);

            return this;
        }

        /// <summary>
        ///     Require two behaviors to be present on the block and initialize them together.
        /// </summary>
        /// <param name="initializer">The initializer that will be called with the two behaviors.</param>
        /// <typeparam name="TBehaviorA">The type of the first behavior to require.</typeparam>
        /// <typeparam name="TBehaviorB">The type of the second behavior to require.</typeparam>
        public BlockDefinition WithBehavior<TBehaviorA, TBehaviorB>(Action<TBehaviorA, TBehaviorB> initializer)
            where TBehaviorA : BlockBehavior, IBehavior<TBehaviorA, BlockBehavior, Block>
            where TBehaviorB : BlockBehavior, IBehavior<TBehaviorB, BlockBehavior, Block>
        {
            var behaviorA = block.Require<TBehaviorA>();
            var behaviorB = block.Require<TBehaviorB>();

            initializer(behaviorA, behaviorB);

            return this;
        }

        /// <summary>
        ///     Contribute to the block's properties.
        /// </summary>
        /// <param name="action">An action contributing, will be called when the block is initializing.</param>
        public BlockDefinition WithProperties(Action<BlockProperties> action)
        {
            block.Initializing += OnInitializing;

            return this;

            void OnInitializing(Object? sender, Block.InitializingEventArgs e)
            {
                action(e.Properties);

                block.Initializing -= OnInitializing;
            }
        }

        /// <summary>
        ///     Contribute to the block's bounding volume.
        /// </summary>
        /// <param name="boundingVolume">The bounding volume to contribute.</param>
        public BlockDefinition WithBoundingVolume(BoundingVolume boundingVolume)
        {
            block.BoundingVolume.ContributeConstant(boundingVolume);

            return this;
        }

        /// <summary>
        ///     Set the wet tint color for the block. This will make the block able to be wet, changing its tint when that is the
        ///     case.
        /// </summary>
        /// <param name="color">The color to use as tint when the block is wet.</param>
        public BlockDefinition WithWetTint(ColorS? color = null)
        {
            block.Require<WetTint>().WetColorInitializer.ContributeConstant(color ?? ColorS.LightGray);

            return this;
        }

        /// <summary>
        ///     Set a <see cref="TextureOverride" /> on this block, which will override textures provided by models.
        ///     Generally only sensible for <see cref="Meshable.Complex" /> blocks.
        /// </summary>
        /// <param name="overrides">
        ///     A dictionary mapping texture indices to the texture to use as override. Use -1 as key to
        ///     override all textures which are not specified in the dictionary otherwise.
        /// </param>
        /// <returns>The current <see cref="BlockDefinition" /> instance.</returns>
        public BlockDefinition WithTextureOverride(IReadOnlyDictionary<Int32, TID> overrides)
        {
            block.Require<TextureOverride>().TexturesInitializer.ContributeConstant(overrides);

            return this;
        }

        /// <summary>
        ///     Make a part of the block's definition conditional based on a boolean condition.
        /// </summary>
        /// <param name="condition">The boolean condition that determines whether the definition should be applied.</param>
        /// <param name="definition">The conditional definition, will be called immediately if the condition is true.</param>
        public BlockDefinition WithConditionalDefinition(Boolean condition, Func<BlockDefinition, BlockDefinition> definition)
        {
            if (condition)
            {
                definition(this);
            }

            return this;
        }

        /// <summary>
        ///     Completes the block definition and registers it in the content registry.
        /// </summary>
        /// <returns>The completed <see cref="Complete" /> instance.</returns>
        public Block Complete()
        {
            registry.RegisterContent(block);

            return block;
        }
    }

    /// <summary>
    ///     An intermediate step in the block creation process, specifically for <see cref="SimpleBlock" />.
    /// </summary>
    public class SimpleBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        ///     Set the texture layout for the block.
        /// </summary>
        /// <param name="textureLayout">The texture layout to set.</param>
        public SimpleBlockDefinition WithTextureLayout(TextureLayout textureLayout)
        {
            block.Require<CubeTextured>().DefaultTextureInitializer.ContributeConstant(textureLayout);

            return this;
        }

        /// <summary>
        ///     Set the wet texture layout for the block. This will make the block able to be wet, swapping the texture when that
        ///     is the case.
        /// </summary>
        /// <param name="wetTextureLayout">The texture layout to use when the block is wet.</param>
        public SimpleBlockDefinition WithWetTextureLayout(TextureLayout wetTextureLayout)
        {
            block.Require<WetCubeTexture>().WetTextureInitializer.ContributeConstant(wetTextureLayout);

            return this;
        }
    }

    /// <summary>
    ///     An intermediate step in the block creation process, specifically for <see cref="Meshable.Foliage" /> blocks.
    /// </summary>
    public class FoliageBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        ///     Set the texture for the block.
        /// </summary>
        /// <param name="texture">The texture to set.</param>
        public FoliageBlockDefinition WithTexture(TID texture)
        {
            if (block.Get<SingleTextured>() is {} textured)
                textured.DefaultTextureInitializer.ContributeConstant(texture);

            return this;
        }
    }

    /// <summary>
    ///     An intermediate step in the block creation process, specifically for <see cref="Meshable.Complex" /> blocks.
    /// </summary>
    public class ComplexBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry);

    /// <summary>
    ///     An intermediate step in the block creation process, specifically for <see cref="Meshable.PartialHeight" /> blocks.
    /// </summary>
    public class PartialHeightBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        ///     Set the texture layout for the block.
        /// </summary>
        /// <param name="textureLayout">The texture layout to set.</param>
        public PartialHeightBlockDefinition WithTextureLayout(TextureLayout textureLayout)
        {
            if (block.Get<CubeTextured>() is {} texture)
                texture.DefaultTextureInitializer.ContributeConstant(textureLayout);

            return this;
        }

        /// <summary>
        ///     Set the wet texture layout for the block. This will make the block able to be wet, swapping the texture when that
        ///     is the case.
        /// </summary>
        /// <param name="wetTextureLayout">The texture layout to use when the block is wet.</param>
        public PartialHeightBlockDefinition WithWetTextureLayout(TextureLayout wetTextureLayout)
        {
            block.Require<WetCubeTexture>().WetTextureInitializer.ContributeConstant(wetTextureLayout);

            return this;
        }
    }

    /// <summary>
    ///     An intermediate step in the block creation process, specifically for <see cref="Meshable.Unmeshed" /> blocks.
    /// </summary>
    public class UnmeshedBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry);
}
