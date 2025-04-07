/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public abstract class igStorageDevice : igFileWorkItemProcessor
	{
		public string _name;
		public string _path;
		public uint _readMediaAlignment;
		public uint _writeMediaAlignment;
		public uint _memoryAlignment;
		public uint _randomAccessTransferSize;
		public uint _sequentialTransferSize;
		public bool _readOnly;
		public bool _removableMedia;

		public abstract void Exists(igFileWorkItem workItem);
		public abstract void Open(igFileWorkItem workItem);
		public abstract void Close(igFileWorkItem workItem);
		public abstract void Read(igFileWorkItem workItem);
		public abstract void Write(igFileWorkItem workItem);
		public abstract void Truncate(igFileWorkItem workItem);
		public abstract void Mkdir(igFileWorkItem workItem);
		public abstract void Rmdir(igFileWorkItem workItem);
		public abstract void GetFileList(igFileWorkItem workItem);
		public abstract void GetFileListWithSizes(igFileWorkItem workItem);
		public abstract void Unlink(igFileWorkItem workItem);
		public abstract void Rename(igFileWorkItem workItem);
		public abstract void Prefetch(igFileWorkItem workItem);
		public abstract void Format(igFileWorkItem workItem);
		public abstract void Commit(igFileWorkItem workItem);
		public override void Process(igFileWorkItem workItem)
		{
			switch(workItem._type)
			{
				case igFileWorkItem.WorkType.kTypeExists:
					Exists(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeOpen:
					Open(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeClose:
					Close(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRead:
					Read(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeWrite:
					Write(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeTruncate:
					Truncate(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeMkdir:
					Mkdir(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRmdir:
					Rmdir(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeFileList:
					GetFileList(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeFileListWithSizes:
					GetFileListWithSizes(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeUnlink:
					Unlink(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRename:
					Rename(workItem);
					break;
				case igFileWorkItem.WorkType.kTypePrefetch:
					Prefetch(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeFormat:
					Format(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeCommit:
					Commit(workItem);
					break;
				default:
					throw new ArgumentException("Work type " + workItem._type.ToString() + " does not exist.");
			}

			SendToNextProcessor(workItem);
		}
	}
}