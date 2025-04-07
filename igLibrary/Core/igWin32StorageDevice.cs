/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Diagnostics;

namespace igLibrary.Core
{
	//This'd be called igWin32StorageDevice, igPosixStorageDevice, igPS3StorageDevice, etc but why bother
	public class igWin32StorageDevice : igPhysicalStorageDevice
	{
		private string getPath(igFileWorkItem workItem)
		{
			if(workItem._path[1] != ':')
			{
				return Path.Combine(igFileContext.Singleton._root, workItem._path);
			}
			return Path.Combine(igFileContext.Singleton._root, workItem._path);
		}
		public override void Close(igFileWorkItem workItem)
		{
			workItem._file._handle.Close();
			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}

		public override void Commit(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Exists(igFileWorkItem workItem)
		{
			if(File.Exists(getPath(workItem)))
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			else
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
		}

		public override void Format(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void GetFileList(igFileWorkItem workItem)
		{
			igStringRefList? list = workItem._buffer as igStringRefList;

			Debug.Assert(list != null);

			string[] files = Directory.GetFiles(workItem._path);

			list.SetCapacity(list._capacity + files.Length);
			for(int i = 0; i < files.Length; i++)
			{
				list.Append(files[i]);
			}

			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}

		public override void GetFileListWithSizes(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Mkdir(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Open(igFileWorkItem workItem)
		{
			FileAccess access = (FileAccess)(workItem._flags & 0b11);

			FileMode mode = (FileMode)(workItem._flags >> 8);

			try
			{
				workItem._file._handle = File.Open(getPath(workItem), mode, access);
				workItem._file._device = this;
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			catch(ArgumentOutOfRangeException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusBadParam);
			}
			catch(ArgumentException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(PathTooLongException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(DirectoryNotFoundException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(FileNotFoundException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
			}
			catch(UnauthorizedAccessException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusPermissionDenied);
			}
		}

		public override void Prefetch(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Read(igFileWorkItem workItem)
		{
			FileStream fs = (FileStream)workItem._file._handle;
			long initialOffset = fs.Position;
			try
			{
				fs.Seek((long)workItem._offset, SeekOrigin.Begin);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
				return;
			}
			catch(ArgumentException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusGeneralError);
				return;
			}
			catch(ObjectDisposedException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusStopped);
				return;
			}

  			try
			{
				fs.Write((byte[])workItem._buffer);
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			catch(ArgumentNullException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusBadParam);
			}
			catch(IOException)
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusReadError);
			}
			finally
			{
				fs.Seek(initialOffset, SeekOrigin.Begin);
			}
		}

		public override void Rename(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Rmdir(igFileWorkItem workItem)
		{
			Directory.Delete(getPath(workItem), true);
		}

		public override void Truncate(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Unlink(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}

		public override void Write(igFileWorkItem workItem)
		{
			workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
		}
	}
}