using System.Reflection;

namespace igLibrary.Core
{
	public class igArkCoreFile
	{
		public const uint _magicCookie = 0x41726B00;
		public const uint _magicVersion = 0x01;
		public const string ArkCoreFolder = "ArkCore";


		private StreamHelper _sh;
		private List<string?> _stringTable;
		public List<igMetaObject> _metaObjectsInFile;
		public List<igCompoundMetaFieldInfo> _compoundsInFile;
		public List<igMetaEnum> _metaEnumsInFile;
		public List<igMetaFieldPlatformInfo> _metaFieldPlatformInfosInFile;
		private Dictionary<Section, StreamHelper> _shs;
		private Dictionary<Section, uint> _offsets;

		private enum Section : int
		{
			Strings,
			CompoundInst,
			ObjectInst,
			CompoundInfo,
			ObjectInfo,
			EnumInfo,
			PlatformInfo,
		}

		public igArkCoreFile()
		{
			_stringTable = new List<string?>();
			_metaObjectsInFile = new List<igMetaObject>();
			_compoundsInFile = new List<igCompoundMetaFieldInfo>();
			_metaEnumsInFile = new List<igMetaEnum>();
			_metaFieldPlatformInfosInFile = new List<igMetaFieldPlatformInfo>();
			_shs = new Dictionary<Section, StreamHelper>(6);
			_offsets = new Dictionary<Section, uint>(6);
			_shs.Add(Section.Strings, null);
			_shs.Add(Section.CompoundInst, null);
			_shs.Add(Section.ObjectInst, null);
			_shs.Add(Section.CompoundInfo, null);
			_shs.Add(Section.ObjectInfo, null);
			_shs.Add(Section.EnumInfo, null);
			_shs.Add(Section.PlatformInfo, null);
		}

		uint GetOffset(Section section)
		{
			return _offsets[section] + _shs[section].Tell();
		}
		public void BeginSave(string filePath)
		{
			_sh = new StreamHelper(new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite));
			_shs[Section.Strings] = new StreamHelper(new MemoryStream());
			_shs[Section.CompoundInst] = new StreamHelper(new MemoryStream());
			_shs[Section.ObjectInst] = new StreamHelper(new MemoryStream());
			_shs[Section.CompoundInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.ObjectInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.EnumInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.PlatformInfo] = new StreamHelper(new MemoryStream());
		}

		public void ReadFile(string filePath)
		{
			_sh = new StreamHelper(new FileStream(filePath, FileMode.Open, FileAccess.Read));

			_sh.Seek(0);
			uint cookie = _sh.ReadUInt32();
			uint version = _sh.ReadUInt32();
			_sh.Seek(0x10);
			int stringCount = _sh.ReadInt32();
			int compoundCount = _sh.ReadInt32();
			int metaObjectCount = _sh.ReadInt32();
			int metaEnumCount = _sh.ReadInt32();
			int metaFieldPlatformCount = _sh.ReadInt32();

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x40 + i * 8);

				uint offset = _sh.ReadUInt32();
				uint length = _sh.ReadUInt32();

