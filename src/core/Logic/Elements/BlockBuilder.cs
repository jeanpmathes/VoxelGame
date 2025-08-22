// <copyright file="BlockBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Definitions;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// The main class for defining blocks. It creates, configures and registers blocks.
/// </summary>
public class BlockBuilder
{
    private readonly ContentRegistry registry;
    private readonly BlockFactory factory;
    
    public IReadOnlyList<Block> BlocksByID => factory.BlocksByID;
    public IReadOnlyDictionary<String, Block> BlocksByNamedID => factory.BlocksByNamedID;
    public ContentRegistry Registry => registry;
    
    /// <summary>
    /// Creates a new instance of the <see cref="BlockBuilder"/> class.
    /// Generally not needed to be called, as the <see cref="Blocks"/> class will handle that.
    /// </summary>
    /// <returns>The created <see cref="BlockBuilder"/> instance.</returns>
    public static BlockBuilder Create()
    {
        return new BlockBuilder();
    }

    private BlockBuilder()
    {
        registry = ContentRegistry.Create();
        factory = new BlockFactory();
    }

    private BlockBuilder(BlockBuilder parent)
    {
        registry = parent.registry.CreateScoped();
        factory = parent.factory;
    }

    /// <summary>
    /// Create a new scoped builder.
    /// The registry of the scoped builder will only contain the blocks defined in this scope.
    /// </summary>
    /// <returns>The scoped <see cref="BlockBuilder"/> instance.</returns>
    public BlockBuilder CreateScoped()
    {
        return new BlockBuilder(this);
    }

    /// <summary>
    /// Build a new convention.
    /// This must be used in convention builder extensions, as it performs scoping of the builder and also registers the convention itself.
    /// </summary>
    /// <param name="supplier">The supplier function that will create the convention.</param>
    /// <typeparam name="TConvention">The type of the convention to build, must implement <see cref="IConvention"/>.</typeparam>
    /// <returns>The created convention.</returns>
    public TConvention BuildConvention<TConvention>(Func<BlockBuilder, TConvention> supplier) where TConvention : IConvention
    {
        return registry.RegisterContent(supplier(CreateScoped()));
    }
    
    /// <summary>
    /// Defines a new <see cref="Meshable.Simple"/> block.
    /// </summary>
    public SimpleBlockDefinition BuildSimpleBlock(String name, String namedID)
    {
        Block block = factory.Create(name, namedID, Meshable.Simple);
        
        return new SimpleBlockDefinition(block, registry);
    }
    
    /// <summary>
    /// Defines a new <see cref="Meshable.Complex"/> block.
    /// </summary>
    public FoliageBlockDefinition BuildFoliageBlock(String name, String namedID)
    {
        Block block = factory.Create(name, namedID, Meshable.Foliage);
        
        return new FoliageBlockDefinition(block, registry);
    }
    
    /// <summary>
    /// Defines a new <see cref="Meshable.Complex"/> block.
    /// </summary>
    public ComplexBlockDefinition BuildComplexBlock(String name, String namedID)
    {
        Block block = factory.Create(name, namedID, Meshable.Complex);
        
        return new ComplexBlockDefinition(block, registry);
    }
    
    /// <summary>
    /// Defines a new <see cref="Meshable.PartialHeight"/> block.
    /// </summary>
    public PartialHeightBlockDefinition BuildPartialHeightBlock(String name, String namedID)
    {
        Block block = factory.Create(name, namedID, Meshable.PartialHeight);
        
        return new PartialHeightBlockDefinition(block, registry);
    }
    
    /// <summary>
    /// Defines a new <see cref="Meshable.Unmeshed"/> block.
    /// </summary>
    public UnmeshedBlockDefinition BuildUnmeshedBlock(String name, String namedID)
    {
        Block block = factory.Create(name, namedID, Meshable.Unmeshed);
        
        return new UnmeshedBlockDefinition(block, registry);
    }

    /// <summary>
    /// An intermediate step in the block creation process.
    /// </summary>
    public class BlockDefinition(Block block, ContentRegistry registry)
    {
        /// <summary>
        /// The block being defined. Do not use this until <see cref="Complete"/> is called.
        /// </summary>
        protected readonly Block block = block;
        
        /// <summary>
        /// The content registry to register the block in.
        /// </summary>
        protected readonly ContentRegistry registry = registry;
        
