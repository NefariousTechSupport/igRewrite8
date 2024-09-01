using System.Reflection;
using System.Reflection.Emit;

namespace igLibrary.Core
{
	public class igMetaObject : igBaseMeta
	{
		[Obsolete("This exists for the reflection system, do not use.")] public int _id = -1;
		[Obsolete("This exists for the reflection system, do not use.")] public int _tfbId = -1;
		[Obsolete("This exists for the reflection system, do not use.")] public int _instanceCount = -1;
		public igMetaObject? _parent;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaObject _lastChild = null;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaObject _nextSibling = null;
		[Obsolete("This exists for the reflection system, do not use.")] public ushort _index = 0xFFFF;
		[Obsolete("This exists for the reflection system, use _sizes instead.")] public ushort _sizeofSize = 0xFFFF;
		[Obsolete("This exists for the reflection system, do not use.")] public ushort _properties;
		[Obsolete("This exists for the reflection system, use _alignments instead.")] public ushort _requiredAlignment = 0x4;
		public List<igMetaField> _metaFields;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _metaFunctions;
		public igObjectList? _attributes;
		public Dictionary<IG_CORE_PLATFORM, ushort> _sizes = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Dictionary<IG_CORE_PLATFORM, ushort> _alignments = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Type? _vTablePointer;
		private bool _baseFieldsInherited = false;
		public List<igMetaObject> _children = new List<igMetaObject>();


		public igMetaObject()
		{
			_metaFields = new List<igMetaField>();
		}
		public T? GetAttribute<T>() where T : igObject
		{
			if(_attributes == null) return null;
			for(int i = 0; i < _attributes._count; i++)
			{
				if(_attributes[i] is T attr) return attr;
			}
			return null;
		}
		public override void GatherDependancies()
		{
			if(_gatheredDependancies) return;
			_gatheredDependancies = true;
			if(_vTablePointer is not null) return;
			_vTablePointer = igArkCore.GetNewTypeBuilder(_name);

			_parent!.GatherDependancies();

			for(int i = 0; i < _metaFields.Count; i++)
			{
				ReadyCompoundFieldDependancy(_metaFields[i]);
			}
			for(int i = 0; i < _parent._metaFields.Count; i++)
			{
				if(_metaFields[i] != _parent._metaFields[i]) ReadyFieldDependancy2(_metaFields[i]);
			}

			igArkCore._pendingTypes.Add(this);

			for(int i = _parent._metaFields.Count; i < _metaFields.Count; i++)
			{
				ReadyFieldDependancy2(_metaFields[i]);
			}

		}
		public override void DefineType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			Type parentType = _parent._vTablePointer;

