namespace igLibrary.Core
{
	public class igObjectStreamManager : igSingleton<igObjectStreamManager>
	{
		//change to Dictionary<igName, igObjectDirectory>
		public Dictionary<uint, igObjectDirectoryList> _directoriesByName = new Dictionary<uint, igObjectDirectoryList>();
		public Dictionary<uint, igObjectDirectory> _directoriesByPath = new Dictionary<uint, igObjectDirectory>();
		public void AddObjectDirectory(igObjectDirectory dir, string filePath)
		{
			igObjectDirectoryList? list = null;
			if(!_directoriesByName.TryGetValue(dir._name._hash, out list))
			{
				list = new igObjectDirectoryList();
				_directoriesByName.Add(dir._name._hash, list);
			}
			list.Append(dir);
			_directoriesByPath.Add(igHash.Hash(filePath), dir);
		}
		public igObjectDirectory? Load(string path)
		{
			return Load(path, new igName(Path.GetFileNameWithoutExtension(path)));
		}
		public igObjectDirectory? Load(string path, igName nameSpace)
		{
			string filePath = igFilePath.GetNativePath(path);
			uint filePathHash = igHash.Hash(filePath);

			igObjectDirectory objDir;
			string result;

			if(_directoriesByPath.ContainsKey(filePathHash))
			{
				result = "was previously loaded.";
				objDir = _directoriesByPath[filePathHash];
			}
			else
			{
				result = "was not previously loaded.";
				objDir = new igObjectDirectory(filePath, nameSpace);
				AddObjectDirectory(objDir, filePath);
				objDir.ReadFile();
				igObjectHandleManager.Singleton.AddDirectory(objDir);
			}

			Logging.Info("igObjectStreamManager was asked to load {0}... {1}", filePath, result);

			return objDir;
		}
	}
}