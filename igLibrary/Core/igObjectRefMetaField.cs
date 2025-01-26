/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary.Core
{
	public class igObjectRefMetaField : igRefMetaField
	{
		private static igObjectRefMetaField _MetaField = new igObjectRefMetaField();
		public static igObjectRefMetaField GetMetaField() => _MetaField;

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
			ulong baseOffset = loader._stream.Tell64();
			igSizeTypeMetaField sizeTypeMetaField = igSizeTypeMetaField._MetaField;
			ulong raw = (ulong)sizeTypeMetaField.ReadIGZField(loader)!;
			igObject? ret = null;
			bool isOffset = loader._runtimeFields._offsets.BinarySearch(baseOffset) >= 0;
			if(isOffset)
			{
				return loader._offsetObjectList[raw];
			}
			bool isNamedExternal = loader._runtimeFields._namedExternals.BinarySearch(baseOffset) >= 0;
			if(isNamedExternal)
			{
				return loader._namedExternalList[(int)(raw & 0x7FFFFFFF)];
			}
			bool isExid = loader._runtimeFields._externals.BinarySearch(baseOffset) >= 0;
			if(isExid)
			{
				return loader._externalList[(int)(raw & 0x7FFFFFFF)].GetObjectAlias<igObject>();
			}
			if(raw != 0)
				throw new InvalidDataException("Failed to read igObjectRefMetaField properly");
			return ret;
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
					Logging.Info("EXID object found, reference to {0}", hnd.ToString());
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
					Logging.Info("EXNM object found, reference to {0}", hnd.ToString());
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
				if(item != null)
				{
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
			if(_metaObject._vTablePointer == null) _metaObject.GatherDependancies();
			return _metaObject._vTablePointer!;
		}
		public override object? GetDefault(igMemoryPool pool)
		{
			if(_construct)
			{
				igObject obj = _metaObject.ConstructInstance(pool);
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
}