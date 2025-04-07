/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection.Emit;
using System.Reflection;

namespace igLibrary.Core
{
	public class igCompoundMetaField : igMetaField
	{
		public igCompoundMetaFieldInfo _compoundFieldInfo;
		[Obsolete("Exists for the reflection system, use GetFieldList instead.")] public igMetaFieldList _fieldList;

		public override object? ReadIGZField(igIGZLoader loader)
		{
			if(_compoundFieldInfo._vTablePointer == null) return null;

			uint objectOffset = loader._stream.Tell();

			List<igMetaField> metaFields = GetFieldList();

			object compoundData = _compoundFieldInfo.ConstructInstance(GetOutputTypeInternal(), metaFields);	//grab the type like this becuase igOrderedMapMetaField<T, U>

			_compoundFieldInfo.CalculateOffsetForPlatform(loader._platform);

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(loader._platform)) continue;

				if(!metaFields[i]._properties._persistent) continue;

				loader._stream.Seek(objectOffset + metaFields[i]._offsets[loader._platform]);

				object? data = metaFields[i].ReadIGZField(loader);

				FieldInfo? field = metaFields[i]._fieldHandle;
				field.SetValue(compoundData, data);
			}

			return compoundData;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(_compoundFieldInfo._vTablePointer == null) return;

			uint objectOffset = section._sh.Tell();

			List<igMetaField> metaFields = GetFieldList();

