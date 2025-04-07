/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igFilePath
	{
		public string _media;
		public string _directory;
		public string _file;
		public string _extension;
		public string _mediaDirectory;
		public string _fileExtension;
		public string _path;
		public string _nativePath;
		public static string _defaultMedia = "isHostFileSystemMountedInternal";
		private static igFilePath _utilFp = new igFilePath();

		public string NativePrefix => string.Empty;
		public string NativeAllowRelativePaths => string.Empty;
		public char NativeSeperator => '/';
		public string NativeActivePrefix => string.Empty;
		public bool NativeCaseSensitive => true;

#pragma warning disable CS8618
	public igFilePath() => Reset();
#pragma warning restore CS8618

		public static string GetNativePath(string path)
		{
			_utilFp.Set(path);
			return _utilFp._path;
		}

		public void Reset()
		{
			_media = string.Empty;
			_directory = string.Empty;
			_file = string.Empty;
			_extension = string.Empty;
			_mediaDirectory = string.Empty;
			_fileExtension = string.Empty;
			_path = string.Empty;
			_nativePath = string.Empty;
		}

		public void Set(string path)
		{
			Reset();
			path.Replace('\\', '/');
			int mediaIndex = path.IndexOf(':');
			if(mediaIndex == 1)
			{
				_path = path;
				_fileExtension = Path.GetExtension(path);
				_extension = _fileExtension.Length > 0 ? _fileExtension.Substring(1) : _fileExtension;
				return;
			}
			if(mediaIndex >= 0)
			{
				_media = path.Substring(0, mediaIndex);
			}
			_mediaDirectory = igFileContext.Singleton.GetMediaDirectory(_media);
			int finalDir = -1;
			if(_media == "cwd")
			{
				//string cwdDir = $"/Temporary/BuildServer/{igCore.GetPlatformString(igCore.platform)}/Output";
				//mediaIndex += cwdDir.Length;
			}
			_directory = path.Substring(mediaIndex + 1);
			for(int i = 0; i < _directory.Length; i++)
			{
				if(_directory[i] == '/') finalDir = i;
			}
			if(finalDir >= 0) _directory = _directory.Substring(0, finalDir + 1);
			else _directory = string.Empty;

			int fileStart = finalDir + mediaIndex + 2;
			int extensionStart = -1;
			for(int i = fileStart; i < path.Length; i++)
			{
				if(path[i] == '.') extensionStart = i;
			}
			if(extensionStart >= 0)
			{
				_file = path.Substring(fileStart, extensionStart - fileStart);
				_extension = path.Substring(extensionStart + 1);
			}
			else
			{
				_file = path.Substring(fileStart);
				_extension = string.Empty;				
			}

			_fileExtension = string.Empty;
			if(_extension.Length > 0)
			{
				_fileExtension += '.';
				_fileExtension += _extension;
			}
			
			_path = GeneratePath(_mediaDirectory);
			_nativePath = _path.Replace('/', NativeSeperator);
			_nativePath = _nativePath.Replace('\\', NativeSeperator);
		}
		public string GeneratePath(string mediaDirectory)
		{
			string nativePath = mediaDirectory;
			int mediaLen = mediaDirectory.Length;
			if(mediaLen != 0)
			{
				nativePath = nativePath.TrimEnd('/', '\\');
			}
			nativePath += _directory;
			if(_directory.Length > 0)
			{
				char finalChar = _directory.Last();
				if(finalChar != '/' && finalChar != '\\') nativePath += '/';
			}
			nativePath += _file;
			nativePath += _fileExtension;
			nativePath = nativePath.Replace('\\', '/');
			nativePath = nativePath.TrimStart('/');
			return nativePath;
		}
		public string getNativePath() => _nativePath;
	}
}