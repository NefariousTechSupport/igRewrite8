# igObjectDirectory files

IGB, IGX, and [IGZ](./IGZ.md) files are all igObjectDirectory files, they contain igObjects, objects being instances of c++ classes defined in the game's executable.

igObjectDirectories have:
- A list of dependencies, other igObjectDirectories that must be loaded.
- A root object list.
- A name list, each name corresponds to each root object. (object 0 has name 0, object 1 has name 1, etc).

