[![build](https://github.com/jeanpmathes/VoxelGame/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/jeanpmathes/VoxelGame/actions/workflows/build.yml)

# VoxelGame

VoxelGame is a (very) work-in-progress voxel engine and game.
The game logic is written mostly in C#, and rendering uses C++ and DirectX 12 for ray tracing.

## Installation

Windows installers are available in the release section.

## Building

Building requires C# and C++ support installed through Visual Studio as well as CMake.
When checking the project out, make sure to initialize the submodules:

```bash
git clone --recursive https://github.com/jeanpmathes/VoxelGame.git
```

Then build by opening the `VoxelGame.sln` solution file in Visual Studio and building the solution in `Release` mode for
`x64`.

## License

VoxelGame is distributed under the [GPL License](LICENSE).

Additional attributions for third-party resources can be found in
the [Attribution directory](src/presentation/Resources/Attribution).

## Contributing

For contribution guidelines see [CONTRIBUTING.md](CONTRIBUTING.md).