				_sh.Seek(offset);
				_shs[sections[i]] = new StreamHelper(new MemoryStream(_sh.ReadBytes(length)));
				_shs[sections[i]].Seek(0);
				_offsets.Add(sections[i], offset);
			}

			ReadStringTable(stringCount);
			InstantiateCompounds(compoundCount);
			InstantiateObjectMetas(metaObjectCount);
			_metaEnumsInFile.Capacity = metaEnumCount;
			for(int i = 0; i < metaEnumCount; i++)
			{
				igMetaEnum metaEnum = ReadMetaEnum();
				if(metaEnum == null) continue;
				igArkCore.AddEnumMeta(metaEnum);
				_metaEnumsInFile.Add(metaEnum);
			}
			ReadPlatformInfos(metaFieldPlatformCount);
			ReadCompounds();
			ReadMetaObject();
		}
		private void ReadStringTable(int stringCount)
		{
			_shs[Section.Strings].Seek(0);
			_stringTable.Capacity = stringCount;
			for(int i = 0; i < stringCount; i++)
			{
				_stringTable.Add(_shs[Section.Strings].ReadString());
			}
		}

		private void InstantiateCompounds(int compoundCount)
		{
			_shs[Section.CompoundInst].Seek(0);
			for(int i = 0; i < compoundCount; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
				compoundInfo._name = ReadString(_shs[Section.CompoundInst]);
				_compoundsInFile.Add(compoundInfo);
				igArkCore.AddCompoundMeta(compoundInfo);
			}
		}

		private void InstantiateObjectMetas(int metaObjectCount)
		{
			_shs[Section.ObjectInst].Seek(0);
			for(int i = 0; i < metaObjectCount; i++)
			{
				string type = ReadString(_shs[Section.ObjectInst]);
				igMetaObject metaObject = (igMetaObject)Activator.CreateInstance(igArkCore.GetObjectDotNetType(type));
				metaObject._name = ReadString(_shs[Section.ObjectInst]);
				_metaObjectsInFile.Add(metaObject);
				metaObject.AppendToArkCore();
			}
		}
		public void SaveMetaObject(igMetaObject metaObject)
		{
			SaveString(_shs[Section.ObjectInst], metaObject.GetType().Name);
			SaveString(_shs[Section.ObjectInst], metaObject._name);
			SaveString(_shs[Section.ObjectInfo], metaObject._parent != null ? metaObject._parent._name : null);
			
			int fieldCount = metaObject._metaFields.Count;
			List<(int, igMetaField)> fieldsToReplace = new List<(int, igMetaField)>();	//I hate tuples too
			int firstField = 0;
			if(metaObject._parent != null)
			{
				for(int i = 0; i < fieldCount; i++)
				{
					if(i < metaObject._parent._metaFields.Count && metaObject._parent._metaFields[i] != metaObject._metaFields[i]) fieldsToReplace.Add((i, metaObject._metaFields[i]));
				}
				firstField = metaObject._parent._metaFields.Count;
			}

			_shs[Section.ObjectInfo].WriteInt32(fieldCount - firstField);
			_shs[Section.ObjectInfo].WriteInt32(fieldsToReplace.Count);

			for(int i = firstField; i < fieldCount; i++)
			{
				SaveMetaField(_shs[Section.ObjectInfo], metaObject._metaFields[i]);
			}
			for(int i = 0; i < fieldsToReplace.Count; i++)
			{
				_shs[Section.ObjectInfo].WriteInt32(fieldsToReplace[i].Item1);
				SaveMetaField(_shs[Section.ObjectInfo], fieldsToReplace[i].Item2);
			}

			SaveAttributes(_shs[Section.ObjectInfo], metaObject._attributes);

			_metaObjectsInFile.Add(metaObject);
		}
		private void ReadPlatformInfos(int platformInfoCount)
		{
			_shs[Section.PlatformInfo].Seek(0);
			for(int i = 0; i < platformInfoCount; i++)
			{
				igMetaFieldPlatformInfo metaFieldPlatformInfo = new igMetaFieldPlatformInfo();
				metaFieldPlatformInfo._name = ReadString(_shs[Section.PlatformInfo]);
				uint platformCount = _shs[Section.PlatformInfo].ReadUInt32();
				for(int j = 0; j < platformCount; j++)
				{
					string platformName = ReadString(_shs[Section.PlatformInfo]);
					ushort size = _shs[Section.PlatformInfo].ReadUInt16();
					ushort align = _shs[Section.PlatformInfo].ReadUInt16();
					IG_CORE_PLATFORM platformValue = Enum.Parse<IG_CORE_PLATFORM>(platformName);
					metaFieldPlatformInfo._sizes.Add(platformValue, size);
					metaFieldPlatformInfo._alignments.Add(platformValue, align);
				}
				_metaFieldPlatformInfosInFile.Add(metaFieldPlatformInfo);
				igArkCore.AddPlatformMeta(metaFieldPlatformInfo);
			}
		}
		public void SaveMetaFieldPlatformInfo(igMetaFieldPlatformInfo metaFieldPlatformInfo)
		{
			SaveString(_shs[Section.PlatformInfo], metaFieldPlatformInfo._name);
			_shs[Section.PlatformInfo].WriteInt32(metaFieldPlatformInfo._sizes.Count);
			for(int j = 0; j < metaFieldPlatformInfo._sizes.Count; j++)
			{
				string platformName = metaFieldPlatformInfo._sizes.ElementAt(j).Key.ToString();
				SaveString(_shs[Section.PlatformInfo], platformName);

				ushort size = metaFieldPlatformInfo._sizes.ElementAt(j).Value;
				_shs[Section.PlatformInfo].WriteUInt16(size);

				ushort align = metaFieldPlatformInfo._alignments.ElementAt(j).Value;
				_shs[Section.PlatformInfo].WriteUInt16(align);
			}
			_metaFieldPlatformInfosInFile.Add(metaFieldPlatformInfo);
		}
		private void ReadMetaObject()
		{
			_shs[Section.ObjectInfo].Seek(0);
			for(int i = 0; i < _metaObjectsInFile.Count; i++)
			{
				igMetaObject metaObject = _metaObjectsInFile[i];
				string? parentName = ReadString(_shs[Section.ObjectInfo]);
				if(parentName != null)
				{
					metaObject._parent = igArkCore.GetObjectMeta(parentName);
					metaObject.InheritFields();
					if(metaObject._parent._name == "igDataList" || metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList")
					{
						metaObject._metaFields[0] = metaObject._metaFields[0].CreateFieldCopy();
						metaObject._metaFields[1] = metaObject._metaFields[1].CreateFieldCopy();
					}
					else if(metaObject._parent._name == "igHashTable")
					{
						metaObject._metaFields[2] = metaObject._metaFields[2].CreateFieldCopy();
						metaObject._metaFields[3] = metaObject._metaFields[3].CreateFieldCopy();
						metaObject._metaFields[4] = metaObject._metaFields[4].CreateFieldCopy();
					}
				}
				int fieldCount = _shs[Section.ObjectInfo].ReadInt32();
				int editedFieldCount = _shs[Section.ObjectInfo].ReadInt32();
				metaObject._metaFields.Capacity += fieldCount;
				for(int j = 0; j < fieldCount; j++)
				{
					metaObject._metaFields.Add(ReadMetaField(_shs[Section.ObjectInfo], metaObject));
				}
				for(int j = 0; j < editedFieldCount; j++)
				{
					int editedIndex = _shs[Section.ObjectInfo].ReadInt32();
					metaObject.ValidateAndSetField(editedIndex, ReadMetaField(_shs[Section.ObjectInfo], metaObject));
				}
				metaObject._attributes = ReadAttributes(_shs[Section.ObjectInfo]);

				metaObject.PostUndump();
			}
		}
		public void SaveMetaEnum(igMetaEnum metaEnum)
		{
			SaveString(_shs[Section.EnumInfo], metaEnum._name);
			_shs[Section.EnumInfo].WriteInt32(metaEnum._names.Count);
			for(int i = 0; i < metaEnum._names.Count; i++)
			{
				SaveString(_shs[Section.EnumInfo], metaEnum._names[i]);
				_shs[Section.EnumInfo].WriteInt32(metaEnum._values[i]);
			}
			_metaEnumsInFile.Add(metaEnum);
		}
		public igMetaEnum ReadMetaEnum()
		{
			igMetaEnum metaEnum = new igMetaEnum();
			metaEnum._name = ReadString(_shs[Section.EnumInfo]);
			int memberCount = _shs[Section.EnumInfo].ReadInt32();
			metaEnum._names.Capacity = memberCount;
			metaEnum._values.Capacity = memberCount;
			for(int i = 0; i < memberCount; i++)
			{
				metaEnum._names.Add(ReadString(_shs[Section.EnumInfo]));
				metaEnum._values.Add(_shs[Section.EnumInfo].ReadInt32());
			}
			if(metaEnum._name == null || metaEnum._name.Length == 0)
				return null;
			metaEnum.PostUndump();
			return metaEnum;
		}
		public void SaveAttributes(StreamHelper sh, igObjectList? attrs)
		{
			if(attrs == null)
			{
				sh.WriteInt32(0);
				return;
			}
			sh.WriteInt32(attrs._count);
			for(int i = 0; i < attrs._count; i++)
			{
				igObject attr = attrs[i];
				if(attr == null)
				{
					sh.WriteInt32(-1);
					continue;
				}
				Type attrType = attr.GetType();
				SaveString(sh, attrType.Name);

				FieldInfo? valueField = attrType.GetField("_value");
				object? attrValue = valueField.GetValue(attr);

				     if(attrValue is bool boolValue)       sh.WriteInt32(boolValue ? 1 : 0);
				else if(attrValue is string stringValue)   SaveString(sh, stringValue);
				else if(attrValue is int intValue)         sh.WriteInt32(intValue);
				else if(attrValue is igMetaObject moValue) SaveString(sh, moValue._name);
				else if(valueField.FieldType.IsEnum)       sh.WriteInt32(Convert.ToInt32(attrValue));	//Never used by the game, only used by my own igPlatformExclusiveAttribute
				else throw new NotSupportedException("Unsupported attribute value type");
			}
		}
		public igObjectList? ReadAttributes(StreamHelper sh)
		{
			int attrCount = sh.ReadInt32();
			if(attrCount == 0) return null;
			igObjectList attrs = new igObjectList();
			attrs.SetCapacity(attrCount);
			for(int i = 0; i < attrCount; i++)
			{
				string? typeName = ReadString(sh);
				if(typeName == null) continue;
				Type attrType = igArkCore.GetObjectDotNetType(typeName);
				igObject attr = (igObject)Activator.CreateInstance(attrType);

				FieldInfo? valueField = attrType.GetField("_value");
				object? attrValue = null;

				     if(valueField.FieldType == typeof(bool))         attrValue = sh.ReadUInt32() != 0;
				else if(valueField.FieldType == typeof(string))       attrValue = ReadString(sh);
				else if(valueField.FieldType == typeof(int))          attrValue = sh.ReadInt32();
				else if(valueField.FieldType == typeof(igMetaObject)) attrValue = igArkCore.GetObjectMeta(ReadString(_shs[Section.ObjectInfo]));
				else if(valueField.FieldType.IsEnum)                  attrValue = Enum.ToObject(valueField.FieldType, sh.ReadInt32());	//Never used by the game, only used by my own igPlatformExclusiveAttribute
				else throw new NotSupportedException("Unsupported attribute value type");

				valueField.SetValue(attr, attrValue);
				attrs.Append(attr);
			}
			return attrs;
		}
		public void SaveMetaField(StreamHelper sh, igMetaField? metaField)
		{
			if(metaField == null) sh.WriteInt32(-1);
			else
			{
				metaField.DumpArkData(this, sh);
				if(metaField._default == null) sh.WriteInt32(-1);
				else metaField.DumpDefault(this, sh);
			}
		}
		public igMetaField? ReadMetaField(StreamHelper sh, igBaseMeta? meta = null)
		{
			string? typeName = ReadString(sh);
			if(typeName == null) return null;
			Type? t = igArkCore.GetObjectDotNetType(typeName);
			igCompoundMetaFieldInfo? compoundFieldInfo = null;
			if(t == null)
			{
				compoundFieldInfo = igArkCore.GetCompoundFieldInfo(typeName);
				t = typeof(igPlaceHolderMetaField);
				if(compoundFieldInfo != null)
				{
					t = typeof(igCompoundMetaField);
				}
				else if(typeName.EndsWith("ArrayMetaField"))
				{
					//This is a hack
					compoundFieldInfo = igArkCore.GetCompoundFieldInfo(typeName.Replace("ArrayMetaField", "MetaField"));
					if(compoundFieldInfo != null)
					{
						t = typeof(igCompoundArrayMetaField);
					}
				}
			}
			igMetaField metaField = (igMetaField)Activator.CreateInstance(t);
			metaField._parentMeta = meta;
			if(metaField is igPlaceHolderMetaField placeHolder)
			{
				placeHolder._platformInfo = igArkCore.GetMetaFieldPlatformInfo(typeName);
			}
			else if(metaField is igCompoundMetaField compoundField)
			{
				compoundField._compoundFieldInfo = compoundFieldInfo;
			}
			metaField.UndumpArkData(this, sh);

			int size = sh.ReadInt32();
			if(size > 0) metaField.UndumpDefault(this, sh);

			return metaField;
		}
		public void SaveCompoundInfo(igCompoundMetaFieldInfo compoundInfo)
		{
			SaveString(_shs[Section.CompoundInst], compoundInfo._name);
			_shs[Section.CompoundInfo].WriteInt32(compoundInfo._fieldList.Count);

			for(int i = 0; i < compoundInfo._fieldList.Count; i++)
			{
				SaveMetaField(_shs[Section.CompoundInfo], compoundInfo._fieldList[i]);
			}

			_compoundsInFile.Add(compoundInfo);
		}
		private void ReadCompounds()
		{
			_shs[Section.CompoundInfo].Seek(0);
			for(int i = 0; i < _compoundsInFile.Count; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = _compoundsInFile[i];
				int fieldCount = _shs[Section.CompoundInfo].ReadInt32();
				compoundInfo._fieldList.Capacity = fieldCount;

				for(int j = 0; j < fieldCount; j++)
				{
					compoundInfo._fieldList.Add(ReadMetaField(_shs[Section.CompoundInfo], compoundInfo));
				}
				compoundInfo.PostUndump();
			}
		}
		public void FinishSave()
		{
			_sh.Seek(0);
			_sh.WriteUInt32(_magicCookie);
			_sh.WriteUInt32(_magicVersion);
			_sh.Seek(0x10);
			_sh.WriteInt32(_stringTable.Count);
			_sh.WriteInt32(_compoundsInFile.Count);
			_sh.WriteInt32(_metaObjectsInFile.Count);
			_sh.WriteInt32(_metaEnumsInFile.Count);
			_sh.WriteInt32(_metaFieldPlatformInfosInFile.Count);

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x40 + i * 8);

				_shs[sections[i]].BaseStream.Flush();

				uint offset = (uint)_sh.BaseStream.Length;
				uint length = (uint)_shs[sections[i]].BaseStream.Length;
				if(offset < 0x80) offset = 0x80;

				_sh.WriteUInt32(offset);
				_sh.WriteUInt32(length);

				_sh.Seek(offset);
				_shs[sections[i]].Seek(0);
				_shs[sections[i]].BaseStream.CopyTo(_sh.BaseStream);

				_sh.BaseStream.Flush();
			}
		}
		public void SaveString(StreamHelper sh, string? data)
		{
			int index = -1;

			if(data != null)
			{
				index = _stringTable.FindIndex(x => x == data);
				if(index < 0)
				{
					index = _stringTable.Count;
					_stringTable.Add(data);
					_shs[Section.Strings].WriteString(data);
				}
			}

			sh.WriteInt32(index);
		}
		public string? ReadString(StreamHelper sh)
		{
			int index = sh.ReadInt32();
			if(index < 0) return null;
			return _stringTable[index];
		}
		public void Dispose()
		{
			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_shs[sections[i]].Close();
				_shs[sections[i]].Dispose();
			}
			_sh.Close();
			_sh.Dispose();
		}
	}
}