			if(_parent._name == "igDataList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];
				parentType = typeof(igTDataList<>).MakeGenericType(dataField._memType.GetOutputType());
			}
			else if(_parent._name == "igObjectList" || _parent._name == "igNonRefCountedObjectList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTObjectList<>).MakeGenericType(dataField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}
			else if(_parent._name == "igHashTable")
			{
				igMemoryRefMetaField valuesField = (igMemoryRefMetaField)_metaFields[0];
				igMemoryRefMetaField keysField = (igMemoryRefMetaField)_metaFields[1];

				parentType = typeof(igTUHashTable<,>).MakeGenericType(valuesField._memType.GetOutputType(), keysField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}

			tb.SetParent(parentType);
			for(int i = _parent._metaFields.Count; i < _metaFields.Count; i++)
			{
				if(_metaFields[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_metaFields[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				_metaFields[i]._fieldHandle = tb.DefineField(_metaFields[i]._fieldName!, _metaFields[i].GetOutputType(), attrs);
			}
		}
		public override void CreateType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			if(!_parent!._finishedFinalization) throw new TypeLoadException("Derived class being initialized before parent.");
			if(!_beganFinalization)
			{
				_beganFinalization = true;

				Logging.Info("Finalizing {0}... ", _name);

				Type testType = tb.CreateType()!;
				igArkCore.AddDynamicTypeToCache(testType!);
				_vTablePointer = testType;

				for(int i = 0; i < _metaFields.Count; i++)
				{
					//I gave up on making this look neat
					if(i < _parent._metaFields.Count)
					{
						if(_parent._metaFields[i] != _metaFields[i])
						{
							_metaFields[i]._fieldHandle = _vTablePointer.GetField(_metaFields[i]._fieldName!, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
						}
					}
					else
					{
						_metaFields[i]._fieldHandle = _vTablePointer.GetField(_metaFields[i]._fieldName!, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
					}
				}

				Type parentType = testType.BaseType!;
				if(_parent._name == "igDataList")
				{
					igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];
					dataField._fieldHandle = parentType.GetField("_data")!;
					_metaFields[0]._fieldHandle = parentType.GetField("_count")!;
					_metaFields[1]._fieldHandle = parentType.GetField("_capacity")!;
				}
				else if(_parent._name == "igObjectList" || _parent._name == "igNonRefCountedObjectList")
				{
					igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

					dataField._fieldHandle = parentType.GetField("_data")!;
					_metaFields[0]._fieldHandle = parentType.GetField("_count")!;
					_metaFields[1]._fieldHandle = parentType.GetField("_capacity")!;
				}
				else if(_parent._name == "igHashTable")
				{
					igMemoryRefMetaField valuesField = (igMemoryRefMetaField)_metaFields[0];
					igMemoryRefMetaField keysField = (igMemoryRefMetaField)_metaFields[1];

					valuesField._fieldHandle = parentType.GetField("_values")!;
					keysField._fieldHandle = parentType.GetField("_keys")!;

					_metaFields[2]._fieldHandle = parentType.GetField("_hashItemCount")!;
					_metaFields[3]._fieldHandle = parentType.GetField("_autoRehash")!;
					_metaFields[4]._fieldHandle = parentType.GetField("_loadFactor")!;
				}

				Logging.Info("Finalized!");

				_finishedFinalization = true;
			}
		}

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetObjectDotNetType(_name);

			//Sometimes is null, in those cases, a dynamic type will be created
			if(_vTablePointer != null)
			{
				_beganFinalizationPrep = true;
				_finishedFinalizationPrep = true;
				_beganFinalization = true;
				_finishedFinalization = true;

				if(_parent == null) return;

				FieldInfo[] fields = _vTablePointer.GetFields();

				//The following is complicated because one type can have multiple fields of the same name, this is to compensate for that
				for(int i = 0; i < _metaFields.Count; i++)
				{
					//don't worry if a parent is null, that'll only happen for __internalObjectBase which doesn't have any fields
					if(i < _parent!._metaFields.Count && _metaFields[i] == _parent._metaFields[i]) continue;
					if(_metaFields[i] is igPropertyFieldMetaField) continue;

					IEnumerable<FieldInfo> applicable = fields.Where(x => x.Name == _metaFields[i]._fieldName);
					     if(applicable.Count() == 1) _metaFields[i]._fieldHandle = applicable.ElementAt(0);
					else if(_vTablePointer.GetProperty(_metaFields[i]._fieldName!) != null) continue;
					else if(applicable.Count() == 0) throw new NotImplementedException($"Field {_metaFields[i]._fieldName} is missing from type {_name}. Contact a developer.");
					else
					{
						IEnumerable<igMetaField> applicableFields = _metaFields.Where(x => x._fieldName == _metaFields[i]._fieldName);
						if(applicableFields.Count() != applicable.Count()) throw new NotImplementedException($"Mismatch between variable numbers for {_metaFields[i]._fieldName} in type {_name}. Contact a developer.");
						for(int j = 0; j < applicableFields.Count(); j++)
						{
							applicableFields.ElementAt(j)._fieldHandle = applicable.ElementAt(j);
						}
					}
				}
			}
		}
		public bool CanBeAssignedTo(igMetaObject other)
		{
			igMetaObject? tester = this;
			while(tester != null)
			{
				if(tester == other) return true;
				tester = tester._parent;
			}
			return false;
		}
		public int GetFieldIndexByName(string name)
		{
			int index = _metaFields.FindIndex(x => x._fieldName == name);
			return index;
		}
		public override igMetaField? GetFieldByName(string name)
		{
			int index = GetFieldIndexByName(name);
			if(index < 0) return null;
			return _metaFields[index];
		}
		public void ValidateAndSetField(int index, igMetaField field)
		{
			field._parentMeta = this;
			_metaFields[index] = field;
		}
		public void InheritFields()
		{
			//if(_baseFieldsInherited) return;
			//_baseFieldsInherited = true;
			if(_parent == null) return;
			//_parent.InheritFields();
			_parent._children.Add(this);
			_metaFields.AddRange(_parent._metaFields);
		}
		public void AppendDynamicField(igMetaField field)
		{
			field._offset = (ushort)(_metaFields.Max(x => x._offset) + 1u);
			field._parentMeta = this;
			_metaFields.Add(field);
		}
		public void CalculateOffsets()
		{
			igMetaEnum platformEnum = igArkCore.GetMetaEnum("IG_CORE_PLATFORM");
			IG_CORE_PLATFORM[] platforms = new IG_CORE_PLATFORM[platformEnum._names.Count];

			for(int i = 0; i < platforms.Length; i++)
			{
				platforms[i] = (IG_CORE_PLATFORM)platformEnum._values[i];

				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) continue;

				CalculateOffsetForPlatform(platforms[i]);
			}
		}
		public void CalculateOffsetForPlatform(IG_CORE_PLATFORM platform)
		{
			//This feels a bit wasteful with how this is getting recalculated a lot but uhhh

			if(_parent != null) _parent.CalculateOffsetForPlatform(platform);
			ushort alignment = (ushort)igAlchemyCore.GetPointerSize(platform);	//alignment set to alignof pointer cos vtable
			igMetaField[] metaFieldsByOffset = _metaFields.OrderBy(x => x._offset).ToArray();
			ushort currentOffset = (ushort)(4u + igAlchemyCore.GetPointerSize(platform));
			igMetaField? maxOffset = null;
			if(_parent != null)
			{
				for(int i = 0; i < _parent._metaFields.Count; i++)
				{
					if(!_metaFields[i].IsApplicableForPlatform(platform)) continue;
					if(maxOffset == null) maxOffset = _metaFields[i];
					if(maxOffset._offset < _metaFields[i]._offset) maxOffset = _metaFields[i];
				}
			}
			bool surpassedMaxOffset = false;
			for(int i = 0; i < metaFieldsByOffset.Length; i++)
			{
				if(!metaFieldsByOffset[i].IsApplicableForPlatform(platform)) continue;

				if(maxOffset != null && !surpassedMaxOffset && metaFieldsByOffset[i]._offset > maxOffset._offset && platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE)	//Dumb Wii U alignment rule
				{
					surpassedMaxOffset = true;
					currentOffset = _parent._sizes[platform];
				}

				if(metaFieldsByOffset[i] is igStaticMetaField) continue;
				if(metaFieldsByOffset[i] is igPropertyFieldMetaField) continue;
				if(metaFieldsByOffset[i] is igBitFieldMetaField bfMf)
				{
					if(!bfMf._offsets.ContainsKey(platform))
					{
						bfMf._offsets.Add(platform, bfMf._storageMetaField._offsets[platform]);
					}
					continue;
				}

				alignment = (ushort)System.Math.Max(alignment, metaFieldsByOffset[i].GetAlignment(platform));

				Align(ref currentOffset, metaFieldsByOffset[i].GetAlignment(platform));

				if(metaFieldsByOffset[i]._offsets.ContainsKey(platform))
					goto addOffsetAndContinue;

				metaFieldsByOffset[i]._offsets.Add(platform, currentOffset);

			addOffsetAndContinue:
				currentOffset += (ushort)metaFieldsByOffset[i].GetSize(platform);
			}

			if(!_alignments.ContainsKey(platform))
			{
				_alignments.Add(platform, alignment);
			}
			if(!_sizes.ContainsKey(platform))
			{
				Align(ref currentOffset, alignment);
				_sizes.Add(platform, currentOffset);
			}
		}
		private void Align(ref ushort offset, uint alignment)
		{
			offset = (ushort)(((offset + (alignment - 1)) / alignment) * alignment);
		}
		public virtual void AppendToArkCore()
		{
			if(_inArkCore) return;
			igArkCore.AddObjectMeta(this);
			_inArkCore = true;
		}
		public virtual igObject ConstructInstance(igMemoryPool memPool, bool setFields = true)
		{
			if(_vTablePointer == null)
			{
				GatherDependancies();
				igArkCore.FlushPendingTypes();
			}
			igObject obj = (igObject)Activator.CreateInstance(_vTablePointer);
			obj.internalMemoryPool = memPool;
			obj.internalMeta = this;
			if(setFields) obj.ResetFields(this);
			return obj;
		}
		public void CorrectObjectMeta(igObject obj)
		{
			FieldInfo? fi = _vTablePointer.GetField("_meta");
			if(fi != null)
			{
				fi.SetValue(obj, this);
			}
		}
		internal void ApplyFixup(int index, igMetaField field)
		{
			_metaFields.Insert(index, field);
			for(int i = 0; i < _children.Count; i++)
			{
				_children[i].ApplyFixup(index, field);
			}
		}


		/// <summary>
		/// Constructs an instance of a given type
		/// </summary>
		/// <typeparam name="T">The igObject to construct</typeparam>
		/// <param name="memPool">The memory pool to create it in</param>
		public static T ConstructInstance<T>(igMemoryPool? memPool = null) where T : igObject
		{
			igMetaObject? meta = igArkCore.GetObjectMeta(typeof(T).Name);
			if(meta == null)
			{
				throw new TypeLoadException($"Failed to load type {typeof(T).Name} while constructing");
			}

			if(memPool == null)
			{
				memPool = igMemoryContext.Default;
			}

			return (T)meta.ConstructInstance(memPool);
		}


		/// <summary>
		/// Constructs an instance of a given type
		/// </summary>
		/// <typeparam name="T">The igObject to construct</typeparam>
		/// <param name="memPool">The name of the memory pool to use</param>
		/// <returns></returns>
		public static T ConstructInstance<T>(string memPool) where T : igObject => ConstructInstance<T>(igMemoryContext.Singleton.GetMemoryPoolByName(memPool)!);
	}
}