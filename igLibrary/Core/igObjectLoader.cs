/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public abstract class igObjectLoader : igObject
	{
		public static Dictionary<string, igObjectLoader> _loaders = new Dictionary<string, igObjectLoader>();
		public static uint _testFileMaxSize;
		public static void RegisterClass<T>() where T : igObjectLoader, new()
		{
			T loader = new T();
			string extension = loader.GetLoaderExtension().ToLower();
			_loaders.TryAdd(loader.GetLoaderType(), loader);
			uint testFileMemorySize = loader.GetTestFileMemorySize();
			RegisterLoader(loader, extension);
			if(testFileMemorySize > _testFileMaxSize)
			{
				_testFileMaxSize = testFileMemorySize;
			}
		}
		public static void RegisterLoader(igObjectLoader loader, string extension)
		{
			_loaders.Add(extension, loader);
		}
		public static igObjectLoader FindLoader(string filePath)
		{
			igFilePath path = new igFilePath();
			path.Set(filePath);
			_loaders.TryGetValue(path._extension, out igObjectLoader? loader);
			if(loader == null) throw new KeyNotFoundException($"Loader for {filePath} files missing.");
			return loader;
		}
		//Technically these are called GetExtension, GetType, and GetName. GetType is defined in System.Object so I've added Loader in all of the names
		public virtual string GetLoaderExtension() => "";
		public virtual string GetLoaderType() => "";
		public virtual string GetLoaderName() => "";
		public virtual uint GetTestFileMemorySize() => 0;
		public virtual void ReadFile(igObjectDirectory dir, string filePath, igBlockingType blockingType){}
	}
}