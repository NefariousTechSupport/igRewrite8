using System.Reflection;

namespace igLibrary.Core
{
	public class igObjectRefMetaField : igRefMetaField
	{
		public igMetaObject _metaObject;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaObject._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaObject = igArkCore.GetObjectMeta(loader.ReadString(sh));
		}
		public override object? ReadIGZField(igIGZLoader loader)
		{
			bool isExid = loader._runtimeFields._externals.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			bool isOffset = loader._runtimeFields._offsets.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			bool isNamedExternal = loader._runtimeFields._namedExternals.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			igSizeTypeMetaField sizeTypeMetaField = new igSizeTypeMetaField();
			ulong raw = (ulong)sizeTypeMetaField.ReadIGZField(loader);
			igObject? ret = null;
			if(raw == 0) return null;
			if(isNamedExternal)
			{
				ret = loader._namedExternalList[(int)(raw & 0x7FFFFFFF)];
			}
			else if(isOffset)
			{
				ret = loader._offsetObjectList[raw];
			}
			else if(isExid)
			{
				ret = loader._externalList[(int)(raw & 0x7FFFFFFF)].GetObjectAlias<igObject>();
			}
			else
			{
				Console.WriteLine("uhhhhhhhhhhhhhhhhh");
			}
			return ret;
			//if(ret is T t) return t;
			//else return null;
		}
		public void WriteIGZFieldShallow(igIGZSaver saver, igIGZSaver.SaverSection section, igObject? obj, out ulong serializedOffset, out bool needsDeep)
		{
			ulong baseOffset = section._sh.Tell64();
			needsDeep = false;
			serializedOffset = 0;

			if(obj == null)
			{
				saver.WriteRawOffset(0, section);
				return;
			}
			bool addDepItems = GetAttribute<igAddDependencyItemsAttribute>() != null;
			if(addDepItems)
			{
				AddDependencyItems((IigDataList)obj, saver._platform, saver._dir, out igStringRefList buildDeps, out igStringRefList fileDeps);
				for(int i = 0; i < buildDeps._count; i++) saver.AddBuildDependency(buildDeps[i]);
				for(int i = 0; i < fileDeps._count; i++) saver.AddFileDependency(fileDeps[i]);
			}

			igExternalReferenceSystem.Singleton._globalSet.MakeReference(obj, null, out igHandleName name);
			if(name._ns._hash != 0)
			{
				section._runtimeFields._namedExternals.Add(section._sh.Tell64());
				section._sh.WriteUInt32((uint)saver.GetOrAddHandle((new igHandle(name), false)) | (_refCounted ? 0x80000000 : 0));
				serializedOffset = 0;
				return;
			}

			igHandle hnd = igObjectHandleManager.Singleton.GetHandle(obj);
			if(hnd != null && hnd._namespace._hash != saver._dir._name._hash)
			{
				if(igObjectHandleManager.Singleton.IsSystemObject(hnd))
				{
					Console.WriteLine("EXID object found, reference to " + hnd.ToString());
					int index = saver._externalList.FindIndex(x => x == hnd);
					if(index < 0)
					{
						index = saver._externalList.Count;
						saver._externalList.Add(hnd);
					}
					section._runtimeFields._externals.Add(section._sh.Tell64());
					section._sh.WriteUInt32((uint)index | (_refCounted ? 0x80000000 : 0));
				}
				else
				{
					Console.WriteLine("EXNM object found, reference to " + hnd.ToString());
					section._runtimeFields._namedExternals.Add(section._sh.Tell64());
					section._sh.WriteUInt32((uint)saver.GetOrAddHandle((hnd, false)) | (_refCounted ? 0x80000000 : 0));
				}
				serializedOffset = 0;
				return;
			}

			serializedOffset = saver.SaveObjectShallow(obj, out needsDeep);
			section._sh.Seek(baseOffset);
			saver.WriteRawOffset(serializedOffset, section);
			section._runtimeFields._offsets.Add(baseOffset);
			if(_refCounted && obj != null)
			{
				saver.RefObject(obj);
			}
		}
		private void AddDependencyItems(IigDataList list, IG_CORE_PLATFORM platform, igObjectDirectory dir, out igStringRefList buildDeps, out igStringRefList fileDeps)
		{
			igObject? item;
			buildDeps = new igStringRefList();
			fileDeps = new igStringRefList();
			for(int i = 0; i < list.GetCount(); i++)
			{
				object obj = list.GetObject(i);
				if(obj == null) continue;
				item = obj as igObject;
				if(item == null)
				{
					if(obj is igHandle hnd) item = hnd.GetObjectAlias<igObject>();
					else throw new InvalidOperationException("Item type is not valid");
				}
				item.GetDependencies(platform, dir, out igStringRefList? itemBuildDeps, out igStringRefList? itemFileDeps);
				if(itemBuildDeps != null)
				{
					for(int j = 0; j < itemBuildDeps._count; j++)
					{
						buildDeps.Append(itemBuildDeps[i]);
					}
				}
				if(itemFileDeps != null)
				{
					for(int j = 0; j < itemFileDeps._count; j++)
					{
						fileDeps.Append(itemFileDeps[i]);
					}
				}
			}
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			WriteIGZFieldShallow(saver, section, (igObject?)value, out ulong serializedOffset, out bool needsDeep);
			if(needsDeep)
			{
				saver.SaveObjectDeep(serializedOffset, (igObject?)value);
			}
		}
		public override Type GetOutputType()
		{
			//if(_metaObject._vTablePointer == typeof(igBlindObject)) return typeof(igObject);
			if(_metaObject._vTablePointer == null) _metaObject.GatherDependancies();
			return _metaObject._vTablePointer;
		}
		public override object? GetDefault(igObject target)
		{
			if(_construct)
			{
				igObject obj = _metaObject.ConstructInstance(target.internalMemoryPool);
				igCapacityAttribute? capacityAttr = GetAttribute<igCapacityAttribute>();
				if(capacityAttr != null)
				{
					if(obj is IigDataList dataList) dataList.SetCapacity(capacityAttr._value);
					else if (obj is IigHashTable hashTable) hashTable.Activate(capacityAttr._value);
				}
				return obj;
			}
			else return _default;
		}
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			if(_metaObject._name == "igMetaObject")
			{
				sh.WriteInt32(4);
				saver.SaveString(sh, ((igMetaObject)_default)._name);
			}
			else
			{
				sh.WriteInt32(-1);
			}
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = igArkCore.GetObjectMeta(loader.ReadString(sh));
		}
	}
	public class igObjectRefArrayMetaField : igObjectRefMetaField
	{
		public short _num;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			Array data = Array.CreateInstance(base.GetOutputType(), _num);
			for(int i = 0; i < _num; i++)
			{
				data.SetValue(base.ReadIGZField(loader), i);
			}
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			Array data = (Array)value;
			for(int i = 0; i < _num; i++)
			{
				base.WriteIGZField(saver, section, data.GetValue(i));
			}
		}
		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			return base.GetSize(platform) * (uint)_num;
		}
		public override Type GetOutputType()
		{
			return base.GetOutputType().MakeArrayType();
		}
	}
}