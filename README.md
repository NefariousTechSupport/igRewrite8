# igCauldron

A modding tool for the alchemy engine

## Usage

Create a new game config, in the game path, put the directory containing the `archives` folder, (e.g. for PS3 this'll be the `USRDIR` folder, and for Wii U this'll be the `content` folder).

Note that only Superchargers 1.6.X and Imaginators 1.1.0 are supported.

Logs are stored in `%AppData%/NefariousTechSupport/igCauldron`. These may be quite large so feel free to delete the Logs directory if you find that you're running out of space, logs before a certain date will be automatically deleted in a later update.

## Running from source

cd into the `Resources` directory and run `dotnet run --project ../igCauldron3`

## Building for release

cd into the root of the repo (the folder with the .sln) and run `dotnet publish --configuration=Release`, this'll also copy the necessary resources

## The Projects

- `igCauldron`: The modding tool itself, it acts as a frontend for igLibrary
- `igLibrary`: The main alchemy interface library, this contains basically everything for interacting with the files through code
- `igLibrary.CodeGen`: Source code generation to remove a lot of boilerplate for array metafields
- `igLibrary.Tests`: Unit tests for parts of igLibrary
- `igRewrite8`: General testing ground for various parts of igLibrary (to be removed)
- `Potion`: The mod manager library: this is responsible for creating, applying, and reverting modifications
- `VvlToDll`: a utility for converting Vvl files to Dlls

## Contributing

Please try and match the styling found in [igMetaField](igLibrary/Core/igMetaField.cs). If you make a change to a file do be sure to update the styling of whatever you find before/after making changes in a separate commit.

## Todo

- Separate `igLibrary` into various layers:
	- Some core utilities library, this should be a library that's shared between projects to avoid copying stuff like StreamHelper between projects
	- Inner alchemy: This should include the main alchemy tech shared across all alchemy games (e.g. file management and loaders, reflection, etc)
	- Alchemy Laboratory: This should include all the vv alchemy laboratory specific stuff (vvls, entity system, etc)
	- tfbTool:  This should include all the tfbTool specific stuff (tfbScript, tfbRender, etc). This isn't implemented at all.
	- Similar things should be done for other "frontends" for alchemy, like the legacy Alchemy 5 and below
- Remove/Greately improve binary ark core files: the binary format could certainly be faster if designed well, however it's not currently designed well
- Remake UI system to allow for undo/redo, this'll also allow for reverting all changes to a file (like when closing without saving)
- More ui overrides for certain types that users want to edit often (e.g. interpretting `igGraphicsMaterial`)
- Better management of resources to allow for closing and destructing unused/closed data
- Redo hashtables to allow for custom hashing methods, mostly important for case sensitive and case insensitive string hashing
- Fork [ILSpy](https://github.com/icsharpcode/ILSpy)'s decompiler and make changes to make it play nice with Vvl bytecode, as it's not identical to standard dotnet