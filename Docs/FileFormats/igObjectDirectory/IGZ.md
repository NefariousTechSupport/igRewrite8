# IGZ

Disclaimer: this documentation is only correct for version 9, it may be incorrect for other
versions.

NEED TO DESCRIBE THE PRE VERSION 9 RUNTIME FIXUP PACKING

IGZs are essentially memory images, with a "fixup" section to allow for in place pointer
fix up.

As a result, they are incredibly fast to load, however they are not designed to be easily
modified and are also incredibly platform and version specific.

## Background reading

Many parts of the IGZ format rely on very technical concepts. I've linked resources to
explain some of those concepts

- Classes & Objects:
- Inheritance:
- Vtables
- Struct alignment:
- 32 bit vs 64 bit:
- pointers:

## IGZ loader fields

An IGZ loader should the following fields

- `_stringList`: a list of strings referenced by things in the file
- `_vtableList`: a list of virtual table pointers.
- `_namedHandleList`: A list of external `igHandle`s referenced by name.
- `_namedExternalList`: A list of external `igObject`s referenced by name. 
- `_externalList`: A list of external `igObject`s referenced by name hash. 

## IGZ header

```c++
struct igzHeader {
	/* 0x000 */ uint32_t _magicNumber;
	/* 0x004 */ uint32_t _version;
	/* 0x008 */ uint32_t _serializableFieldsHash;
	/* 0x00C */ IG_CORE_PLATFORM _platform;
	/* 0x010 */ uint32_t _numFixups;
	/* 0x014 */ igzChunkHeader _chunks[0x20];
	/* 0x224 */ char _memoryPoolNames[0x5DC];
};
struct igzChunkHeader {
	/* 0x00 */ uint32_t _memoryPoolNameOffset;
	/* 0x04 */ uint32_t _offset;
	/* 0x08 */ uint32_t _length;
	/* 0x0C */ uint32_t _alignment;
};
```

