/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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
			_directoriesByPath.Add(igHash.HashI(filePath), dir);
		}
		public igObjectDirectory? Load(string path)
		{
			return Load(path, new igName(Path.GetFileNameWithoutExtension(path)));
		}
		public igObjectDirectory? Load(string path, igName nameSpace)
		{
			string filePath = igFilePath.GetNativePath(path);
			uint filePathHash = igHash.HashI(filePath);

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