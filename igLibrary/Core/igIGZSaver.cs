using System.Collections.Generic;

namespace igLibrary.Core
{
	public class igIGZSaver
	{
		//public Dictionary<igObject, ulong> _objectOffsetList = new Dictionary<igObject, ulong>();
		public List<igMetaObject> _vTableList = new List<igMetaObject>();
		public List<string> _stringList = new List<string>();
		public Dictionary<string, uint> _stringRefList = new Dictionary<string, uint>();
		public List<Tuple<ulong, ulong>> _thumbnails = new List<Tuple<ulong, ulong>>();
		private List<SaverSection> _sections = new List<SaverSection>();
		public List<(igHandle, bool)> _namedList = new List<(igHandle, bool)>();	//If true then namedHandle, otherwise named external
		public List<igHandle> _externalList = new List<igHandle>();
		public List<(string, string)> _buildDependancies = new List<(string, string)>();
		public IG_CORE_PLATFORM _platform;
		public igObjectDirectory _dir;
		private StreamHelper _stream;
		public uint _version;
		private uint _fixupCount;
		private uint _fixupSize;
		private ulong _nameListOffset = 0;

		public class SaverSection
		{
			public StreamHelper _sh;
			public igMemoryPool _pool;
			public igRuntimeFields _runtimeFields;
			public Dictionary<igObject, ulong> _objectOffsetList;
			public uint _fileOffset;
			public uint _fileSize;
			public uint _index;
			public uint _alignment;
			private IG_CORE_PLATFORM _platform;
			public SaverSection(igMemoryPool pool, IG_CORE_PLATFORM platform)
			{
				if(pool == null) throw new ArgumentNullException("Memory pool cannot be null!");
				_sh = new StreamHelper(new MemoryStream(), igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big: StreamHelper.Endianness.Little);
				_pool = pool;
				_platform = platform;
				_runtimeFields = new igRuntimeFields();
				_objectOffsetList = new Dictionary<igObject, ulong>();
			}
			public ulong FindFreeMemory(ushort alignment) => Align((uint)_sh.BaseStream.Length, alignment);
			public ulong Align(ulong input, uint alignment)
			{
				return (ulong)(((input + (alignment - 1)) / alignment) * alignment);
			}
			public ulong Malloc(uint size) => MallocAligned(size, 1);
			public ulong MallocAligned(uint size, ushort alignment)
			{
				ulong offset = FindFreeMemory(alignment);
				_sh.Seek(offset);
				for(int i = 0; i < size; i++)
				{
					_sh.WriteByte(0);
				}
				return offset;
			}
			public void PushAlignment(uint alignment)
			{
				if(_alignment < alignment) _alignment = alignment;
			}
		}
		public void WriteFile(igObjectDirectory dir, string path, IG_CORE_PLATFORM platform)
		{
			FileStream fs = File.Create(path);
			WriteFile(dir, fs, platform);
			fs.Close();
		}
		public void WriteFile(igObjectDirectory dir, Stream dst, IG_CORE_PLATFORM platform)
		{
			_platform = platform;
			_version = 0x09;
			_stream = new StreamHelper(dst, igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big : StreamHelper.Endianness.Little);
			_dir = dir;

			SaverSection rootSection = GetSaverSection(dir._objectList.internalMemoryPool);
			rootSection._runtimeFields._objectLists.Add(SaveObject(dir._objectList));
			if(dir._useNameList)
			{
				_nameListOffset = SaveObject(dir._nameList);
			}

			if(_vTableList.Any(x => x._name == "igLocalizedInfo"))
			{
				//This isn't accurate to how the game does it
				string formatStr = Path.ChangeExtension(_dir._path, null).ReplaceBeginning("data:/", $"cwd:/Temporary/BuildServer/{igAlchemyCore.GetPlatformString(_platform)}/Output/") + "_{0}.lng";
				AddBuildDependency(string.Format(formatStr, "da"));
				AddBuildDependency(string.Format(formatStr, "de"));
				AddBuildDependency(string.Format(formatStr, "en"));
				AddBuildDependency(string.Format(formatStr, "es"));
				AddBuildDependency(string.Format(formatStr, "fi"));
				AddBuildDependency(string.Format(formatStr, "fr"));
				AddBuildDependency(string.Format(formatStr, "it"));
				AddBuildDependency(string.Format(formatStr, "mx"));
				AddBuildDependency(string.Format(formatStr, "nl"));
				AddBuildDependency(string.Format(formatStr, "no"));
				AddBuildDependency(string.Format(formatStr, "pt"));
				AddBuildDependency(string.Format(formatStr, "sv"));
			}

			WriteFixupSections(dir);
			WriteHeader();

			WriteOutSections();

			_stream.Seek(0);
		}

