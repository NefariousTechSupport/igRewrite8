/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
        public List<(igHandle, bool)> _namedList = new List<(igHandle, bool)>();    //If true then namedHandle, otherwise named external
        public List<igHandle> _externalList = new List<igHandle>();
        public List<(string, string)> _buildDependancies = new List<(string, string)>();
        public IG_CORE_PLATFORM _platform;
        public igObjectDirectory _dir;
        private StreamHelper _stream;
        public uint _version;
        private uint _fixupCount;
        private uint fixupCount2;
        private uint _fixupSize;
        private ulong _nameListOffset = 0;
        private ulong _rootListOffset = 0;

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
                if (pool == null) throw new ArgumentNullException("Memory pool cannot be null!");
                _sh = new StreamHelper(new MemoryStream(), igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big : StreamHelper.Endianness.Little);
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
                for (int i = 0; i < size; i++)
                {
                    _sh.WriteByte(0);
                }
                return offset;
            }
            public void PushAlignment(uint alignment)
            {
                if (_alignment < alignment) _alignment = alignment;
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
            switch (igArkCore.Game)
            {
                case igArkCore.EGame.EV_SkylandersSpyrosAdventure:
                    _version = 0x05;
                    break;
                case igArkCore.EGame.EV_SkylandersTrapTeam:
                    _version = 0x08;
                    break;
                case igArkCore.EGame.EV_SkylandersSuperchargers:
                case igArkCore.EGame.EV_SkylandersImaginators:
                    _version = 0x09;
                    break;
                default:
                    throw new NotImplementedException($"Game {igArkCore.Game} is not implemented");
            }
            _stream = new StreamHelper(dst, igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big : StreamHelper.Endianness.Little);
            _dir = dir;

            // tfbTool level.bld nd language packs do this funky thing where
            // they seemingly preallocate the sections this will be incorrect
            // for the tfbTool texture igzs but don't worry about it as it won't
            // cause problems
            if (igRegistry.GetRegistry()._engineType == EngineType.TfbTool)
            {
                Func<string, igMemoryPool?> dumb = igMemoryContext.Singleton.GetMemoryPoolByName;
                if (_version < 0x7)
                {   // true for SSA, not sure about other versions

                    // very bad basically hardcoded way to check if collision chunk exists
                    // workaround for the problems caused by GetSaverSection
                    bool hascollision = (dir._objectList[0].ToString().Split('.').Last() == "tfbPhysicsWorld");
                    GetSaverSection(dumb("Default")!);
                    GetSaverSection(dumb("String")!);
                    if (hascollision)
                    {
                        GetSaverSection(dumb("Collision")!);
                    }
                    GetSaverSection(dumb("Image")!);
                    GetSaverSection(dumb("VertexObject")!);
                    GetSaverSection(dumb("Vertex")!);
                    GetSaverSection(dumb("AnimationData")!);
                    GetSaverSection(dumb("Text")!);
                    GetSaverSection(dumb("Audio")!);
                }
                else
                {
                    GetSaverSection(dumb("Default")!);
                    GetSaverSection(dumb("Image")!);
                    GetSaverSection(dumb("Vertex")!);
                    GetSaverSection(dumb("Audio")!);
                    GetSaverSection(dumb("AnimationData")!);
                    GetSaverSection(dumb("VertexObject")!);
                    GetSaverSection(dumb("String")!);
                }
            }

            SaverSection rootSection = GetSaverSection(dir._objectList.internalMemoryPool);
            _rootListOffset = SaveObject(dir._objectList);
            if (dir._useNameList)
            {
                _nameListOffset = SaveObject(dir._nameList);
            }

            if (_vTableList.Any(x => x._name == "igLocalizedInfo"))
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
            if (needsDeep)
            {
                SaveObjectDeep(offset, obj);
            }
            return offset;
        }
        public ulong SaveObjectShallow(igObject? obj, out bool needsDeep)
        {
            if (obj == null)
            {
                needsDeep = false;
                return 0;
            }
            SaverSection section = GetSaverSection(obj.internalMemoryPool);
            bool previouslyWritten = section._objectOffsetList.TryGetValue(obj, out ulong offset);
            if (previouslyWritten)
            {
                needsDeep = false;
                return offset | (section._index << (_version >= 7 ? 0x1B : 0x18));
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
            if (index < 0)
            {
                index = _namedList.Count;
                _namedList.Add(named);
            }
            int properIndex = index;
            for (int i = 0; i < index; i++)
            {
                if (_namedList[i].Item2 != named.Item2) properIndex--;
            }
            return properIndex;
        }
        public ulong SaveObjectDeep(ulong serialized, igObject obj)
        {
            GetOffsetBad(serialized, out igIGZSaver.SaverSection section, out ulong deserialized);
            bool found = section._objectOffsetList.TryGetValue(obj, out ulong offset);
            if (!found)
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
            _stream.WriteUInt32(0);         //SerializableFieldsHash
            if (_version < 7)
            {
                _stream.WriteUInt32(0);
            }
            else
            {
                _stream.WriteInt32(igArkCore.GetMetaEnum("IG_CORE_PLATFORM").GetValueFromEnum(_platform));
                _stream.WriteUInt32(_fixupCount);
                fixupCount2 = _fixupCount;
            }
        }
        private void WriteOutSections()
        {
            int memPoolNameOffset = 0;
            uint memoryOffset = 0x800 + _fixupSize;
            //Write out fixup section
            if (_version < 0x7)
            {
                _stream.Seek(0xC);
            }
            else
            {
                _stream.Seek(0x14);
            }
            _stream.WriteUInt32(0);
            _stream.WriteUInt32(0x800);
            _stream.WriteUInt32(_fixupSize);
            _stream.WriteUInt32(0x800);

            for (int i = 0; i < _sections.Count; i++)
            {
                if (_version < 0x7)
                {
                    _stream.Seek(0x1C + i * 0x10);
                }
                else
                {
                    _stream.Seek(0x24 + i * 0x10);
                }
                uint unknown = 0;
                _stream.WriteUInt32((uint)(memPoolNameOffset & 0xFFFF) | ((unknown << 16) & 0xFFFF));
                _stream.WriteUInt32(memoryOffset);

                uint sectionSize = (uint)_sections[i]._sh.BaseStream.Length;

                // Dumb thing where they write 0xFFFFFFFF insted of the real size
                // if it's 4 since it cannot be addressed by R fixups
                bool realSection = _version >= 9 || sectionSize > 4;
                uint alignment = _sections[i]._alignment;


                _stream.WriteUInt32(realSection ? sectionSize : 0xFFFFFFFF);

                if (realSection && alignment == 0)
                {
                    Logging.Warn("The alignment of section {0} in file {1} has an alignment of 0, this is bad, forcing the alignment to 0x10 to prevent game crashes", i, _dir._path);
                    alignment = 0x10;
                }
                _stream.WriteUInt32(alignment);

                _stream.Seek(memoryOffset);
                _sections[i]._sh.BaseStream.Flush();
                _sections[i]._sh.Seek(0);

                _sections[i]._sh.BaseStream.CopyTo(_stream.BaseStream);

                memPoolNameOffset += _sections[i]._pool._name.Length + 1;   //Don't forget the null byte!
                memoryOffset += sectionSize;

                _sections[i]._sh.BaseStream.Close();
            }

            if (_version < 0x7)
            {
                _stream.Seek(0x1C + 0x10 * 0x55); // First chunk after the fixup chunk starts at 0x1C. Every chunk header is 0x10 bytes long, you can have a total of 0x55 chunks (in ssa).
            }
            else
            {
                _stream.Seek(0x24 + 0x10 * 0x20); // Same thing but first chunk starts at 0x24 and you can have a total of 0x20 chunks.
            }
            for (int i = 0; i < _sections.Count; i++)
            {
                _stream.WriteString(_sections[i]._pool._name);
            }
        }
        private void WriteVTable(igMetaObject meta, SaverSection section)
        {
            section.PushAlignment(igAlchemyCore.GetPointerSize(_platform));
            int index = _vTableList.FindIndex(x => x == meta);
            if (index < 0)
            {
                index = _vTableList.Count;
                _vTableList.Add(meta);
                if (meta is DotNet.igDotNetDynamicMetaObject dndmo && dndmo._owner._path != "scripts:/common.vvl")
                {
                    AddBuildDependency(dndmo._owner._path);
                }
            }
            section._runtimeFields._vtables.Add(section._sh.Tell());
            WriteRawOffset((ulong)index, section);
        }

        public void GetOffsetBad(ulong serialized, out igIGZSaver.SaverSection section, out ulong deserialized)
        {
            if (_version <= 0x06)
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
            if (pool == null)
            {
                Logging.Warn("Tried finding a saver section with a null pool, assigning default to prevent crash");
                pool = igMemoryContext.Singleton.GetMemoryPoolByName("Default")!;
            }

            int index = _sections.FindIndex(x => x._pool == pool);
            SaverSection ret;
            if (index < 0)
            {
                ret = new SaverSection(pool, _platform);
                ret._index = (uint)_sections.Count;
                if (_version < 9)
                {
                    // first 4 bytes cannot be addressed by the R fixups
                    // so pad that out
                    ret._sh.WriteUInt32(0);
                }
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
            if (_version <= 0x06) return offset | (index << 0x18);
            else return offset | (index << 0x1B);
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
            if (!depList.Any(x => x.Item2 == path))
            {
                depList.Add((Path.GetFileNameWithoutExtension(path), path));
            }
        }
        public void WriteFixupSections(igObjectDirectory dir)
        {
            ulong endOffset = (_version < 0x7) ? 0x81Cul : 0x800ul;
            ulong startOffset = 0x800;
            _stream.Seek(startOffset);

            ulong dependancyStartOffset = 0x800;
            int defaultFixupSize = (_version < 0x7) ? 0x18 : 0x10;
            ulong fixupSizeOffset = (_version < 0x7) ? 0x10ul : 0x08ul;
            //Dependancy list can't be autogenerated, cry

            // write the fixup igz header (for versions < 7)
            if (_version < 7)
            {
                _stream.WriteUInt32(igIGZLoader._littleMagicCookie);
                _stream.WriteUInt32(_version);
                _stream.WriteUInt16((ushort)igArkCore.GetMetaEnum("IG_CORE_PLATFORM").GetValueFromEnum(_platform));
                _stream.WriteUInt16(0);
                _stream.WriteUInt32(0x1C);
                _stream.WriteUInt32(0x0E); // fixup count for all .blds, I wish I didnt have to hardcode this
                _stream.WriteUInt32(0x1C);
                _stream.WriteUInt32(0);

            }
            if (dir._dependencies.Count > 0)
            {
                List<(string, string)> depList = new List<(string, string)>();
                for (int j = 0; j < dir._dependencies.Count; j++)
                {
                    depList.Add((dir._dependencies[j]._name._string, dir._dependencies[j]._path));
                }
                depList = depList.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                //List<(string, string)> buildDepList = _buildDependancies.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                List<(string, string)> buildDepList = new List<(string, string)>();//_buildDependancies.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                List<(string, string)> sortedDepList = new List<(string, string)>();

                int depIndex = 0;
                int bDepIndex = 0;
                while (depIndex < depList.Count || bDepIndex < buildDepList.Count)
                {
                    if (depIndex >= depList.Count) goto addBuildDep;
                    if (bDepIndex >= buildDepList.Count) goto addDep;

                    int cmp = string.Compare(depList[depIndex].Item1, buildDepList[bDepIndex].Item1, false);
                    if (cmp < 0) goto addDep;
                    if (cmp > 0) goto addBuildDep;

                    cmp = string.Compare(depList[depIndex].Item2, buildDepList[bDepIndex].Item2, false);
                    if (cmp < 0) goto addDep;
                    if (cmp > 0) goto addBuildDep;

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
                _stream.WriteInt32(defaultFixupSize);

                for (int j = 0; j < sortedDepList.Count; j++)
                {
                    _stream.WriteString(sortedDepList[j].Item1);
                    _stream.WriteString(sortedDepList[j].Item2);
                }

                endOffset = Align(_stream.Tell(), 4);
                if (endOffset == _stream.Tell())
                {
                    endOffset += 4;
                }
                _stream.Seek(startOffset + fixupSizeOffset);
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

            // setup _stringList from EXNM (because for whatever reason the code is written like this)
            if (_namedList.Count > 0)
            {
                for (int j = 0; j < _namedList.Count; j++)
                {
                    AddString(_namedList[j].Item1._namespace._string);
                    AddString(_namedList[j].Item1._alias._string);
                }
            }
            //TSTR
            if (_stringList.Count > 0)
            {
                startOffset = endOffset;
                _stream.Seek(startOffset);
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0x01);
                    _stream.WriteInt64(0);
                }
                else
                {
                    _stream.WriteUInt32(0x52545354);
                }
                _stream.WriteInt32(_stringList.Count);
                _stream.WriteInt32(0);
                _stream.WriteInt32(defaultFixupSize);
                for (int j = 0; j < _stringList.Count; j++)
                {
                    long basePos = _stream.BaseStream.Position;
                    _stream.WriteString(_stringList[j]);

                    int bits = (_version > 7) ? 2 : 1;
                    _stream.Seek(basePos + bits + (_stringList[j].Length & (uint)(-bits)));
                }
                ulong tempEndOffset = Align(_stream.Tell(), 4);
                _stream.Seek(startOffset + fixupSizeOffset);
                _stream.WriteUInt32((uint)(tempEndOffset - startOffset));
                _stream.Seek(tempEndOffset);
                endOffset = (ulong)_stream.BaseStream.Position;
                //_stream.Seek(endOffset + tempEndOffset - 0x800);
                _fixupCount++;
            }
            //TMET
            if (_vTableList.Count > 0)
            {
                startOffset = endOffset;
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0x00);
                    _stream.WriteUInt32(0x00);
                    _stream.WriteUInt32(0x00);
                }
                else
                {
                    _stream.WriteUInt32(0x54454D54);
                }
                _stream.WriteInt32(_vTableList.Count);
                _stream.WriteInt32(0);
                _stream.WriteInt32(defaultFixupSize);
                for (int j = 0; j < _vTableList.Count; j++)
                {
                    long basePos = _stream.BaseStream.Position;
                    _stream.WriteString(_vTableList[j]._name);

                    int bits = (_version > 7) ? 2 : 1;
                    _stream.Seek(basePos + bits + (_vTableList[j]._name.Length & (uint)(-bits)));
                }
                endOffset = Align(_stream.Tell(), 4);
                _stream.Seek(startOffset + fixupSizeOffset);
                _stream.WriteUInt32((uint)(endOffset - startOffset));
                _stream.Seek(endOffset);

                startOffset = endOffset;
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0xC);
                    _stream.WriteInt64(0);
                }
                else
                {
                    _stream.WriteUInt32(0x5A53544D); // MTSZ
                }
                _stream.WriteInt32(_vTableList.Count);
                _stream.WriteInt32(_vTableList.Count * 4 + defaultFixupSize);
                _stream.WriteInt32(defaultFixupSize);
                for (int j = 0; j < _vTableList.Count; j++)
                {
                    _stream.WriteUInt32(_vTableList[j]._sizes[_platform]);
                }
                endOffset = startOffset + (uint)defaultFixupSize + (uint)_vTableList.Count * 4u;
                _stream.Seek(endOffset);
                _fixupCount += 2;
            }

            if (_externalList.Count > 0)
            {
                startOffset = endOffset;
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0x02);
                    _stream.WriteInt64(0);
                }
                else
                {
                    _stream.WriteUInt32(0x44495845); // EXID
                }
                _stream.WriteInt32(_externalList.Count);
                if ((_stream.BaseStream.Position + 0x8) % 8 == 0)
                {
                    _stream.WriteUInt32((uint)_externalList.Count * 8 + ((_version < 0x7) ? 0x18u : 0x10u));
                    _stream.WriteInt32(defaultFixupSize);
                }
                else
                {
                    _stream.WriteUInt32((uint)_externalList.Count * 8 + ((_version < 0x7) ? 0x1Cu : 0x14u));
                    _stream.WriteInt32(defaultFixupSize + 0x4);
                    _stream.WriteInt32(0);
                }
                for (int j = 0; j < _externalList.Count; j++)
                {
                    _stream.WriteUInt32(_externalList[j]._alias._hash);
                    _stream.WriteUInt32(_externalList[j]._namespace._hash);
                }
                endOffset = startOffset + (uint)defaultFixupSize + (uint)_externalList.Count * 8u;
                _stream.Seek(endOffset);
                _fixupCount += 1;
            }

            if (_namedList.Count > 0)
            {
                startOffset = endOffset;
                int externalCount = _namedList.Count;
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0x03);
                    _stream.WriteInt64(0x00);
                }
                else
                {
                    _stream.WriteUInt32(0x4D4E5845); // EXNM
                }
                _stream.WriteInt32(externalCount);
                uint alignedDataStart = (uint)Align(_stream.Tell() + 8 - (uint)startOffset, igAlchemyCore.GetPointerSize(_platform));
                _stream.WriteUInt32((uint)externalCount * igAlchemyCore.GetPointerSize(_platform) * 2u + alignedDataStart);
                _stream.WriteUInt32(alignedDataStart);
                _stream.Seek(startOffset + alignedDataStart);

                for (int j = 0; j < _namedList.Count; j++)
                {
                    _stream.WriteUInt32((uint)AddString(_namedList[j].Item1._namespace._string) | (_namedList[j].Item2 ? 0x80000000u : 0u));
                    _stream.WriteUInt32((uint)AddString(_namedList[j].Item1._alias._string));
                }

                endOffset = _stream.Tell();
                _stream.Seek(endOffset);
                _fixupCount += 1;
            }

            if (_thumbnails.Count > 0)
            {
                startOffset = endOffset;
                if (_version < 0x7)
                {
                    _stream.WriteUInt32(0x0A);
                    _stream.WriteInt64(0);
                }
                else
                {
                    _stream.WriteUInt32(0x4E484D54); // TMHN
                }
                _stream.WriteInt32(_thumbnails.Count);
                if ((_stream.BaseStream.Position + 0x8) % 8 == 0)
                {
                    _stream.WriteUInt32((uint)_thumbnails.Count * igAlchemyCore.GetPointerSize(_platform) * 2u + ((_version < 0x7) ? 0x18u : 0x10u));
                    _stream.WriteInt32(defaultFixupSize);
                }
                else
                {
                    _stream.WriteUInt32((uint)_thumbnails.Count * igAlchemyCore.GetPointerSize(_platform) * 2u + ((_version < 0x7) ? 0x1Cu : 0x14u));
                    _stream.WriteInt32(defaultFixupSize + 0x4);
                    _stream.WriteInt32(0);
                }
                for (int j = 0; j < _thumbnails.Count; j++)
                {
                    if (igAlchemyCore.isPlatform64Bit(_platform))
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

            if (_version < 0x7)
            {
                WriteRuntimeFixup(0x05, ref startOffset, ref endOffset, x => x._runtimeFields._vtables);          //RVTB
                WriteRuntimeFixup(0x0E, ref startOffset, ref endOffset, x => x._runtimeFields._stringRefs);       //RSTR
                                                                                                                  // RSTT does not exist
                WriteRuntimeFixup(0x06, ref startOffset, ref endOffset, x => x._runtimeFields._offsets);          //ROFS
                WriteRuntimeFixup(0x0F, ref startOffset, ref endOffset, x => x._runtimeFields._poolIds);          //RPID
                WriteRuntimeFixup(0x07, ref startOffset, ref endOffset, x => x._runtimeFields._externals);        //REXT
                                                                                                                  //RHND does not exist
                WriteRuntimeFixup(0x10, ref startOffset, ref endOffset, x => x._runtimeFields._namedExternals);   //RNEX
                WriteRuntimeFixup(0x0B, ref startOffset, ref endOffset, x => x._runtimeFields._memoryHandles);    //RMHN
            }
            else
            {
                WriteRuntimeFixup(0x42545652, ref startOffset, ref endOffset, x => x._runtimeFields._vtables);          //RVTB
                WriteRuntimeFixup(0x52545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringRefs);       //RSTR
                WriteRuntimeFixup(0x54545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringTables);     //RSTT
                WriteRuntimeFixup(0x53464F52, ref startOffset, ref endOffset, x => x._runtimeFields._offsets);          //ROFS
                WriteRuntimeFixup(0x44495052, ref startOffset, ref endOffset, x => x._runtimeFields._poolIds);          //RPID
                WriteRuntimeFixup(0x54584552, ref startOffset, ref endOffset, x => x._runtimeFields._externals);        //REXT
                WriteRuntimeFixup(0x444E4852, ref startOffset, ref endOffset, x => x._runtimeFields._handles);          //RHND
                WriteRuntimeFixup(0x58454E52, ref startOffset, ref endOffset, x => x._runtimeFields._namedExternals);   //RNEX
                WriteRuntimeFixup(0x4E484D52, ref startOffset, ref endOffset, x => x._runtimeFields._memoryHandles);    //RMHN
            }

            // ROOT fixup isn't actually compressed, it took me 3 years to learn this
            startOffset = endOffset;
            uint rootAlignedStart = (_version < 0x7) ? 0x18u : 0x10u;
            if (((startOffset & 0x4) != 0) && igAlchemyCore.isPlatform64Bit(_platform))
            {
                rootAlignedStart += 4;
            }
            if (_version < 0x7)
            {
                _stream.WriteUInt32(0x8);
                _stream.WriteInt64(0);
            }
            else
            {
                _stream.WriteUInt32(0x544F4F52);
            }
            _stream.WriteInt32(1);
            _stream.WriteUInt32(rootAlignedStart + 4);
            _stream.WriteUInt32(rootAlignedStart);
            _stream.WriteUInt32((uint)_rootListOffset);
            endOffset = startOffset + rootAlignedStart + 4;
            _fixupCount += 1;

            _stream.Seek(endOffset);

            if (_nameListOffset > 0)
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
            if (index < 0)
            {
                index = _stringList.Count;
                _stringList.Add(value);
            }
            return index;
        }
        private void WriteRuntimeFixup(uint magic, ref ulong startOffset, ref ulong endOffset, Func<SaverSection, List<ulong>> getRuntimeFunc)
        {
            ulong defaultFixupSize = (_version < 0x7) ? 0x18ul : 0x10ul;
            int countOffset = (_version < 0x7) ? 0x0C : 0x04;
            startOffset = endOffset;
            byte[] compressedData = GetRuntimeFixupData(getRuntimeFunc, out uint size, out uint count);
            if (count == 0) return;
            _stream.WriteUInt32(magic);
            if (_version < 0x7)
            {
                _stream.WriteInt64(0);
            }
            _stream.WriteInt32(0);
            _stream.WriteInt32(0);
            _stream.WriteInt32((int)defaultFixupSize);
            _stream.BaseStream.Write(compressedData);
            _stream.Seek(startOffset + (ulong)countOffset);
            _stream.WriteUInt32(count);
            endOffset = Align(startOffset + defaultFixupSize + size, 4);
            _stream.WriteUInt32((uint)(endOffset - startOffset));
            _stream.Seek(endOffset);
            _fixupCount++;
        }
        private byte[] GetRuntimeFixupData(Func<SaverSection, List<ulong>> getRuntimeFunc, out uint size, out uint count)
        {
            List<ulong> offsets = new List<ulong>();
            for (int i = 0; i < _sections.Count; i++)
            {
                List<ulong> sectionOffsets = getRuntimeFunc.Invoke(_sections[i]);
                offsets.Capacity += sectionOffsets.Count;
                sectionOffsets.Sort();
                for (int j = 0; j < sectionOffsets.Count; j++)
                {
                    if (_version < 0x7)
                    {
                        offsets.Add(sectionOffsets[j] + (ulong)(i << 0x18));
                    }
                    else
                    {
                        offsets.Add(sectionOffsets[j] + (ulong)(i << 0x1B));
                    }
                }
            }
            count = (uint)offsets.Count;
            byte[] compressedData = PackUncompressedIntegers(count, offsets);
            size = (uint)compressedData.Length;
            return compressedData;
        }
        public void WriteRawOffset(ulong data, SaverSection section)
        {
            if (igAlchemyCore.isPlatform64Bit(_platform)) section._sh.WriteUInt64(data);
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

            for (int i = 0; i < count; i++)
            {
                bool firstPass = true;
                ulong deltaInt = (offsets[i] - previousInt) / 4 - (_version < 0x09 ? 1u : 0u);
                previousInt = offsets[i];
                while (true)
                {
                    byte delta = (byte)((deltaInt >> shiftAmount) & 0b0111);
                    ulong remaining = ((deltaInt >> shiftAmount) & ~0b0111u);
                    if (remaining > 0 || delta > 0 || firstPass)
                    {
                        if (remaining != 0)
                        {
                            delta |= 0x08;
                        }
                        shiftAmount += 3;
                        if (shiftMoveOrMask)
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
            if (shiftMoveOrMask)
            {
                compressedData.Add(currentByte);
            }

            return compressedData.ToArray();
        }
    }
}