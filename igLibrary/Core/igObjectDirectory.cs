/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// Represents a group of igObjects under a shared namespace
	/// Can be loaded from an IGX/IGB/IGZ file
	/// </summary>
	public class igObjectDirectory : igObject
	{
		public string _path;
		public igName _name;
		public List<igObjectDirectory> _dependencies = new List<igObjectDirectory>();
		public igObjectList _objectList = new igObjectList();
		public bool _useNameList = false;
		public igNameList? _nameList = null;
		public bool _useNamespaceList = false;
		public igNameList? _namespaceList = null;
		[Obsolete("This exists for the reflection system, do not use.")] public igDataList _memory;
		[Obsolete("This exists for the reflection system, do not use.")] public ulong _memoryUsage;
		[Obsolete("This exists for the reflection system, do not use.")] public ulong _childMemoryUsage;
		public igObject _loaderData;
		public igIGZLoader _loader;     //needs to be changed to igObjectLoader
		[Obsolete("This exists for the reflection system, do not use.")] public FileType _sourceFileType;
		[Obsolete("This exists for the reflection system, do not use.")] public int _loadCount;
		[Obsolete("This exists for the reflection system, do not use.")] public bool _allowMultipleInstances;
		[Obsolete("This exists for the reflection system, do not use.")] public igObjectList _debugObjects;
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _thumbnails;
		[Obsolete("This exists for the reflection system, do not use.")] public igObjectList _createdMetaObjects;
		[Obsolete("This exists for the reflection system, do not use.")] public igStringRefList _userSpecifiedPaths;
		public igFileDescriptor _fd;
		public static Func<string, igName, igBlockingType, igObjectDirectory?> _loadDependencyFunction = igObjectDirectory.LoadDependancyDefault;
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _assertObjectLifetimesCallback;


		/// <summary>
		/// Enum representing the different filetypes
		/// </summary>
		[igEnum]
		public enum FileType : uint
		{
			kAuto,
			kIGB,
			kIGX,
			kDataStream,
			kIGZ,
			kInvalid,	//This isn't real
		}

		public FileType _type;


		/// <summary>
		/// Default constructor
		/// </summary>
		public igObjectDirectory(){}


		/// <summary>
		/// Constructor, does not load
		/// </summary>
		/// <param name="path">The path to load from</param>
		/// <param name="nameSpace">The namespace to associate this igObjectDirectory with</param>
		public igObjectDirectory(string path, igName nameSpace)
		{
			_path = path;
			_name = nameSpace;
		}


		/// <summary>
		/// Constructor, does not load
		/// </summary>
		/// <param name="path">The path to load from, namespace is the filename without extension of the path</param>
		public igObjectDirectory(string path)
		{
			_path = path;
			_name = new igName(Path.GetFileNameWithoutExtension(path));
		}


		/// <summary>
		/// Loads the <c>igObjectDirectory</c> from a file
		/// </summary>
		public void ReadFile()
		{
			igObjectLoader loader = igObjectLoader.FindLoader(_path);
			loader.ReadFile(this, _path, igBlockingType.kMayBlock);
		}


		/// <summary>
		/// Writes the <c>igObjectDirectory</c> to a stream
		/// </summary>
		/// <param name="dst">The destination stream</param>
		/// <param name="platform">The platform to write with, defaults to the platform this <c>igObjectDirectory</c> was loaded with</param>
		public void WriteFile(Stream dst, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			if(_type == FileType.kIGZ)
			{
				igIGZSaver saver = new igIGZSaver();
				if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
				{
					saver.WriteFile(this, dst, _loader._platform);
				}
				else
				{
					saver.WriteFile(this, dst, platform);
				}
			}
		}


		/// <summary>
		/// Writes the <c>igObjectDirectory</c> to a file
		/// </summary>
		/// <param name="path">The destination file</param>
		/// <param name="platform">The platform to write with, defaults to the platform this <c>igObjectDirectory</c> was loaded with</param>
		public void WriteFile(string path, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			FileStream fs = File.Create(path);
			WriteFile(path, platform);
			fs.Close();
		}


		/// <summary>
		/// Add a new root object to the file
		/// </summary>
		/// <param name="obj">The object</param>
		/// <param name="ns">The namespace to associate it with, keep this the same as the <c>igObjectDirectory</c></param>
		/// <param name="name">The name of the object, must be null if this <c>igObjectDirectory</c> isn't using a name list</param>
		/// <exception cref="ArgumentException">Thrown if passing a non-null name if this <c>igObjectDirectory</c> doesn't use a name list</exception>
		public void AddObject(igObject obj, igName ns, igName name)
		{
			_objectList.Append(obj);
			if(_useNameList)
			{
				_nameList!.Append(name);
				igObjectHandleManager.Singleton.AddObject(this, obj, name);
			}
			else
			{
				if(name._hash != 0) throw new ArgumentException("Name is not null even though namelist is!");
			}
		}


		/// <summary>
		/// Gets the first root object of the specified type, else null
		/// </summary>
		/// <typeparam name="T">The type in question</typeparam>
		/// <returns>The first igObject of that type, else null</returns>
		public T? GetObjectOfType<T>() where T : igObject => (T?)GetObjectOfType(typeof(T));


		/// <summary>
		/// Gets the first root object of the specified metaobject, else null
		/// </summary>
		/// <param name="metaObject">The type in question</param>
		/// <returns>The first igObject of that type, else null</returns>
		public igObject? GetObjectOfType(igMetaObject metaObject) => GetObjectOfType(metaObject._vTablePointer!);


		/// <summary>
		/// Gets the first root object of the specified type, else null
		/// </summary>
		/// <param name="type">The type in question</param>
		/// <returns>The first igObject of that type, else null</returns>
		public igObject? GetObjectOfType(Type type)
		{
			for(int o = 0; o < _objectList._count; o++)
			{
				if(_objectList[o].GetType().IsAssignableTo(type))
				{
					return _objectList[o];
				}
			}
			return null;
		}


		/// <summary>
		/// Gets all the root objects of the specified type
		/// </summary>
		/// <typeparam name="T">The type in question</typeparam>
		/// <returns>an <c>igObjectList</c> containing the root objects</returns>
		public igObjectList GetObjectsOfType<T>() where T : igObject => GetObjectsOfType(typeof(T));


		/// <summary>
		/// Gets all the root objects of the specified metaobject
		/// </summary>
		/// <param name="metaObject">The metaobject in question</typeparam>
		/// <returns>an <c>igObjectList</c> containing the root objects</returns>
		public igObjectList GetObjectsOfType(igMetaObject metaObject) => GetObjectsOfType(metaObject._vTablePointer!);


		/// <summary>
		/// Gets all the root object of the specified type
		/// </summary>
		/// <param name="type">The type in question</param>
		/// <returns>an <c>igObjectList</c> containing the root objects</returns>
		public igObjectList GetObjectsOfType(Type type)
		{
			igObjectList objects = new igObjectList();

			for(int o = 0; o < _objectList._count; o++)
			{
				if(_objectList[o].GetType().IsAssignableTo(type))
				{
					objects.Append(_objectList[o]);
				}
			}

			return objects;
		}
		public static igObjectDirectory? LoadDependancyDefault(string path, igName name, igBlockingType idk)
		{
			return igObjectStreamManager.Singleton.Load(path, name);
		}
		public static igObjectDirectory? LoadDependancyDefault(string filePath, igName nameSpace)
		{
			return igObjectStreamManager.Singleton.Load(filePath, nameSpace);
		}
		public static FileType GetLoader(string filePath)
		{
			igFilePath path = new igFilePath();
			path.Set(filePath);
			switch(path._extension.ToString())
			{
				case "igz":
				case "lng":
				case "pak":	//not to be confused with the archive extension
				case "bld":	//not to be confused with the archive extension
					return FileType.kIGZ;
				default:
					return FileType.kInvalid;
					//throw new InvalidOperationException($"Invalid filetype {path._fileExtension}");
			}
		}
	}
	public class igObjectDirectoryList : igTObjectList<igObjectDirectory> {}
}