		public ulong SaveObject(igObject? obj)
		{
			ulong offset = SaveObjectShallow(obj, out bool needsDeep);
			if(needsDeep)
			{
				SaveObjectDeep(offset, obj);
			}
			return offset;
		}
		public ulong SaveObjectShallow(igObject? obj, out bool needsDeep)
		{
			if(obj == null)
			{
				needsDeep = false;
				return 0;
			}

			SaverSection section = GetSaverSection(obj.internalMemoryPool);

			bool previouslyWritten = section._objectOffsetList.TryGetValue(obj, out ulong offset);
			if(previouslyWritten)
			{
				needsDeep = false;
				return offset;
			}

			igMetaObject meta = obj.GetMeta();

			meta.CalculateOffsetForPlatform(_platform);

			offset = section.MallocAligned(meta._sizes[_platform], meta._alignments[_platform]);
			section._sh.Seek(offset);

			section._objectOffsetList.Add(obj, offset);
			WriteVTable(meta, section);
			section._sh.WriteUInt32(0);

			section._sh.Seek(offset);

			needsDeep = true;

			return offset | (section._index << (_version >= 7 ? 0x1B : 0x18));
		}
		public int GetOrAddHandle((igHandle, bool) named)
		{
			int index = _namedList.FindIndex(x => x.Item1 == named.Item1 && x.Item2 == named.Item2);
			if(index < 0)
			{
				index = _namedList.Count;
				_namedList.Add(named);
			}
			int properIndex = index;
			for(int i = 0; i < index; i++)
			{
				if(_namedList[i].Item2 != named.Item2) properIndex--;
			}
			return properIndex;
		}
		public ulong SaveObjectDeep(ulong serialized, igObject obj)
		{
			GetOffsetBad(serialized, out igIGZSaver.SaverSection section, out ulong deserialized);
			bool found = section._objectOffsetList.TryGetValue(obj, out ulong offset);
			if(!found)
			{
				throw new KeyNotFoundException("Failed to find saved object somehow");
			}
			section._sh.Seek(deserialized);
			obj.WriteIGZFields(this, section);
			return offset;
		}
		public void RefObject(igObject? obj)
		{
			SaverSection section = GetSaverSection(obj.internalMemoryPool);
			ulong offset = section._objectOffsetList[obj];
			section._sh.Seek(offset + igAlchemyCore.GetPointerSize(_platform));
			uint refCount = section._sh.ReadUInt32();
			section._sh.Seek(offset + igAlchemyCore.GetPointerSize(_platform));
			section._sh.WriteUInt32(refCount + 1);
		}