			_compoundFieldInfo.CalculateOffsetForPlatform(saver._platform);

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(saver._platform)) continue;

				object? data = null;

				if(metaFields[i]._properties._persistent)
				{
					FieldInfo? field = metaFields[i]._fieldHandle;

					data = field.GetValue(value);
				}
				else
				{
					data = metaFields[i].GetDefault(igMemoryContext.Singleton.GetMemoryPoolByName("Default"));
					if((metaFields[i].GetOutputType().IsValueType || metaFields[i].GetOutputType() == typeof(string)) && data == null) continue;
				}

				section._sh.Seek(objectOffset + metaFields[i]._offsets[saver._platform]);

				metaFields[i].WriteIGZField(saver, section, data);
			}
		}
		public override Type GetOutputType()
		{
			if(_compoundFieldInfo._vTablePointer == null) return typeof(object);
			else return _compoundFieldInfo._vTablePointer;
		}
		//Exists to deal with both igOrderedMap<T, U> being annoying and also needing to handle array metafields
		private Type GetOutputTypeInternal()
		{
			Type t = GetOutputType();
			if(t.IsArray) t = t.GetElementType();
			return t;
		}
		public virtual List<igMetaField> GetFieldList() => _compoundFieldInfo._fieldList;

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._alignments[platform];
		public override uint GetSize(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._sizes[platform];
		public override object? GetDefault(igMemoryPool pool)
		{
			return _compoundFieldInfo.ConstructInstance(GetOutputTypeInternal(), GetFieldList());
		}


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// I cannot be bothered to implement this
			Logging.Warn("Tried parsing igCompoundMetaField value string when unimplemented, returning success...");
			return true;
		}
	}

	public class igCompoundMetaFieldInfo : igBaseMeta
	{
		public List<igMetaField> _fieldList = new List<igMetaField>();
		public igMetaFieldPlatformInfo _platformInfo;
		public Type _vTablePointer;
		private bool _beganTypeDeclaration = false;
		private bool _beganTypeDefiniton = false;
		private bool _beganTypeFinalization = false;
		private bool _finishedTypeFinalization = false;

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetStructDotNetType(_name.Substring(0, _name.Length-9));
			_platformInfo = igArkCore.GetMetaFieldPlatformInfo(_name);

			//Sometimes is null, in those cases, a dynamic type will be created
			if(_vTablePointer != null)
			{
				for(int i = 0; i < _fieldList.Count; i++)
				{
					_fieldList[i]._fieldHandle = _vTablePointer.GetField(_fieldList[i]._fieldName ?? $"_unk{i}");
				}
			}
		}

		public override void GatherDependancies()
		{
			if(_gatheredDependancies) return;
			_gatheredDependancies = true;
			if(_vTablePointer is not null) return;
			_vTablePointer = igArkCore.GetNewStructTypeBuilder(_name);

			for(int i = 0; i < _fieldList.Count; i++)
			{
				ReadyCompoundFieldDependancy(_fieldList[i]);
			}

			igArkCore._pendingTypes.Add(this);

			for(int i = 0; i < _fieldList.Count; i++)
			{
				ReadyFieldDependancy2(_fieldList[i]);
			}
		}
		public override void DefineType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_fieldList[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				_fieldList[i]._fieldHandle = tb.DefineField(_fieldList[i]._fieldName, _fieldList[i].GetOutputType(), attrs);
			}
		}
		public override void CreateType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			if(!_beganFinalization)
			{
				_beganFinalization = true;

				Logging.Info("Finalizing {0}", _name);
				
				Type testType = tb.CreateType();
				_vTablePointer = testType;

				for(int i = 0; i < _fieldList.Count; i++)
				{
					_fieldList[i]._fieldHandle = _vTablePointer.GetField(_fieldList[i]._fieldName!, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
				}

				Logging.Info("Finalized!");

				_finishedFinalization = true;
			}
		}

		public void CalculateOffsets()
		{
			igMetaEnum platformEnum = igArkCore.GetMetaEnum("IG_CORE_PLATFORM");
			IG_CORE_PLATFORM[] platforms = new IG_CORE_PLATFORM[platformEnum._names.Count];
			
			for(int i = 0; i < platforms.Length; i++)
			{
				platforms[i] = (IG_CORE_PLATFORM)platformEnum.GetEnumFromName(platformEnum._names[i]);

				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) continue;

				CalculateOffsetForPlatform(platforms[i]);
			}
		}
		public void CalculateOffsetForPlatform(IG_CORE_PLATFORM platform)
		{
			igMetaField[] metaFieldsByOffset = _fieldList.OrderBy(x => x._offset).ToArray();
			ushort currentOffset = 0;
			for(int i = 0; i < metaFieldsByOffset.Length; i++)
			{
				if(metaFieldsByOffset[i] is igStaticMetaField) continue;
				if(metaFieldsByOffset[i] is igPropertyFieldMetaField) continue;
				if(metaFieldsByOffset[i] is igBitFieldMetaField) continue;
				if(!metaFieldsByOffset[i].IsApplicableForPlatform(platform)) continue;
				if(metaFieldsByOffset[i]._offsets.ContainsKey(platform)) continue;

				Align(ref currentOffset, metaFieldsByOffset[i].GetAlignment(platform));

				metaFieldsByOffset[i]._offsets.Add(platform, currentOffset);

				currentOffset += (ushort)metaFieldsByOffset[i].GetSize(platform);
			}
		}
		public override igMetaField? GetFieldByName(string name)
		{
			int index = _fieldList.FindIndex(x => x._fieldName == name);
			if(index < 0) return null;
			return _fieldList[index];
		}
		private void Align(ref ushort offset, uint alignment)
		{
			offset = (ushort)(((offset + (alignment - 1)) / alignment) * alignment);
		}
		internal object ConstructInstance(Type t, List<igMetaField> fields)
		{
			object obj = Activator.CreateInstance(t)!;
			ResetFields(obj, fields);
			return obj;
		}
		public object ConstructInstance(Type t, bool setFields = true)
		{
			object obj = Activator.CreateInstance(t)!;
			if(setFields) ResetFields(obj);
			return obj;
		}
		internal void ResetFields(object dat, List<igMetaField> fields)
		{
			for(int i = 0; i < fields.Count; i++)
			{
				if(fields[i] is igStaticMetaField || fields[i] is igPropertyFieldMetaField || fields[i] is igBitFieldMetaField) continue;

				//Defaults not working cos they're not in the metadata iirc
				FieldInfo? field = fields[i]._fieldHandle;

				object? data = fields[i].GetDefault(igMemoryContext.Singleton.GetMemoryPoolByName("Default"));

				if(fields[i].GetOutputType().IsValueType && data == null) continue;

				field.SetValue(dat, data);
			}
		}
		public void ResetFields(object dat) => ResetFields(dat, _fieldList);
	}

	public class igStruct : Attribute{}
}