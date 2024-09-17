using System.Diagnostics;
using System.IO.Compression;
using K4os.Compression.LZ4;
using SevenZip;

namespace igLibrary.Core
{
	public class igArchive : igStorageDevice
	{
		[igStruct]
		public struct Header
		{
			public uint _magicNumber;
			public uint _version;
			public uint _tocSize;
			public uint _numFiles;
			public uint _sectorSize;
			public uint _hashSearchDivider;
			public uint _hashSearchSlop;
			public uint _numLargeFileBlocks;
			public uint _numMediumFileBlocks;
			public uint _numSmallFileBlocks;
			public ulong _nameTableOffset;
			public uint _nameTableSize;
			public uint _flags;
		}
		enum CompressionType : uint
		{
			kUncompressed = 0,
			kZlib = 1,
			kLzma = 2,
			kLz4 = 3,
			kCompressionFormatShift = 28,
			kCompressionFormatMask = 0xF0000000,
			kFirstBlockMask = 0x0FFFFFFF,
			kOffsetBits = 40,
		};
		public enum EBlockType : byte
		{
			kSmall,
			kMedium,
			kLarge,
			kNone
		}
		public class FileInfo
		{
			public uint _hash;
			public uint _offset;
			public uint _ordinal;
			public uint _length;
			public uint _blockIndex;	//Change this for just compression mode at some point
			public string _name;
			public string _logicalName;
			public uint _modificationTime;
			public uint[]? _blocks;
			public byte[] _compressedData;
			public EBlockType GetBlockType(uint sectorSize)
			{
				if(_blocks == null) return EBlockType.kNone;
				if(0x7F * sectorSize < _length)
				{
					if(0x7FFF * sectorSize < _length)
					{
						return EBlockType.kLarge;
					}
					return EBlockType.kMedium;
				}
				return EBlockType.kSmall;
			}
		}

		static CoderPropID[] propIDs =
		{
			CoderPropID.DictionarySize,
			CoderPropID.PosStateBits,
			CoderPropID.LitContextBits,
			CoderPropID.LitPosBits,
			CoderPropID.Algorithm,
			CoderPropID.NumFastBytes,
			CoderPropID.EndMarker,
			CoderPropID.MatchFinder,
		};
		static object[] properties =
		{
			0x8000,
			2,
			3,
			0,
			2,
			0x80,
			false,
			"bt4",
		};


		public bool _loadNameTable;
		public bool _sequentialRead;
		public bool _loadingForIncrementalUpdate;
		public bool _enableCache;
		public bool _override;
		public igFileDescriptor _fileDescriptor;
		public bool _open;
		public bool _configured;
		public bool _needsEndianSwap = false;
		public Header _archiveHeader;
		[Obsolete("This exists for the reflection system, do not use.")] public igMemory<byte> _tocBuffer;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _fileIdTable;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _fileInfoTable;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _largeFileBlockTable;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _mediumFileBlockTable;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _smallFileBlockTable;
		[Obsolete("This exists for the reflection system, do not use.")] public igStringRefList? _nameTable;
		[Obsolete("This exists for the reflection system, do not use.")] public igStringRefList? _logicalNameTable;
		[Obsolete("This exists for the reflection system, do not use.")] public igUnsignedIntList? _modificationTimeTable;
		public List<FileInfo> _files = new List<FileInfo>();
		public string _nativeMedia;
		public string _nativePath;
		public static string _nativeAppPath;