		private void WriteHeader()
		{
			_stream.Seek(0);
			_stream.WriteUInt32(igIGZLoader._littleMagicCookie);
			_stream.WriteUInt32(_version);
			_stream.WriteUInt32(0);			//SerializableFieldsHash
			_stream.WriteInt32(igArkCore.GetMetaEnum("IG_CORE_PLATFORM").GetValueFromEnum(_platform));
			_stream.WriteUInt32(_fixupCount);
		}
		private void WriteOutSections()
		{
			int memPoolNameOffset = 0;
			uint memoryOffset = 0x800 + _fixupSize;

			//Write out fixup section
			_stream.Seek(0x14);
			_stream.WriteUInt32(0);
			_stream.WriteUInt32(0x800);
			_stream.WriteUInt32(_fixupSize);
			_stream.WriteUInt32(0x800);

			for(int i = 0; i < _sections.Count; i++)
			{
				_stream.Seek(0x24 + i * 0x10);
				uint unknown = 0;
				_stream.WriteUInt32((uint)(memPoolNameOffset & 0xFFFF) | ((unknown << 16) & 0xFFFF));
				_stream.WriteUInt32(memoryOffset);
				uint sectionSize = (uint)_sections[i]._sh.BaseStream.Length;
				_stream.WriteUInt32(sectionSize);
				_stream.WriteUInt32(_sections[i]._alignment);

				_stream.Seek(memoryOffset);
				_sections[i]._sh.BaseStream.Flush();
				_sections[i]._sh.Seek(0);
				_sections[i]._sh.BaseStream.CopyTo(_stream.BaseStream);

				memPoolNameOffset += _sections[i]._pool._name.Length + 1;	//Don't forget the null byte!
				memoryOffset += sectionSize;

				_sections[i]._sh.BaseStream.Close();
			}

			_stream.Seek(0x24 + 0x10 * 0x20);
			for(int i = 0; i < _sections.Count; i++)
			{
				_stream.WriteString(_sections[i]._pool._name);
			}
		}
		private void WriteVTable(igMetaObject meta, SaverSection section)
		{
			section.PushAlignment(igAlchemyCore.GetPointerSize(_platform));
			int index = _vTableList.FindIndex(x => x == meta);
			if(index < 0)
			{
				index = _vTableList.Count;
				_vTableList.Add(meta);
				if(meta is DotNet.igDotNetDynamicMetaObject dndmo && dndmo._owner._path != "scripts:/common.vvl")
				{
					AddBuildDependency(dndmo._owner._path);
				}
			}
			section._runtimeFields._vtables.Add(section._sh.Tell());
			WriteRawOffset((ulong)index, section);
		}

