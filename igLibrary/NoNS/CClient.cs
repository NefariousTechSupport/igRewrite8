/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary
{
	public class CClient : igSingleton<CClient>
	{
		public igVector<CManager> _managers = new igVector<CManager>();
		public void AddManager<T>() where T : CManager, new()
		{
			T manager = new T();
			FieldInfo? instanceField = typeof(T).GetField("_Instance");
			if(instanceField == null) throw new Exception($"{typeof(T).FullName} class was incorrectly declared, and is missing an _Instance field");
			instanceField.SetValue(null, manager);
			_managers.Append(manager);
			manager.Intialize();
		}
	}
}