		public void Open(string filePath, igBlockingType blockingType)
		{
			igFileContext.Singleton.Open(filePath, igFileContext.GetOpenFlags(FileAccess.ReadWrite, FileMode.Open), out _fileDescriptor, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			_path = _fileDescriptor._path;
			StreamHelper sh = new StreamHelper(_fileDescriptor._handle);
			sh.Seek(0);
			_archiveHeader._magicNumber = sh.ReadUInt32();
			if(_archiveHeader._magicNumber == 0x4947411A)
			{
				if(sh._endianness == StreamHelper.Endianness.Little) sh._endianness = StreamHelper.Endianness.Big;
				else                                                 sh._endianness = StreamHelper.Endianness.Little;
			}
			else if(_archiveHeader._magicNumber != 0x1A414749) throw new InvalidDataException($"{filePath} is not a valid igArchive.");
			if((sh._endianness == StreamHelper.Endianness.Little && !BitConverter.IsLittleEndian) || (sh._endianness == StreamHelper.Endianness.Big && BitConverter.IsLittleEndian)) _needsEndianSwap = true;
			_archiveHeader._version = sh.ReadUInt32();
			switch(_archiveHeader._version)
			{
				case 0x0B:
					_archiveHeader._tocSize = sh.ReadUInt32();
					_archiveHeader._numFiles = sh.ReadUInt32();
					_archiveHeader._sectorSize = sh.ReadUInt32();
					_archiveHeader._hashSearchDivider = sh.ReadUInt32();
					_archiveHeader._hashSearchSlop = sh.ReadUInt32();
					_archiveHeader._numLargeFileBlocks = sh.ReadUInt32();
					_archiveHeader._numMediumFileBlocks = sh.ReadUInt32();
					_archiveHeader._numSmallFileBlocks = sh.ReadUInt32();
					_archiveHeader._nameTableOffset = sh.ReadUInt64();
					_archiveHeader._nameTableSize = sh.ReadUInt32();
					_archiveHeader._flags = sh.ReadUInt32();
					break;
				default:
					throw new NotImplementedException($"Version {_archiveHeader._version} is not implemented");
			}
			_files.Capacity = (int)_archiveHeader._numFiles;
			for(int i = 0; i < _archiveHeader._numFiles; i++)
			{
				FileInfo fileInfo = new FileInfo();
				fileInfo._hash = sh.ReadUInt32();
				_files.Add(fileInfo);
			}
			for(int i = 0; i < _archiveHeader._numFiles; i++)
			{
				//technically the offset is 5 bytes and the ordinal is 3
				ulong temp = sh.ReadUInt64();	//????
				_files[i]._ordinal = (uint)(temp >> 40);
				_files[i]._offset = (uint)(temp & 0xFFFFFFFF);
				_files[i]._length = sh.ReadUInt32();
				_files[i]._blockIndex = sh.ReadUInt32();
			}
			for(int i = 0; i < _archiveHeader._numFiles; i++)
			{
				sh.Seek(_archiveHeader._nameTableOffset + (uint)i * 0x04);
				sh.Seek(_archiveHeader._nameTableOffset + sh.ReadUInt32());
				string name1 = sh.ReadString();

				string? name2 = null;
				if(_archiveHeader._version >= 0x0A)
				{
					name2 = sh.ReadString();
				}
				if(_archiveHeader._version >= 0x08)
				{
					_files[i]._modificationTime = sh.ReadUInt32();
				}
				if(_archiveHeader._version >= 0x0B)
				{
					_files[i]._name = name1;
					_files[i]._logicalName = name2;
				}
				else
				{
					_files[i]._logicalName = name1;
					_files[i]._name = name2;
				}			
			}
			uint blockInfoStart = GetHeaderSize() + _archiveHeader._numFiles * (0x04u + GetFileInfoSize());
			sh.Seek(blockInfoStart);
			uint[] largeBlockTable = sh.ReadStructArray<uint>(_archiveHeader._numLargeFileBlocks);
			ushort[] mediumBlockTable = sh.ReadStructArray<ushort>(_archiveHeader._numMediumFileBlocks);
			byte[] smallBlockTable = sh.ReadStructArray<byte>(_archiveHeader._numSmallFileBlocks);

			for(int i = 0; i < _files.Count; i++)
			{
				sh.Seek(_files[i]._offset);
				if(_files[i]._blockIndex == 0xFFFFFFFF)
				{
					_files[i]._compressedData = sh.ReadBytes(_files[i]._length);
					continue;
				}
				uint numSectors = 0;
				uint numBlocks = (_files[i]._length + 0x7FFF) >> 0xF;
				uint[] fixedBlocks = new uint[numBlocks];
				for(uint j = 0; j < numBlocks; j++)
				{
					uint blockIndex = (_files[i]._blockIndex & 0x0FFFFFFF) + j;
					bool isCompressed;
					uint block;
					if(0x7F * _archiveHeader._sectorSize < _files[i]._length)
					{
						if(0x7FFF * _archiveHeader._sectorSize < _files[i]._length)
						{
							block = largeBlockTable[blockIndex];
							isCompressed = (block >> 0x1F) == 1;
							block &= 0x7FFFFFFF;
							numSectors += (uint)(largeBlockTable[blockIndex + 1] & 0x7FFFFFFF) - block;
						}
						else
						{
							block = mediumBlockTable[blockIndex];
							isCompressed = (block >> 0x0F) == 1;
							block &= 0x7FFF;
							numSectors += (uint)(mediumBlockTable[blockIndex + 1] & 0x7FFF) - block;
						}
					}
					else
					{
						block = smallBlockTable[blockIndex];
						isCompressed = (block >> 0x07) == 1;
						block &= 0x7F;
						numSectors += (uint)(smallBlockTable[blockIndex + 1] & 0x7F) - block;
					}
					fixedBlocks[j] = (isCompressed ? 0x80000000u : 0u) | block;
				}
				_files[i]._blocks = fixedBlocks;
				_files[i]._compressedData = sh.ReadBytes(numSectors * _archiveHeader._sectorSize);
			}
			igFileContext.Singleton.Close(_fileDescriptor, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			_fileDescriptor = null;
		}
		public void Close(igBlockingType blockingType)
		{
			//igFileContext.Singleton._archiveMountManager.UnmountArchive(this);
		}
		public void Save(string filePath)
		{
			FileStream fs = File.Create(filePath);
			StreamHelper sh = new StreamHelper(fs, StreamHelper.Endianness.Big);
			sh.WriteUInt32(0x1A414749);
			sh.WriteUInt32(_archiveHeader._version);
			UpdateFileHashes();

			FillBlockBuffers(out List<byte> smallBlockTable, out List<ushort> mediumBlockTable, out List<uint> largeBlockTable);

			sh.Seek(GetHeaderSize());
			for(int i = 0; i < _files.Count; i++) sh.WriteUInt32(_files[i]._hash);

			_archiveHeader._numSmallFileBlocks = (uint)smallBlockTable.Count;
			_archiveHeader._numMediumFileBlocks = (uint)mediumBlockTable.Count;
			_archiveHeader._numLargeFileBlocks = (uint)largeBlockTable.Count;
			_archiveHeader._tocSize = _archiveHeader._numFiles * (0x04u + GetFileInfoSize()) + _archiveHeader._numSmallFileBlocks + (_archiveHeader._numMediumFileBlocks << 1) + (_archiveHeader._numLargeFileBlocks << 2);

			uint fileHeaderOffset = GetHeaderSize() + (_archiveHeader._numFiles << 2);
			sh.Seek(_archiveHeader._tocSize + GetHeaderSize());
			sh.Align(4);
			sh.WriteUInt32(0xFFFFFFFF);
			sh.WriteByte(0x00);
			sh.WriteByte(0x80);
			sh.WriteUInt16(0x0000);
			sh.WriteUInt64(0xFFFFFFFFFFFFFFFF);
			sh.WriteUInt64(0xFFFFFFFFFFFFFFFF);
			sh.Align(_archiveHeader._sectorSize);
			uint currentOffset = sh.Tell();

			IOrderedEnumerable<FileInfo> orderedFiles = _files.OrderBy(x => x._ordinal);
			for(int i = 0; i < _files.Count; i++)
			{
				orderedFiles.ElementAt(i)._offset = currentOffset = sh.Tell();
				sh.Seek(currentOffset);
				sh.BaseStream.Write(orderedFiles.ElementAt(i)._compressedData);
				sh.Align(_archiveHeader._sectorSize);
			}
			
			currentOffset = sh.Tell();

			for(int i = 0; i < _files.Count; i++)
			{
				sh.Seek(fileHeaderOffset + i * GetFileInfoSize());
				sh.WriteUInt64(((ulong)_files[i]._ordinal << 40) | _files[i]._offset);
				sh.WriteUInt32(_files[i]._length);
				sh.WriteUInt32(_files[i]._blockIndex);
			}

			_archiveHeader._nameTableOffset = currentOffset;
			currentOffset += (uint)_files.Count * 0x4u;
			for(int i = 0; i < _files.Count; i++)
			{
				sh.Seek(_archiveHeader._nameTableOffset + (uint)i * 4u);
				sh.WriteUInt32(currentOffset - (uint)_archiveHeader._nameTableOffset);
				sh.Seek(currentOffset);

				if(_archiveHeader._version <= 0x08)
				{
					sh.WriteString(_files[i]._name);
				}
				else if(_archiveHeader._version == 0x0A)
				{
					sh.WriteString(_files[i]._logicalName);
					sh.WriteString(_files[i]._name);
				}
				else
				{
					sh.WriteString(_files[i]._name);
					sh.WriteString(_files[i]._logicalName);
				}

				if(_archiveHeader._version >= 0x08)
				{
					sh.WriteUInt32(_files[i]._modificationTime);
				}
				currentOffset = sh.Tell();
			}
			_archiveHeader._nameTableSize = currentOffset - (uint)_archiveHeader._nameTableOffset;

			_archiveHeader._numFiles = (uint)_files.Count;
			CalculateHashSearchProperties();

			sh.Seek(GetHeaderSize() + (GetFileInfoSize() + 0x04u) * _archiveHeader._numFiles);
			for(int i = 0; i < largeBlockTable.Count; i++)  sh.WriteUInt32(largeBlockTable[i]);
			for(int i = 0; i < mediumBlockTable.Count; i++) sh.WriteUInt16(mediumBlockTable[i]);
			for(int i = 0; i < smallBlockTable.Count; i++)  sh.WriteByte(smallBlockTable[i]);

			sh.Seek(0x08);
			switch(_archiveHeader._version)
			{
				case 0x0B:
					sh.WriteUInt32(_archiveHeader._tocSize);
					sh.WriteUInt32(_archiveHeader._numFiles);
					sh.WriteUInt32(_archiveHeader._sectorSize);
					sh.WriteUInt32(_archiveHeader._hashSearchDivider);
					sh.WriteUInt32(_archiveHeader._hashSearchSlop);
					sh.WriteUInt32(_archiveHeader._numLargeFileBlocks);
					sh.WriteUInt32(_archiveHeader._numMediumFileBlocks);
					sh.WriteUInt32(_archiveHeader._numSmallFileBlocks);
					sh.WriteUInt64(_archiveHeader._nameTableOffset);
					sh.WriteUInt32(_archiveHeader._nameTableSize);
					sh.WriteUInt32(_archiveHeader._flags);
					break;
				default:
					throw new NotImplementedException($"Version {_archiveHeader._version} is not implemented");
			}
			fs.Flush();
			fs.Close();
		}
		private void UpdateFileHashes()
		{
			for(int i = 0; i < _files.Count; i++)
			{
				if(_files[i]._logicalName == null) return;

				_files[i]._hash = HashFilePath(_files[i]._logicalName);
			}
			_files.OrderBy(x => x._hash);
		}
		private uint HashFilePath(string filepath)
		{
			string pathToHash = filepath;
			if((_archiveHeader._flags & 1u) != 0)
			{
				pathToHash = pathToHash.Replace('\\', '/');
				pathToHash = pathToHash.ToLower();
			}
			if((_archiveHeader._flags & 2u) != 0) pathToHash = Path.GetFileName(pathToHash);
			pathToHash = pathToHash.TrimStart('/', '\\');
			return igHash.Hash(pathToHash);
		}
		private void FillBlockBuffers(out List<byte> smallBlockTable, out List<ushort> mediumBlockTable, out List<uint> largeBlockTable)
		{
			IOrderedEnumerable<FileInfo> files = _files.OrderBy(x => x._ordinal);
			smallBlockTable = new List<byte>();
			mediumBlockTable = new List<ushort>();
			largeBlockTable = new List<uint>();
			for(int i = 0; i < files.Count(); i++)
			{
				EBlockType blockType = files.ElementAt(i).GetBlockType(_archiveHeader._sectorSize);
				FileInfo file = files.ElementAt(i);
				file._blockIndex = file._blockIndex & 0xF0000000;
				uint numSectors;
				switch(blockType)
				{
#pragma warning disable CS8602
					case EBlockType.kSmall:
						file._blockIndex |= (uint)smallBlockTable.Count;
						smallBlockTable.Capacity += file._blocks.Length + 1;
						for(int j = 0; j < file._blocks.Length; j++)
						{
							smallBlockTable.Add((byte)((file._blocks[j] >> 24) | (file._blocks[j] & 0x7F)));
						}
						smallBlockTable.Add((byte)(StreamHelper.Align((uint)file._compressedData.Length, _archiveHeader._sectorSize) / _archiveHeader._sectorSize));
						break;
					case EBlockType.kMedium:
						file._blockIndex |= (uint)mediumBlockTable.Count;
						mediumBlockTable.Capacity += file._blocks.Length + 1;
						for(int j = 0; j < file._blocks.Length; j++)
						{
							mediumBlockTable.Add((ushort)((file._blocks[j] >> 16) | (file._blocks[j] & 0x7FFF)));
						}
						mediumBlockTable.Add((ushort)(StreamHelper.Align((uint)file._compressedData.Length, _archiveHeader._sectorSize) / _archiveHeader._sectorSize));
						break;
					case EBlockType.kLarge:
						file._blockIndex |= (uint)largeBlockTable.Count;
						largeBlockTable.Capacity += file._blocks.Length + 1;
						for(int j = 0; j < file._blocks.Length; j++)
						{
							largeBlockTable.Add(file._blocks[j]);
						}
						largeBlockTable.Add((uint)(StreamHelper.Align((uint)file._compressedData.Length, _archiveHeader._sectorSize) / _archiveHeader._sectorSize));
						break;
#pragma warning restore CS8602
					case EBlockType.kNone:
						file._blockIndex = 0xFFFFFFFF;
						break;
				}
			}
		}
		private byte GetFileInfoSize()
		{
			switch(_archiveHeader._version)
			{
				case 0x0B: return 0x10;
				default: throw new NotSupportedException($"IGA version {_archiveHeader._version} is unsupported");
			}
		}
		private byte GetHeaderSize()
		{
			switch(_archiveHeader._version)
			{
				case 0x0B: return 0x38;
				default: throw new NotSupportedException($"IGA version {_archiveHeader._version} is unsupported");
			}
		}
		public void Decompress(string path, Stream src) => Decompress(HashFilePath(path), src);
		public void Decompress(uint hash, Stream dst) => Decompress(_files[HashSearch(_files, (uint)_files.Count, _archiveHeader._hashSearchDivider, _archiveHeader._hashSearchSlop, hash)], dst);
		public void Decompress(FileInfo fileInfo, Stream dst)
		{
			if(fileInfo._blockIndex == 0xFFFFFFFF)
			{
				dst.Write(fileInfo._compressedData);
				dst.Flush();
				dst.Seek(0, SeekOrigin.Begin);
				return;
			}
			CompressionType type = (CompressionType)(fileInfo._blockIndex >> 28);
			byte[]? lzmaProps = null;
			if(type == CompressionType.kLzma)
			{
				lzmaProps = new byte[5];
			}
			for(int i = 0; i < fileInfo._blocks.Length; i++)
			{
				uint decompressedSize = (fileInfo._length < (i + 1) * 0x8000) ? fileInfo._length & 0x7FFF : 0x8000;
				bool shouldDecompress = (fileInfo._blocks[i] & 0x80000000u) != 0;
				uint offset = (fileInfo._blocks[i] & 0x7FFFFFFF) * _archiveHeader._sectorSize;
				if((fileInfo._blocks[i] & 0x80000000u) == 0)
				{
					dst.Write(fileInfo._compressedData, (int)offset, (int)decompressedSize);
					continue;
				}

				uint compressedSize = BitConverter.ToUInt16(fileInfo._compressedData, (int)offset);
				offset += 2;

				MemoryStream tempMs;
				switch(type)
				{
					case CompressionType.kZlib:
						tempMs = new MemoryStream(fileInfo._compressedData, (int)offset, (int)compressedSize);
						DeflateStream zstr = new DeflateStream(tempMs, CompressionMode.Decompress);
						zstr.CopyTo(dst);
						zstr.Close();
						tempMs.Close();
						break;
					case CompressionType.kLzma:
						Array.Copy(fileInfo._compressedData, offset, lzmaProps!, 0, 5);
						tempMs = new MemoryStream(fileInfo._compressedData, (int)offset + 5, (int)compressedSize);
						SevenZip.Compression.LZMA.Decoder dec = new SevenZip.Compression.LZMA.Decoder();
						dec.SetDecoderProperties(lzmaProps);
						dec.Code(tempMs, dst, compressedSize, decompressedSize, null);
						tempMs.Close();
						break;
					case CompressionType.kLz4:
						byte[] dest = new byte[decompressedSize];
						LZ4Codec.Decode(fileInfo._compressedData, (int)offset, (int)compressedSize, dest, 0, dest.Length);
						dst.Write(dest);
						break;
					default:
						throw new NotImplementedException($"Compression type 0x{type.ToString("X")} is unsupported");
				}
			}
			dst.Flush();
			dst.Seek(0, SeekOrigin.Begin);
		}
		public void Compress(string path, Stream src) => Compress(HashFilePath(path), src);
		public void Compress(uint hash, Stream src) => Compress(_files[HashSearch(_files, (uint)_files.Count, _archiveHeader._hashSearchDivider, _archiveHeader._hashSearchSlop, hash)], src);
		public void Compress(FileInfo fileInfo, Stream src)
		{
			src.Seek(0, SeekOrigin.Begin);
			fileInfo._length = (uint)src.Length;

			//Temp, for testing
			fileInfo._blockIndex = 0xFFFFFFFF;
			fileInfo._blocks = null;

			//Add in setting the modification time for the funny
			if(fileInfo._blockIndex == 0xFFFFFFFF)
			{
				fileInfo._compressedData = new byte[src.Length];
				src.Read(fileInfo._compressedData);
				return;
			}
			fileInfo._blocks = new uint[(src.Length + 0x7FFF) >> 0xF];
			CompressionType type = (CompressionType)(fileInfo._blockIndex >> 28);
			MemoryStream dst = new MemoryStream();
			for(uint processedBytes = 0, blockI = 0; processedBytes < src.Length; processedBytes += 0x8000, blockI++)
			{
				uint decompressedSize = (uint)src.Length - processedBytes;
				if(decompressedSize > 0x8000) decompressedSize = 0x8000;
				ushort compressedSize;
				MemoryStream tempMs = new MemoryStream();
				src.Seek(processedBytes, SeekOrigin.Begin);
				dst.Position = StreamHelper.Align((uint)dst.Position, _archiveHeader._sectorSize);
				fileInfo._blocks[blockI] = 0x80000000u | ((uint)dst.Position / _archiveHeader._sectorSize);
				switch(type)
				{
					case CompressionType.kLzma:
						SevenZip.Compression.LZMA.Encoder enc = new SevenZip.Compression.LZMA.Encoder();
						enc.SetCoderProperties(propIDs, properties);
						enc.WriteCoderProperties(tempMs);
						enc.Code(src, tempMs, decompressedSize, -1, null);
						compressedSize = (ushort)(tempMs.Length - 5);
						break;
					default:
						throw new NotImplementedException($"Compression for type {type} is not supported");
				}
				dst.WriteByte((byte)(compressedSize & 0xFF));
				dst.WriteByte((byte)(compressedSize >> 8));
				tempMs.Flush();
				tempMs.Seek(0, SeekOrigin.Begin);
				tempMs.WriteTo(dst);
				tempMs.Close();
			}
			//The memory stream buffer has extra zeroes at the end
			fileInfo._compressedData = new byte[dst.Length];
			dst.Flush();
			Array.Copy(dst.GetBuffer(), fileInfo._compressedData, fileInfo._compressedData.Length);
			File.WriteAllBytes("debugiga.dat", dst.GetBuffer());
		}
		public FileInfo GetAddFile(string filePath)
		{
			//check if the file already exists, return it if so
			uint hash = HashFilePath(filePath);
			int index = HashSearch(_files, _archiveHeader._numFiles, _archiveHeader._hashSearchDivider, _archiveHeader._hashSearchSlop, hash);
			if(index >= 0) return _files[index];

			//if not, generate a new one
			FileInfo file = new FileInfo();
			file._hash = hash;
			file._blockIndex = 0xFFFFFFFF;
			file._logicalName = filePath;
			file._name = $"Temporary/BuildServer/{igAlchemyCore.GetPlatformString(igRegistry.GetRegistry()._platform)}/Output/{filePath}";
			file._ordinal = _archiveHeader._numFiles;

			//figure out where to insert the new one
			int indexToInsertTo = (int)_archiveHeader._numFiles;
			for(int i = 0; i < _archiveHeader._numFiles; i++)
			{
				if(_files[i]._hash > hash)
				{
					indexToInsertTo = i;
					break;
				}
			}

			//insert the file and update the file header
			_archiveHeader._numFiles++;
			_files.Insert(indexToInsertTo, file);
			CalculateHashSearchProperties();
			return file;
		}
		//Reverse engineered by DTZxPorter
		private void CalculateHashSearchProperties()
		{
			_archiveHeader._hashSearchDivider = uint.MaxValue / _archiveHeader._numFiles;

			int TopMatchIndex = 0;
			for (int i = 0x0; i < _files.Count; i++)
			{
				int Matches = 0;

				for (int j = 0x0; j < _files.Count; j++)
				{
					if (HashSearch(_files, (uint)_files.Count, _archiveHeader._hashSearchDivider, (uint)i, _files[j]._hash) != -1)
						Matches++;
				}

				if (Matches == _files.Count)
				{
					TopMatchIndex = i;
					break;
				}
			}

			_archiveHeader._hashSearchSlop = (uint)TopMatchIndex;
		}
		//Reverse engineered by DTZxPorter
		private static int HashSearch(List<FileInfo> fileInfos, uint numFiles, uint hashSearchDivider, uint hashSearchSlop, uint fileId)
		{
			uint fileIdDivided = fileId / hashSearchDivider;
			uint searchAt = 0;
			if (hashSearchSlop < fileIdDivided)
				searchAt = (fileIdDivided - hashSearchSlop);

			fileIdDivided += hashSearchSlop + 1;
			if (fileIdDivided < numFiles)
				numFiles = fileIdDivided;

			uint index = searchAt;
			searchAt = (numFiles - index);
			uint i = searchAt;
			while (0 < i)
			{
				i = searchAt / 2;
				if (fileInfos[(int)(index + i)]._hash < fileId)
				{
					index += i + 1;
					i = searchAt - 1 - i;
				}
				searchAt = i;
			}

			if (index < fileInfos.Count && fileInfos[(int)index]._hash == fileId)
			{
				return (int)index;
			}

			return -1;
		}
		public bool HasFile(string path) => HasFile(HashFilePath(path));
		public bool HasFile(uint hash) => HashSearch(_files, (uint)_files.Count, _archiveHeader._hashSearchDivider, _archiveHeader._hashSearchSlop, hash) >= 0;
		public override void Exists(igFileWorkItem workItem)
		{
			if(HasFile(workItem._path))
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			else
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
		}
		public override void Open(igFileWorkItem workItem)
		{
			int fileId = HashSearch(_files, (uint)_files.Count, _archiveHeader._hashSearchDivider, _archiveHeader._hashSearchSlop, HashFilePath(workItem._path));
			if(fileId == -1)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
				return;
			}
			workItem._file._path = workItem._path;
			workItem._file._size = _files[fileId]._length;
			workItem._file._position = 0;
			workItem._file._handle = new MemoryStream((int)workItem._file._size);
			workItem._file._device = this;
			Decompress(_files[fileId], (MemoryStream)workItem._file._handle);
			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}
		public override void Close(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Read(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Write(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Truncate(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Mkdir(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Rmdir(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void GetFileList(igFileWorkItem workItem)
		{
			igStringRefList nameList = (igStringRefList)workItem._buffer;
			nameList.SetCapacity(_files.Count);
			for(int i = 0; i < _files.Count; i++)
			{
				nameList.Append(_files[i]._logicalName);
			}
			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}
		public override void GetFileListWithSizes(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Unlink(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Rename(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Prefetch(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Format(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
		public override void Commit(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
	}
	public class igArchiveList : igTObjectList<igArchive> {}
}