igz header:
- `_magicNumber`: Constant, `0x015A4749`
- `_version`: self explanatory
- `_serializableFieldHash`: value caluated based on the metadata of the class
- `_platform`: the [platform](#ig_core_platform) this file was built for.
- `_numFxups`: the number of fixups in the file
- `_chunks`: different memory images in the file
- `_memoryPoolNames`: list of memory pool names, stored as null terminated strings

igz chunk header:
- `_memoryPoolNameOffset`: offset of the name of the memory pool to load this into, relative
  to the start of `_memoryPoolNames` in `igzHeader`.
- `_offset`: the offset at which this section begins
- `_length`: the size of this section
- `_alignment`: the alignment of this section.

## IG_CORE_PLATFORM

the values for IG_CORE_PLATFORM change every version.

version 9
```c++
enum IG_CORE_PLATFORM {
	IG_CORE_PLATFORM_DEFAULT = 0x00,
	IG_CORE_PLATFORM_WIN32 = 0x01,
	IG_CORE_PLATFORM_WII = 0x02,
	IG_CORE_PLATFORM_DURANGO = 0x03,
	IG_CORE_PLATFORM_ASPEN = 0x04,
	IG_CORE_PLATFORM_XENON = 0x05,
	IG_CORE_PLATFORM_PS3 = 0x06,
	IG_CORE_PLATFORM_OSX = 0x07,
	IG_CORE_PLATFORM_WIN64 = 0x08,
	IG_CORE_PLATFORM_CAFE = 0x09,
	IG_CORE_PLATFORM_RASPI = 0x0A,
	IG_CORE_PLATFORM_ANDROID = 0x0B,
	IG_CORE_PLATFORM_ASPEN64 = 0x0C,
	IG_CORE_PLATFORM_LGTV = 0x0D,
	IG_CORE_PLATFORM_PS4 = 0x0E,
	IG_CORE_PLATFORM_WP8 = 0x0F,
	IG_CORE_PLATFORM_LINUX = 0x10,
	IG_CORE_PLATFORM_MAX = 0x11
};
```

- Win32: Windows 32 bit
- Wii: Wii
- Durango: Xbox One
- Aspen: iOS 32 bit
- Xenon: Xbox 360
- PS3: PS3
- OSX: Max OS 32 bit
- Win64: Windows 64 bit
- Cafe: Wii U
- Raspi: Raspberry Pi
- Android: Android
- Aspen64: iOS 64 bit
- LGTV: LG Smart TV
- PS4: PS4
- WP8: Windows Phone
- Linux: Linus Tech Tips
- Max: number of platforms

## Fixups

Fixup sections contain information needed to fix things like pointers to be actually usable
as memory.

There are many types of fixups

All are structured as follows:

Version 9:
```c++
struct FixupHeader {
	/* 0x00 */ uint32_t _magicNumber;
	/* 0x04 */ uint32_t _numberOfItems;
	/* 0x08 */ uint32_t _length;
	/* 0x0C */ uint32_t _dataOffset;
};
```

- `_magicNumber`: constant value identifying the type of fixup
- `_numberOfItems`: the number of items in this fixup
- `_length`: the length of the fixup + header in bytes
- `_dataOffset`: where the data for the fixup starts, relative to the start of the fixup header 

### TDEP: Dependency Table

This is a string array, it contains two strings per item. The first is the namespace of the
string, the second is the file path of the dependency. If the filepath begins with `<build>`,
then it should be ignored.

the dependencies are read into an `igObjectDirectory`'s dependency list.

### TSTR: String Table

This is a string array, it contains one string per item. The strings are aligned to 2 bytes
on igz version 7 and above, and 1 byte on igz version 6 and below.

### TMET: MetaObject Table

Structured the same way as TMET, the strings stored here represent the names of classes as
defined in the reflection system.

### EXNM: External Names

Structured as a 64 bit number.
The upper 32 bits are the index into TSTR for the namespace of the handle
The lower 32 bits are the index into TSTR for the name of the object the handle references.

When resolving handles, the namespaces in the dependency list gets priority.

if the namespace index has the most significant bit set, then the external is appended to
the `_namedHandleList`. The handle doesn't need to be resolved for this.

If the namespace index does not have the most significant bit set, then it's appended to
the `_namedExternalList`. This requires the handle be resolved when loading the file, rather
than it being resolved when actually being used.

### EXID: External Identifiers

Similar to EXNM in premise, except instead of storing the namespace and name's string, it
stores their fnv1a32 hashes.

When resolving handles, the namespaces in the dependency list gets priority.

### TMHN: Memory Handle Table

Structured the same as an igMemory object, really, this is just a list of `igMemory`s.

## Runtime Fixups

Runtime fixups contain a list of pointers to pointers

This list is compressed

First off, pointers are all serialized as described __here__, as well as being ordered in
ascending order.

Here's an example of a set of pointers

```
00000004
00000010
00000014
00000024
08000010
```

Second off, all stored as the difference from the previous one, with the first one just
being the first one (demonstrated by subtracting zero from it).

```
00000004 - 00000000 => 00000004
00000010 - 00000004 => 0000000C
00000014 - 00000010 => 00000004
00000024 - 00000014 => 00000010
08000010 - 00000024 => 07FFFFEC
```

Next, all of the items are divided by 4.

```
00000004 / 4 => 00000001
0000000C / 4 => 00000003
00000004 / 4 => 00000001
00000010 / 4 => 00000004
07FFFFEC / 4 => 01FFFFFB
```

This final list is what's actually compressed.

data is compressed into 4 bit chunks. The most significant bit states whether or not the
data continues to the next chunk. The remaining 3 bits are the least significant bits of
the integer.

so, let's start with the first item in the list, it'll be compressed to one chunk, `0001`.
The next one will be compressed to one chunk as well, `0011`, then `0001`, `0100`.

`01FFFFFB` on the other hand will be compressed into multiple chunks. Let's convert it
to its binary representation to make it easier to read for this purpose.

```
0001111111111111111111111011
```

Now let's split it up into 3 bit chunks

```
0 001 111 111 111 111 111 111 111 011
```

Now let's take each chunk and add that fourth bit to it, which states whether or not it's
the end of a chunk.

```
0001 1111 1111 1111 1111 1111 1111 1111 1011
```

The leftmost chunk was removed as the integer was already completed.

now let's combine all these chunks

```
0001 0011
0001 0100
0001 1111
1111 1111
1111 1111
1111 1111
1011 0000 (the 0 is padding since there's 8 bits in a byte)
```

now, the least significant bits are read, and then the most significant 4 bits, so it should
be stored as such.

```
0011 0001
0100 0001
1111 0001
1111 1111
1111 1111
1111 1111
0000 1011 (the 0 is padding since there's 8 bits in a byte)
```
```
00110001
01000001
11110001
11111111
11111111
11111111
00001011
```

Then converting these to hex gives us the following list of packed integers

```
31 41 F1 FF FF FF 0B
```

Unpacking the integers is simply this process in reverse.

### RVTB: Runtime Virtual Tables

The RVTB contains a list of pointers to the starts of objects, specifically their vtable
pointers. The value contained at one of these pointers is a 32 bit number representing the
index of the type that this object is in the [TMET](#tmet-metaobject-table), or `_vtableList`.

### ROOT: Runtime Root Object List

This fixup contains only one pointer, a pointer to the root object list.

### ROFS: Runtime Offsets

This fixup contains a list of pointers to all serialized offsets. The value contained at one
of these pointers is a 32 bit number representing an offset, this offset is deserialized and
converted to a proper pointer.

### RPID: Runtime Pool IDs

Honestly a bit unsure, i just know it points to the offset field of empty igMemory objects

### RSTT: Runtime String Table

This fixup contains a list of pointers to all references to strings stored in
[TSTR](#tstr-string-table) (excluding [EXNM](#exnm-external-names)). The value contained at
one of these pointers is a 32 bit number representing an index into [TSTR](#tstr-string-table)
or `_stringList`, the string is looked up in this list and then the index is replaced by a
pointer to that string.

Either this or [RSTR](#rstr-runtime-string-references) are used, but not both. Though both
may be usable at the same time.

### RSTR: Runtime String References

Very similar to [ROFS](#rofs-runtime-offsets), except only for strings, it's a separate
fixup since the engine has a whole system for managing strings.

Either this or [RSTT](#rstt-runtime-string-table) are used, but not both. Though both may
be usable at the same time.

### RMHN: Runtime Memory Handle

This fixup contains a list of pointers to all memory handles. The value contained at one
of these pointers is a 32 bit number representing an index into the
[TMHN](#tmhn-memory-handle-table), this index is looked up and converted to a
thumbnail/memory handle.

### REXT: Runtime External Identifiers

This fixup contains a list of pointers to all references to an EXID object ref. The value
contained at one of these pointers is a 32 bit number representing an index into the EXID,
this index is deserialized and converted to a proper pointer to the object in question.

### RNEX: Runtime Named Externals

This fixup contains a list of pointers to all references to an EXNM object ref. The value
contained at one of these pointers is a 32 bit number representing an index into the EXID,
this index is deserialized and converted to a proper pointer to the object in question.
