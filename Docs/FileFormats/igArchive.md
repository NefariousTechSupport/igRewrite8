# igArchive

Disclaimer: this is only accurate for igArchive version 11, for other versions it may be incorrect.

igArchives are the main archive format used in the alchemy engine from marvel ultimate alliance 2 onwards.

They contain a number of files within them, with the per-file option to be compressed.
When compressing them, they are compressed in blocks.

The file consists of a header, table of contents, block table, file data, and then nametable.

## igArchive header format

The header changes per game, it'll be listed several times, once for each version

Version 11:
```c++
struct Header {
	/* 0x00 */ uint32_t _magicNumber;
	/* 0x04 */ uint32_t _version;
	/* 0x08 */ uint32_t _tocSize;
	/* 0x0C */ uint32_t _numFiles;
	/* 0x10 */ uint32_t _sectorSize;
	/* 0x14 */ uint32_t _hashSearchDivider;
	/* 0x18 */ uint32_t _hashSearchSlop;
	/* 0x1C */ uint32_t _numLargeFileBlocks;
	/* 0x20 */ uint32_t _numMediumFileBlocks;
	/* 0x24 */ uint32_t _numSmallFileBlocks;
	/* 0x2C */ uint64_t _nameTableOffset;
	/* 0x30 */ uint32_t _nameTableSize;
	/* 0x34 */ uint32_t _flags;
};
```

- `_magicNumber`: constant, should be `0x1A414749`.
- `_version`: self explanatory.
- `_tocSize`: total size of the file path hashes, file info headers, and block tables in bytes.
- `_numFiles`: number of files in this archive.
- `_sectorSize`: [used in aligning file data](#compression-in-igarchives)
- `_hashSearchDivider`: used in looking up files, can be calculated as `0xFFFFFFFF / _numFiles`.
- `_hashSearchSlop`: used in looking up files, __Need to write this__.
- `_numLargeFileBlocks`: see [block tables](#block-table) and  [compression in igArchives](#compression-in-igarchives)
- `_numMediumFileBlocks`: see [block tables](#block-table) and  [compression in igArchives](#compression-in-igarchives)
- `_numSmallFileBlocks`: see [block tables](#block-table) and  [compression in igArchives](#compression-in-igarchives)
- `_nameTableOffset`: offset to the [nametable](#nametable)
- `_nameTableSize`: size of the [nametable](#nametable)
- `_flags`: flags for hashing file paths
	- `kCaseInsensitiveHash`: If the 1s bit is set, then any instance of `\` becomes `/`, and the path is converted to lower case.
	- `kHashNameAndExtensionOnly`: If the 2s bit is set, then only hash the file name and extension.

## File Hashes

The header is followed by a list of hashes

Each hash is the fnv1a32 hash of the [logical name]() of the file.
These hashes are organised in increasing order.

Each hash maps one-to-one to the [file info headers](#file-info-header) and [name table]().

## File info header

The list of hashes is followed by information about each file in the archive the structure varies between file version (determined from the [header](#igarchive-header-format)), so all are listed here:

Version 11:
```c++
struct FileInfoHeader {
	/* 0x00 */ uint64_t _ordinalAndOffset;	// first 40 bits are the offset, remaining bits are the ordinal. Read this as a uint64_t to account for differing endiannesses.
	/* 0x08 */ uint32_t _length;
	/* 0x0C */ uint32_t _blockIndexAndCompressionType;
};
```

- Ordinal: The index of this file in the input list of files. The developers had a txt file with all the file paths to put in an archive, the first would have an ordinal of 0, the next 1, etc.
- Offset: where the file data starts, be it compressed or uncompressed.
- Length: the uncompressed size of the file
- Block Index & compression type: explained in [Compression in igArchives](#compression-in-igarchives) 

## Block table

Block tables will be discussed in more detail in [this](#compression-in-igarchives) section.

## Nametable

After seeking to the nametable offset, there'll be a list of uints, one for each file, stored in the same order as the file info headers and file path hashes.

Each uint is an offset to the start of the name for a file relative to the start of the nametable.

Once you reach a name, there'll actually be two null-terminated strings, and then another uint.
- The first string is called the "name". It seems to be a remenant from when the file was built.
- The next string is called the "logical name", the logical name is what's hashed to get the file path hash used in the the [file hashes](#file-hashes).
- The uint is the modification date of the file, unsure how it's encoded.

## Compression in igArchives

Files are not compressed all in one go, instead, they're split up into 32kb (`0x8000`) chunks, and each chunk is compressed individually.

Each compressed chunk starts from a multiple of the `_sectorSize` field in the header.

To assist with this, there are block tables, a block table consists of some numbers that explain where a compressed chunk of data starts, along with whether or not that chunk is compressed. Yes, sometimes an individual chunk is left uncompressed, presumably it was more efficient to do so.

A block table consists of a list of integers, where if the most significant bit is set to 1, then the file is compressed, the remaining bits function as a sector index, used to locate the offset of that compressed chunk with `_fileOffset + _sectorSize  * sector index` where `_fileOffset` is the start of the file according to the [file info header](#file-info-header) for the file.

There are three block tables, one for large files, where the items in the list are 32 bit integers, one for medium files, where the items in the list are 16 bit integers, and one for small files, where the items in the list are 8 bit integers.

One file only uses one kind of block table, never more. The block table to use depends on the length of the file and the `_sectorSize`. If a file is smaller than `0x7F * _sectorSize`, then use the small block table. If a file is smaller than `0x7FFF * _sectorSize`, then use the medium block table. If the file is larger than that, then use the large block table.

The compression algorithm is determined from taking `_blockIndexAndCompressionType >> 28`.

```c++
enum CompressionType {
	kUncompressed = 15,
	kZlib = 1,
	kLzma = 2,
	kLz4 = 3
};
```

To find the blocks for a file, find the corresponding block table as stated above and take `_blockIndexAndCompressionType & 0x0FFFFFFF`. Index into the block table using that number. The blocks to use stored in order from that index onwards. There will be one item that is just 0 to separate files, however this isn't strictly needed. If the file is uncompressed then there's no need to read the block table at all.

A compressed chunk starts with a little endian 16 bit integer (it's always little endian, even if the rest of the file is big endian, idk why), this number is the size of the compressed chunk.

if the compression type is LZMA, then there's 5 bytes for the decoder properties. the size of the compressed chunk does not include this.

The remaining data is the compressed data, it'll decompress either to 32kb or however much data remains in the file.

Upon decompressing a chunk, move to the next item in the block table, seek to `_fileOffset + _sectorSize  * sector index`, check if it's compressed by checking the most significant bit, then if not compressed, simply copy the data, otherwise decompress it.

# Credits

- AdventureT & LG-RZ: figuring out the hashing algorithm
- DTZxPorter: figuring out how to calculate the hash search slop and how files are looked up in the igArchive

# Resources

- igCauldron's [igArchive class](../../igLibrary/Core/igArchive.cs)
- LG-RZ's [igArchiveLib](https://github.com/LG-RZ/igArchiveLib)