        /// <summary>
        /// Require a behavior to be present on the block.
        /// </summary>
        /// <param name="initializer">An optional initializer for the behavior which will be called immediately after the behavior is created or retrieved.</param>
        /// <typeparam name="TBehavior">The type of the behavior to require.</typeparam>
        public BlockDefinition WithBehavior<TBehavior>(Action<TBehavior>? initializer = null) 
            where TBehavior : BlockBehavior, IBehavior<TBehavior, BlockBehavior, Block>
        {
            var behavior = block.Require<TBehavior>();
            
            initializer?.Invoke(behavior);

            return this;
        }
        
        /// <summary>
        /// Require two behaviors to be present on the block and initialize them together.
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
        /// Contribute to the block's properties.
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
        /// Contribute to the block's bounding volume.
        /// </summary>
        /// <param name="boundingVolume">The bounding volume to contribute.</param>
        public BlockDefinition WithBoundingVolume(BoundingVolume boundingVolume)
        {
            block.BoundingVolume.ContributeConstant(boundingVolume);
            
            return this;
        }
        
        /// <summary>
        /// Set the wet tint color for the block. This will make the block able to be wet, changing its tint when that is the case.
        /// </summary>
        /// <param name="color">The color to use as tint when the block is wet.</param>
        public BlockDefinition WithWetTint(ColorS? color = null)
        {
            block.Require<WetTint>().WetColorInitializer.ContributeConstant(color ?? ColorS.LightGray);
            
            return this;
        }
        
        /// <summary>
        /// Make a part of the block's definition conditional based on a boolean condition.
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
        /// Completes the block definition and registers it in the content registry.
        /// </summary>
        /// <returns>The completed <see cref="Complete"/> instance.</returns>
        public Block Complete()
        {
            registry.RegisterContent(block);
            
            return block;
        }
    }
    
    /// <summary>
    /// An intermediate step in the block creation process, specifically for <see cref="SimpleBlock"/>.
    /// </summary>
    public class SimpleBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        /// Set the texture layout for the block.
        /// </summary>
        /// <param name="textureLayout">The texture layout to set.</param>
        public SimpleBlockDefinition WithTextureLayout(TextureLayout textureLayout)
        {
            if (block.Get<CubeTextured>() is {} texture)
                texture.DefaultTextureInitializer.ContributeConstant(textureLayout);

            return this;
        }
        
        /// <summary>
        /// Set the wet texture layout for the block. This will make the block able to be wet, swapping the texture when that is the case.
        /// </summary>
        /// <param name="wetTextureLayout">The texture layout to use when the block is wet.</param>
        public SimpleBlockDefinition WithWetTextureLayout(TextureLayout wetTextureLayout)
        {
            block.Require<WetCubeTexture>().WetTextureInitializer.ContributeConstant(wetTextureLayout);

            return this;
        }
    }

    /// <summary>
    /// An intermediate step in the block creation process, specifically for <see cref="Meshable.Foliage"/> blocks.
    /// </summary>
    public class FoliageBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        /// Set the texture for the block.
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
    /// An intermediate step in the block creation process, specifically for <see cref="Meshable.Complex"/> blocks.
    /// </summary>
    public class ComplexBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry);

    /// <summary>
    /// An intermediate step in the block creation process, specifically for <see cref="Meshable.PartialHeight"/> blocks.
    /// </summary>
    public class PartialHeightBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry)
    {
        /// <summary>
        /// Set the texture layout for the block.
        /// </summary>
        /// <param name="textureLayout">The texture layout to set.</param>
        public PartialHeightBlockDefinition WithTextureLayout(TextureLayout textureLayout)
        {
            if (block.Get<CubeTextured>() is {} texture)
                texture.DefaultTextureInitializer.ContributeConstant(textureLayout);

            return this;
        }
        
        /// <summary>
        /// Set the wet texture layout for the block. This will make the block able to be wet, swapping the texture when that is the case.
        /// </summary>
        /// <param name="wetTextureLayout">The texture layout to use when the block is wet.</param>
        public PartialHeightBlockDefinition WithWetTextureLayout(TextureLayout wetTextureLayout)
        {
            block.Require<WetCubeTexture>().WetTextureInitializer.ContributeConstant(wetTextureLayout);

            return this;
        }
    }

    /// <summary>
    /// An intermediate step in the block creation process, specifically for <see cref="Meshable.Unmeshed"/> blocks.
    /// </summary>
    public class UnmeshedBlockDefinition(Block block, ContentRegistry registry) : BlockDefinition(block, registry);
}



