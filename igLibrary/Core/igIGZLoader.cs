/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igIGZLoader
	{
		public const uint _bigMagicCookie = 0x015A4749;
		public const uint _littleMagicCookie = 0x49475A01;
		public List<string> _stringList = new List<string>();
		public List<igMetaObject> _vtableList = new List<igMetaObject>();
		public Dictionary<ulong, igObject> _offsetObjectList = new Dictionary<ulong, igObject>();
		public List<ushort> _metaSizes = new List<ushort>();
		public List<igHandle> _externalList = new List<igHandle>();
		public List<igHandle> _namedHandleList = new List<igHandle>();
		public List<igHandle> _unresolvedNames = new List<igHandle>();
		public igObjectList _namedExternalList = new igObjectList();
		public List<Tuple<ulong, ulong>> _thumbnails = new List<Tuple<ulong, ulong>>();
		public igRuntimeFields _runtimeFields = new igRuntimeFields();
		public uint _version;
		public uint _metaObjectVersion;
		public IG_CORE_PLATFORM _platform;
		//public igFileDescriptor _file;
		public StreamHelper _stream;
		public igObjectDirectory _dir;
		public uint _fixups;
		public uint[] _loadedPointers = new uint[0x1F];
		public igMemoryPool[] _loadedPools = new igMemoryPool[0x1F];
		public ulong nameListOffset;
		private bool _readDependancies;
		public igFileDescriptor _fd;

		public igIGZLoader(igObjectDirectory dir, Stream stream, bool readDependancies)
		{
			_stream = new StreamHelper(stream);
		}

		public igIGZLoader(igObjectDirectory dir, string path, bool readDependancies)
		{
			igFile file = new igFile();
			file.Open(path);
			_fd = file._file;
			_stream = new StreamHelper(file._file._handle);
		}

		public void Read(igObjectDirectory dir, bool readDependancies)
		{
			_dir = dir;
			_stream.Seek(0);
			_readDependancies = readDependancies;

			uint magicCookie = _stream.ReadUInt32();
			     if(magicCookie == _bigMagicCookie)    _stream._endianness = StreamHelper.Endianness.Big;
			else if(magicCookie == _littleMagicCookie) _stream._endianness = StreamHelper.Endianness.Little;
			else                                       throw new InvalidDataException("IGZ Magic Cookie didn't match, the file could be corrupt or wasn't loaded correctly, either way, Quigley is coming to break your kneecaps, run.");

			_version = _stream.ReadUInt32();
			_metaObjectVersion = _stream.ReadUInt32();
			
			igMetaEnum platformEnum = igArkCore.GetMetaEnum("IG_CORE_PLATFORM");
			_platform = (IG_CORE_PLATFORM)platformEnum.GetEnumFromValue(_stream.ReadInt32());	//_platform
			uint numFixups = _stream.ReadUInt32();

			ParseSections();
			ProcessFixupSections(dir, numFixups);
			ReadObjects();
		}
		public void ReadObjects()
		{
			foreach(KeyValuePair<ulong, igObject> offsetObject in _offsetObjectList)
			{
				_stream.Seek(DeserializeOffset(offsetObject.Key));
				offsetObject.Value.ReadIGZFields(this);
			}
		}

		public void ParseSections()
		{
			int sectionCount = 0;
			for(int i = 0; i < 0x20; i++)
			{
				_stream.Seek(0x14 + 0x10 * i);
				uint memPoolName = _stream.ReadUInt32();
				uint offset = _stream.ReadUInt32();
				uint length = _stream.ReadUInt32();
				uint alignment = _stream.ReadUInt32();

				if(offset == 0)
				{
					sectionCount = i;
					break;
				}

				_stream.Seek(0x224 + memPoolName);
				string memoryPoolName = _stream.ReadString();
				if(i > 0)
				{
					_loadedPools[i - 1] = igMemoryContext.Singleton.GetMemoryPoolByName(memoryPoolName);
				}

				if(i > 0) _loadedPointers[i - 1] = offset;
				else      _fixups = offset;
			}
		}

		public void ProcessFixupSections(igObjectDirectory dir, uint numFixups)
		{
			uint bytesProcessed = 0;
			for(int i = 0; i < numFixups; i++)
			{
				_stream.Seek(_fixups + bytesProcessed);
				uint magic = _stream.ReadUInt32();
				uint count = _stream.ReadUInt32();
				uint length = _stream.ReadUInt32();
				uint start = _stream.ReadUInt32();
				_stream.Seek(_fixups + bytesProcessed + start);

				switch(magic)
				{
					case 0x50454454:							//TDEP
						if(!_readDependancies) break;
						_dir._dependencies.Capacity = (int)count;
						for(uint j = 0; j < count; j++)
						{
							string nameStr = _stream.ReadString();
							string pathStr = _stream.ReadString();
							if(pathStr.StartsWith("<build>")) continue;
							igName name = new igName(nameStr);
							igObjectDirectory? dependantDir = igObjectDirectory.LoadDependancyDefault(pathStr, name, igBlockingType.kBlocking);
							if(dependantDir == null) throw new FileNotFoundException($"Failed to find dependancy {pathStr}");

							dir._dependencies.Add(dependantDir);
						}
						break;
					case 0x54454D54:							//TMET
						_vtableList.Capacity = (int)count;
						for(uint j = 0; j < count; j++)
						{
							long basePos = _stream.BaseStream.Position;
							string vtableName = _stream.ReadString();

							igMetaObject vtable = igArkCore.GetObjectMeta(vtableName);
							if(vtable != null) _vtableList.Add(vtable);
							else               throw new TypeLoadException("Failed to find type \"" + vtableName + "\". This type may be platform specific or loaded from a game script, please contact NefariousTechSupport.");

							vtable.GatherDependancies();
							vtable.CalculateOffsetForPlatform(_platform);

							int bits = (_version > 7) ? 2 : 1;
							_stream.Seek(basePos + bits + ((_stream.BaseStream.Position - basePos - 1) & (uint)(-bits)));
						}
						igArkCore.FlushPendingTypes();
						break;
					case 0x52545354:							//TSTR
						_stringList.Capacity = (int)count;
						for(uint j = 0; j < count; j++)
						{
							long basePos = _stream.BaseStream.Position;
							string data = _stream.ReadString();

							_stringList.Add(data);

							int bits = (_version > 7) ? 2 : 1;
							_stream.Seek(basePos + bits + ((_stream.BaseStream.Position - basePos - 1) & (uint)(-bits)));
						}
						break;
					case 0x44495845:							//EXID
						_externalList.Capacity = (int)count;
						for(uint j = 0; j < count; j++)
						{
							igHandleName depName = new igHandleName();
							uint rawDepNameHash = _stream.ReadUInt32();
							uint rawDepNSHash = _stream.ReadUInt32();
							depName._name._hash = rawDepNameHash;
							depName._ns._hash = rawDepNSHash;

							igObject? obj = null;
							if(!igObjectStreamManager.Singleton._directoriesByName.TryGetValue(depName._ns._hash, out igObjectDirectoryList? list))
							{
								Logging.Warn("igIGZ EXID load: Failed to find namespace {0}, referenced in {1}", depName._ns._hash.ToString("X08"), _dir._path);
								goto finish;
							}
							for(int d = 0; d < list._count; d++)
							{
								igObjectDirectory dependantDir = list[d];
								if(dependantDir._useNameList)
								{
									for(int k = 0; k < dependantDir._nameList!._count; k++)
									{
										if(dependantDir._nameList[k]._hash == depName._name._hash)
										{
											obj = dependantDir._objectList[k];
											break;
										}
									}
								}
								if(obj != null) break;
							}
							if(obj == null)
							{
								Logging.Warn("igIGZ EXID load: Failed to find object {0} in {1}, referenced in {2}", depName._name._hash.ToString("X08"), list[0]._name._string, _dir._path);
							}
							finish:
								_externalList.Add(new igHandle(depName));
						}
						break;
					case 0x4D4E5845:							//EXNM
						for(uint j = 0; j < count; j++)
						{
							igHandleName depHandleName = new igHandleName();
							ulong rawHandle = _stream.ReadUInt64();
							uint nsStrIndex = (uint)(rawHandle >> 32);
							uint nameStrIndex = unchecked((uint)rawHandle);
							depHandleName._ns.SetString(_stringList[(int)(nsStrIndex & 0x7FFFFFFF)]);
							depHandleName._name.SetString(_stringList[(int)(nameStrIndex & 0x7FFFFFFF)]);

							igObject? obj = null;
							if(dir._dependencies.Any(x => x._name._hash == depHandleName._ns._hash))
							{
								igObjectDirectory dependantDir = dir._dependencies.First(x => x._name._hash == depHandleName._ns._hash);
								if(dependantDir._useNameList)
								{
									for(int k = 0; k < dependantDir._nameList._count; k++)
									{
										if(dependantDir._nameList[k]._hash == depHandleName._name._hash)
										{
											obj = dependantDir._objectList[k];
											break;
										}
									}
								}
							}

							if((nsStrIndex & 0x80000000) != 0)
							{
								igHandle hnd = new igHandle(depHandleName);
								_namedHandleList.Add(hnd);
							}
							else
							{
								igObject? reference = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(depHandleName, null);
								if(reference == null)
								{
									igHandle hnd = new igHandle(depHandleName);
									reference = hnd.GetObjectAlias<igObject>();
								}
								_namedExternalList.Append(reference);
							}
						}
						break;
					case 0x4E484D54:							//TMHN
						_thumbnails.Capacity = (int)count;
						for(uint j = 0; j < count; j++)
						{
							ulong size = ReadRawOffset();
							ulong raw = ReadRawOffset();
							_thumbnails.Add(new Tuple<ulong, ulong>(size, raw));
						}
						break;
					case 0x42545652:							//RVTB
						UnpackCompressedInts(_runtimeFields._vtables, _stream.ReadBytes(length - start), count, false);
						InstantiateAndAppendObjects();
						break;
					case 0x544F4F52:							//ROOT
						UnpackCompressedInts(_runtimeFields._objectLists, _stream.ReadBytes(length - start), count, false);
						_dir._objectList = (igObjectList)_offsetObjectList[_runtimeFields._objectLists[0]];
						break;
					case 0x53464F52:							//ROFS
						UnpackCompressedInts(_runtimeFields._offsets, _stream.ReadBytes(length - start), count);
						break;
					case 0x44495052:							//RPID
						UnpackCompressedInts(_runtimeFields._poolIds, _stream.ReadBytes(length - start), count);
						break;
					case 0x54545352:							//RSTT
						UnpackCompressedInts(_runtimeFields._stringTables, _stream.ReadBytes(length - start), count);
						break;
					case 0x52545352:							//RSTR
						UnpackCompressedInts(_runtimeFields._stringRefs, _stream.ReadBytes(length - start), count);
						break;
					case 0x4E484D52:							//RMHN
						UnpackCompressedInts(_runtimeFields._memoryHandles, _stream.ReadBytes(length - start), count);
						break;
					case 0x54584552:							//REXT
						UnpackCompressedInts(_runtimeFields._externals, _stream.ReadBytes(length - start), count);
						break;
					case 0x58454E52:							//RNEX
						UnpackCompressedInts(_runtimeFields._namedExternals, _stream.ReadBytes(length - start), count);
						break;
					case 0x444E4852:							//RHND
						UnpackCompressedInts(_runtimeFields._handles, _stream.ReadBytes(length - start), count);
						break;
					case 0x4D414E4F:							//ONAM
						_dir._useNameList = true;
						_dir._nameList = (igNameList)_offsetObjectList[_stream.ReadUInt32()];
						break;
				}

				bytesProcessed += length;
			}
		}
		public void UnpackCompressedInts(List<ulong> list, byte[] bytes, uint count, bool deserialize = true)
		{
			list.Capacity = (int)count;
			uint previousInt = 0;

			bool shiftMoveOrMask = false;

			unsafe
			{
				fixed(byte *fixedData = bytes)
				{
					byte* data = fixedData;
					for (int i = 0; i < count; i++)
					{
						uint currentByte;

						if (!shiftMoveOrMask)
						{
							currentByte = (uint)*data & 0xf;
							shiftMoveOrMask = true;
						}
						else
						{
							currentByte = (uint)(*data >> 4);
							data++;
							shiftMoveOrMask = false;
						}
						byte shiftAmount = 3;
						uint unpackedInt = currentByte & 7;
						while ((currentByte & 8) != 0)
						{
							if (!shiftMoveOrMask)
							{
								currentByte = (uint)*data & 0xf;
								shiftMoveOrMask = true;
							}
							else
							{
								currentByte = (uint)(*data >> 4);
								data++;
								shiftMoveOrMask = false;
							}
							unpackedInt = unpackedInt | (currentByte & 7) << (byte)(shiftAmount & 0x1f);
							shiftAmount += 3;
						}

						previousInt = (uint)(previousInt + (unpackedInt * 4) + (_version < 9 ? 4 : 0));
						if(deserialize)
						{
							list.Add(DeserializeOffset(previousInt));
						}
						else
						{
							list.Add(previousInt);
						}
					}
				}
			}
		}

		public ulong DeserializeOffset(ulong offset)
		{
			if(_version <= 0x06) return (_loadedPointers[(offset >> 0x18)] + (offset & 0x00FFFFFF));
			else                 return (_loadedPointers[(offset >> 0x1B)] + (offset & 0x00FFFFFF));
		}
		public igMemoryPool GetMemoryPoolFromSerializedOffset(ulong offset)
		{
			if(_version <= 0x06) return _loadedPools[(offset >> 0x18)];
			else                 return _loadedPools[(offset >> 0x1B)];

		}
		public igMemoryPool GetMemoryPoolFromDeserializedOffset(ulong offset)
		{
			//This is a hack, should come up with a better way, only used in RVTB, so perhaps should move instantiating objects to processing of that fixup
			return GetMemoryPoolFromSerializedOffset(SerializeOffset(offset));
		}
		private ulong SerializeOffset(ulong offset)
		{
			uint poolIndex = (uint)Array.FindIndex<uint>(_loadedPointers, x => x > offset) - 1;
			ulong offsetInPool = offset - _loadedPointers[poolIndex];

			if(_version <= 0x06) return ((poolIndex << 0x18) + (offsetInPool & 0x00FFFFFF));
			else                 return ((poolIndex << 0x1B) + (offsetInPool & 0x07FFFFFF));
		}

		public ulong ReadRawOffset()
		{
			//Should be replaced with igSizeTypeMetaField
			if(igAlchemyCore.isPlatform64Bit(_platform)) return _stream.ReadUInt64();
			                                      return _stream.ReadUInt32();
		}

		public void InstantiateAndAppendObjects()
		{
			for(int i = 0; i < _runtimeFields._vtables.Count; i++)
			{
				_offsetObjectList.Add(_runtimeFields._vtables[i], InstantiateObject(_runtimeFields._vtables[i]));
			}
		}

		public igObject InstantiateObject(ulong offset)
		{
			_stream.Seek(DeserializeOffset(offset));
			int index = (int)ReadRawOffset();
			igObject obj = _vtableList[index].ConstructInstance(GetMemoryPoolFromSerializedOffset(offset), false);

			if(obj is igBlindObject blindObj)
			{
				blindObj.Initialize(_vtableList[index]);
			}

			return obj;
		}
	}
}