		public void GetOffsetBad(ulong serialized, out igIGZSaver.SaverSection section, out ulong deserialized)
		{
			if(_version <= 0x06)
			{
				deserialized = serialized & 0x00FFFFFF;
				section = _sections[(int)(serialized >> 0x18)];
			}
			else
			{
				deserialized = serialized & 0x07FFFFFF;
				section = _sections[(int)(serialized >> 0x1B)];
			}
		}
		public SaverSection GetSaverSection(igMemoryPool pool)
		{
			int index = _sections.FindIndex(x => x._pool == pool);
			SaverSection ret;
			if(index < 0)
			{
				ret = new SaverSection(pool, _platform);
				ret._index = (uint)_sections.Count;
				_sections.Add(ret);
			}
			else
			{
				ret = _sections[index];
			}
			return ret;
		}
		public uint SerializeOffset(uint offset, SaverSection section) => SerializeOffset(offset, section._index);
		public uint SerializeOffset(uint offset, uint index)
		{
			if(_version <= 0x06) return offset | (index << 0x18);
			else                 return offset | (index << 0x1B);
		}
		public void AddBuildDependency(string path)
		{
			AddDependencyInternal(_buildDependancies, path);
		}
		public void AddFileDependency(string path)
		{
			return;
		}
		private void AddDependencyInternal(List<(string, string)> depList, string path)
		{
			if(!depList.Any(x => x.Item2 == path))
			{
				depList.Add((Path.GetFileNameWithoutExtension(path), path));
			}
		}
		public void WriteFixupSections(igObjectDirectory dir)
		{
			ulong endOffset = 0x800;
			ulong startOffset = 0x800;
			_stream.Seek(startOffset);

			ulong dependancyStartOffset = 0x800;

			//Dependancy list can't be autogenerated, cry
			if(dir._dependencies.Count > 0)
			{
				List<(string, string)> depList = new List<(string, string)>();
				for(int j = 0; j < dir._dependencies.Count; j++)
				{
					depList.Add((dir._dependencies[j]._name._string, dir._dependencies[j]._path));
				}
				depList = depList.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
				//List<(string, string)> buildDepList = _buildDependancies.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
				List<(string, string)> buildDepList = new List<(string, string)>();//_buildDependancies.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
				List<(string, string)> sortedDepList = new List<(string, string)>();

				int depIndex = 0;
				int bDepIndex = 0;
				while(depIndex < depList.Count || bDepIndex < buildDepList.Count)
				{
					if(depIndex >= depList.Count) goto addBuildDep;
					if(bDepIndex >= buildDepList.Count) goto addDep;

					int cmp = string.Compare(depList[depIndex].Item1, buildDepList[bDepIndex].Item1, false);
					if(cmp < 0) goto addDep;
					if(cmp > 0) goto addBuildDep;

					cmp = string.Compare(depList[depIndex].Item2, buildDepList[bDepIndex].Item2, false);
					if(cmp < 0) goto addDep;
					if(cmp > 0) goto addBuildDep;

					bDepIndex++;

					addDep:
						sortedDepList.Add(depList[depIndex]);
						depIndex++;
						continue;

					addBuildDep:
						sortedDepList.Add((buildDepList[bDepIndex].Item1, "<build>" + buildDepList[bDepIndex].Item2));
						bDepIndex++;
						continue;
				}

				startOffset = endOffset;
				_stream.WriteUInt32(0x50454454);
				_stream.WriteInt32(sortedDepList.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);

				for(int j = 0; j < sortedDepList.Count; j++)
				{
					_stream.WriteString(sortedDepList[j].Item1);
					_stream.WriteString(sortedDepList[j].Item2);
				}

				endOffset = Align(_stream.Tell(), 4);
				if(endOffset == _stream.Tell())
				{
					endOffset += 4;
				}
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);
				dependancyStartOffset = endOffset;
				_fixupCount++;
			}
			/*if(_namedExternalList.Count > 0)
			{
				dir._dependancies.Clear();
				for(int i = 0; i < _namedExternalList.Count; i++)
				{
					if(igObjectHandleManager.Singleton.IsSystemObject(_namedExternalList[i]))
					{
						continue;
					}
					igObjectDirectory dependantDir = igObjectStreamManager.Singleton._directories[_namedExternalList[i]._namespace._hash];
					dir._dependancies.Add(dependantDir);
				}

				startOffset = endOffset;
				_stream.WriteUInt32(0x50454454);
				_stream.WriteInt32(dir._dependancies.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);

				for(int j = 0; j < dir._dependancies.Count; j++)
				{
					_stream.WriteString(dir._dependancies[j]._name._string);
					_stream.WriteString(dir._dependancies[j]._path);
				}

				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);
				dependancyStartOffset = endOffset;
			}*/

			//TMET
			if(_vTableList.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x54454D54);
				_stream.WriteInt32(_vTableList.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _vTableList.Count; j++)
				{
					long basePos = _stream.BaseStream.Position;
					_stream.WriteString(_vTableList[j]._name);

					int bits = (_version > 7) ? 2 : 1;
					_stream.Seek(basePos + bits + (_vTableList[j]._name.Length & (uint)(-bits)));
				}
				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);

