namespace igLibrary.Core
{
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
		[Obsolete("This exists for the reflection system, do not use.")] public igObjectList _debugObjects;
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _thumbnails;
		[Obsolete("This exists for the reflection system, do not use.")] public igObjectList _createdMetaObjects;
		[Obsolete("This exists for the reflection system, do not use.")] public igStringRefList _userSpecifiedPaths;
		public igFileDescriptor _fd;
		public static Func<string, igName, igBlockingType, igObjectDirectory?> _loadDependencyFunction = igObjectDirectory.LoadDependancyDefault;
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _assertObjectLifetimesCallback;

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

		public igObjectDirectory(){}
		public igObjectDirectory(string path, igName nameSpace)
		{
			_path = path;
			_name = nameSpace;
		}
		public igObjectDirectory(string path)
		{
			_path = path;
			_name = new igName(Path.GetFileNameWithoutExtension(path));
		}
		public void ReadFile()
		{
			igObjectLoader loader = igObjectLoader.FindLoader(_path);
			loader.ReadFile(this, _path, igBlockingType.kMayBlock);
		}
		public void WriteFile(Stream dst, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			_objectList.internalMemoryPool = igMemoryContext.Default;
			if(_nameList != null) _nameList.internalMemoryPool = igMemoryContext.Default;
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
		public void WriteFile(string path, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			FileStream fs = File.Create(path);
			WriteFile(path, platform);
			fs.Close();
		}
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
		public void BuildIGZ(string path)
		{
			
		}
	}
	public class igObjectDirectoryList : igTObjectList<igObjectDirectory> {}
}