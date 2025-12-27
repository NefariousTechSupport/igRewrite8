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
		public List<uint> _handleList = new List<uint>();		//technically igNamespaceHashHandleTable
		public Dictionary<igObject, igHandle> _objectToHandleTable = new Dictionary<igObject, igHandle>();
		public Dictionary<ulong, igHandle> _handleTable = new Dictionary<ulong, igHandle>();
		//public igHandleRedirectPool _handleRedirectPool;

		public void AddSystemNamespace(string name)
		{
			if(!_systemNamespaces.Contains(name))
			{
				_systemNamespaces.Append(name);
			}
		}
		public void RemoveSystemNamespace(string name)
		{
			_systemNamespaces.Remove(name);
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

		private ulong GetHandleKey(igName ns, igName name) => (((ulong)ns._hash) << 32) | name._hash;

		public igHandle LookupHandle(igHandleName handleName) => LookupHandle(handleName._ns, handleName._name);
		public igHandle LookupHandle(igName ns, igName name)
		{
			if (!_handleTable.TryGetValue(GetHandleKey(ns, name), out igHandle? handle))
			{
				handle = new igHandle();
				handle._namespace = ns;
				handle._alias = name;
				_handleTable.Add(GetHandleKey(ns, name), handle);
			}

			// Attempt to set up the strings properly
			if (ns._string != null && handle._namespace._string == null)
			{
				handle._namespace._string = ns._string;
			}

			if (name._string != null && handle._alias._string == null)
			{
				handle._alias._string = name._string;
			}

			return handle;
		}

		public void RemoveHandle(igHandleName handleName)
		{
			ulong key = GetHandleKey(handleName._ns, handleName._name);
			if (_handleTable.TryGetValue(key, out igHandle? handle))
			{
				_handleTable.Remove(key);

				igObject? obj = handle._object;
				if (obj != null)
				{
					_objectToHandleTable.Remove(obj);
				}
			}
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
			if(_handleList.Contains(dir._name._hash)) return;
			_handleList.Add(dir._name._hash);

			for (int i = 0; i < dir._objectList._count; i++)
			{
				igHandle hnd = LookupHandle(dir._name, dir._nameList![i]);

				_objectToHandleTable.Add(dir._objectList[i], hnd);
			}
		}
		public igHandle AddObject(igObject obj, igName nameSpace, igName name)
		{
			igHandle hnd = new igHandle();
			hnd._object = obj;
			hnd._namespace = nameSpace;
			hnd._alias = name;
			_objectToHandleTable.Add(obj, hnd);
			_handleTable.Add(GetHandleKey(hnd._namespace, hnd._alias), hnd);
			return hnd;
		}
		public igHandle AddObject(igObject obj, igHandleName name) => AddObject(obj, name._ns, name._name);
		public igHandle AddObject(igObjectDirectory dir, igObject obj, igName name) => AddObject(obj, dir._name, name);
		public igHandle AddObject(igObjectDirectory dir, igObject obj, uint hash) => AddObject(dir, obj, new igName(hash));
		public igHandle AddObject(igObjectDirectory dir, igObject obj, string name) => AddObject(dir, obj, new igName(name));
	}
}