				startOffset = endOffset;
				_stream.WriteUInt32(0x5A53544D);
				_stream.WriteInt32(_vTableList.Count);
				_stream.WriteInt32(_vTableList.Count * 4 + 0x10);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _vTableList.Count; j++)
				{
					_stream.WriteUInt32(_vTableList[j]._sizes[_platform]);
				}
				endOffset = startOffset + 0x10u + (uint)_vTableList.Count * 4u;
				_stream.Seek(endOffset);
				_fixupCount += 2;
			}

			if(_externalList.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x44495845);
				_stream.WriteInt32(_externalList.Count);
				_stream.WriteUInt32((uint)_externalList.Count * 8 + 0x10);
				_stream.WriteUInt32(0x10);
				for(int j = 0; j < _externalList.Count; j++)
				{
					_stream.WriteUInt32(_externalList[j]._alias._hash);
					_stream.WriteUInt32(_externalList[j]._namespace._hash);
				}
				endOffset = startOffset + 0x10u + (uint)_externalList.Count * 8u;
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			if(_namedList.Count > 0)
			{
				startOffset = endOffset;
				int externalCount = _namedList.Count;
				_stream.WriteUInt32(0x4D4E5845);
				_stream.WriteInt32(externalCount);
				uint alignedDataStart = (uint)Align(_stream.Tell()+8 - (uint)startOffset, igAlchemyCore.GetPointerSize(_platform));
				_stream.WriteUInt32((uint)externalCount * igAlchemyCore.GetPointerSize(_platform) * 2u + alignedDataStart);
				_stream.WriteUInt32(alignedDataStart);
				_stream.Seek(startOffset + alignedDataStart);

				for(int j = 0; j < _namedList.Count; j++)
				{
					_stream.WriteUInt32((uint)AddString(_namedList[j].Item1._namespace._string) | (_namedList[j].Item2 ? 0x80000000u : 0u));
					_stream.WriteUInt32((uint)AddString(_namedList[j].Item1._alias._string));
				}

				endOffset = _stream.Tell();
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			//TSTR
			if(_stringList.Count > 0)
			{
				startOffset = dependancyStartOffset;
				_stream.Seek(startOffset);
				byte[] otherFixups = new byte[endOffset - startOffset];
				_stream.Read(otherFixups);
				_stream.Seek(startOffset);
				for(int i = 0; i < otherFixups.Length; i++)
				{
					_stream.WriteByte(0);
				}
				_stream.Seek(startOffset);

				_stream.WriteUInt32(0x52545354);
				_stream.WriteInt32(_stringList.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _stringList.Count; j++)
				{
					long basePos = _stream.BaseStream.Position;
					_stream.WriteString(_stringList[j]);

					int bits = (_version > 7) ? 2 : 1;
					_stream.Seek(basePos + bits + (_stringList[j].Length & (uint)(-bits)));
				}
				ulong tempEndOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(tempEndOffset - startOffset));
				_stream.Seek(tempEndOffset);
				_stream.BaseStream.Write(otherFixups);				
				endOffset = _stream.Tell64();
				//_stream.Seek(endOffset + tempEndOffset - 0x800);
				_fixupCount++;
			}

			if(_thumbnails.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x4E484D54);
				_stream.WriteInt32(_thumbnails.Count);
				uint alignedDataStart = (uint)Align(_stream.Tell()+8 - (uint)startOffset, igAlchemyCore.GetPointerSize(_platform));
				_stream.WriteUInt32((uint)_thumbnails.Count * igAlchemyCore.GetPointerSize(_platform) * 2u + alignedDataStart);
				_stream.WriteUInt32(alignedDataStart);
				for(int j = 0; j < _thumbnails.Count; j++)
				{
					if(igAlchemyCore.isPlatform64Bit(_platform))
					{
						_stream.WriteUInt64(_thumbnails[j].Item1);
						_stream.WriteUInt64(_thumbnails[j].Item2);
					}
					else
					{
						_stream.WriteUInt32((uint)_thumbnails[j].Item1);
						_stream.WriteUInt32((uint)_thumbnails[j].Item2);
					}
				}
				endOffset = _stream.Tell();
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			WriteRuntimeFixup(0x42545652, ref startOffset, ref endOffset, x => x._runtimeFields._vtables);			//RVTB
			WriteRuntimeFixup(0x52545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringRefs);		//RSTR
			WriteRuntimeFixup(0x54545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringTables);		//RSTT
			WriteRuntimeFixup(0x53464F52, ref startOffset, ref endOffset, x => x._runtimeFields._offsets);			//ROFS
			WriteRuntimeFixup(0x44495052, ref startOffset, ref endOffset, x => x._runtimeFields._poolIds);			//RPID
			WriteRuntimeFixup(0x54584552, ref startOffset, ref endOffset, x => x._runtimeFields._externals);		//REXT
			WriteRuntimeFixup(0x444E4852, ref startOffset, ref endOffset, x => x._runtimeFields._handles);			//RHND
			WriteRuntimeFixup(0x58454E52, ref startOffset, ref endOffset, x => x._runtimeFields._namedExternals);	//RNEX
			WriteRuntimeFixup(0x4E484D52, ref startOffset, ref endOffset, x => x._runtimeFields._memoryHandles);	//RMHN
			WriteRuntimeFixup(0x544F4F52, ref startOffset, ref endOffset, x => x._runtimeFields._objectLists);		//ROOT

			_stream.Seek(endOffset);

			if(_nameListOffset > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x4D414E4F);
				_stream.WriteInt32(1);
				_stream.WriteInt32(0x14);
				_stream.WriteInt32(0x10);
				_stream.WriteUInt32((uint)_nameListOffset);
	
				endOffset = startOffset + 0x14u;
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			_fixupSize = (uint)endOffset - 0x800u;
		}
		private int AddString(string value)
		{
			int index = _stringList.FindIndex(x => x == value);
			if(index < 0)
			{
				index = _stringList.Count;
				_stringList.Add(value);
			}
			return index;
		}
		private void WriteRuntimeFixup(uint magic, ref ulong startOffset, ref ulong endOffset, Func<SaverSection, List<ulong>> getRuntimeFunc)
		{
			startOffset = endOffset;
			byte[] compressedData = GetRuntimeFixupData(getRuntimeFunc, out uint size, out uint count);
			if(count == 0) return;
			_stream.WriteUInt32(magic);
			_stream.WriteInt32(0);
			_stream.WriteInt32(0);
			_stream.WriteInt32(0x10);
			_stream.BaseStream.Write(compressedData);
			_stream.Seek(startOffset + 4);
			_stream.WriteUInt32(count);
			endOffset = Align(startOffset + 0x10 + size, 4);
			_stream.WriteUInt32((uint)(endOffset - startOffset));
			_stream.Seek(endOffset);
			_fixupCount++;
		}
		private byte[] GetRuntimeFixupData(Func<SaverSection, List<ulong>> getRuntimeFunc, out uint size, out uint count)
		{
			List<ulong> offsets = new List<ulong>();
			for(int i = 0; i < _sections.Count; i++)
			{
				List<ulong> sectionOffsets = getRuntimeFunc.Invoke(_sections[i]);
				offsets.Capacity += sectionOffsets.Count;
				sectionOffsets.Sort();
				for(int j = 0; j < sectionOffsets.Count; j++)
				{
					offsets.Add(sectionOffsets[j] + (ulong)(i << 0x1B));
				}
			}
			count = (uint)offsets.Count;
			byte[] compressedData = PackUncompressedIntegers(count, offsets);
			size = (uint)compressedData.Length;
			return compressedData;
		}
		public void WriteRawOffset(ulong data, SaverSection section)
		{
			if(igAlchemyCore.isPlatform64Bit(_platform)) section._sh.WriteUInt64(data);
			                                             section._sh.WriteUInt32((uint)data);
		}
		private ulong Align(ulong input, uint alignment)
		{
			return (ulong)(((input + (alignment - 1)) / alignment) * alignment);
		}
		private byte[] PackUncompressedIntegers(uint count, List<ulong> offsets)
		{
			List<byte> compressedData = new List<byte>();

			ulong previousInt = 0x00;
			bool shiftMoveOrMask = false;
			byte currentByte = 0x00;
			int shiftAmount = 0x00;

			for(int i = 0; i < count; i++)
			{
				bool firstPass = true;
				ulong deltaInt = (offsets[i] - previousInt) / 4 - (_version < 0x09 ? 1u : 0u);
				previousInt = offsets[i];
				while(true)
				{
					byte delta = (byte)((deltaInt >> shiftAmount) & 0b0111);
					ulong remaining = ((deltaInt >> shiftAmount) & ~0b0111u);
					if(remaining > 0 || delta > 0 || firstPass)
					{
						if(remaining != 0)
						{
							delta |= 0x08;
						}
						shiftAmount += 3;
						if(shiftMoveOrMask)
						{
							currentByte |= (byte)(delta << 4);
							compressedData.Add(currentByte);
							currentByte = 0x00;
						}
						else
						{
							currentByte |= delta;
						}
						firstPass = false;
						shiftMoveOrMask = !shiftMoveOrMask;
					}
					else
					{
						shiftAmount = 0;
						previousInt = offsets[i];
						break;
					}
				}
			}
			if(shiftMoveOrMask)
			{
				compressedData.Add(currentByte);
			}

			return compressedData.ToArray();
		}
	}
}