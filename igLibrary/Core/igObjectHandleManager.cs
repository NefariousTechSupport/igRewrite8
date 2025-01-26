/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igObjectHandleManager : igSingleton<igObjectHandleManager>
	{
		public igStringRefList _systemNamespaces = new igStringRefList();
		//public Func<int> _resolveToHandleFunction;	//placeholder params
		//public bool _assertHandleOverwrites;
		//public igMutex _lock;
		//public igSephamore _handleLock;
		//public int _highWaterCounter;
		//public igName _runtimeHandleName;
		//public uint _runtimeHandleId;
		//public igHandlesPool _handlePool;
		public List<uint> _handleTable = new List<uint>();		//technically igNamespaceHashHandleTable
		public Dictionary<igObject, igHandle> _objectToHandleTable = new Dictionary<igObject, igHandle>();
		//public igHandleRedirectPool _handleRedirectPool;

		public void AddSystemNamespace(string name)
		{
			if(!_systemNamespaces.Contains(name))
			{
				_systemNamespaces.Append(name);
			}
		}
		public bool IsSystemObject(igObject obj)
		{
			igHandle hnd = GetHandleInternal(obj);
			return IsSystemObject(hnd);
		}
		public bool IsSystemObject(igHandle hnd)
		{
			return _systemNamespaces.Contains(hnd._namespace._string);
		}

		public igHandle GetHandle(igObject obj) => GetHandleInternal(obj);
		public igHandle GetHandleInternal(igObject obj)
		{
			if(obj == null) return null;
			bool keyExists = _objectToHandleTable.TryGetValue(obj, out igHandle val);
			if(keyExists) return val;
			else return null;
		}
		public void AddDirectory(igObjectDirectory dir)
		{
			if(!dir._useNameList) return;
			if(_handleTable.Contains(dir._name._hash)) return;
			_handleTable.Add(dir._name._hash);

			for(int i = 0; i < dir._objectList._count; i++)
			{
				igHandle hnd = new igHandle();
				hnd._object = dir._objectList[i];
				hnd._namespace = dir._name;
				hnd._alias = dir._nameList[i];

				_objectToHandleTable.Add(dir._objectList[i], hnd);
			}
		}
		public void AddObject(igObjectDirectory dir, igObject obj, igName name)
		{
			igHandle hnd = new igHandle();
			hnd._object = obj;
			hnd._namespace = dir._name;
			hnd._alias = name;
			_objectToHandleTable.Add(obj, hnd);
		}
		public void AddObject(igObjectDirectory dir, igObject obj, uint hash) => AddObject(dir, obj, new igName(hash));
		public void AddObject(igObjectDirectory dir, igObject obj, string name) => AddObject(dir, obj, new igName(name));
	}
}